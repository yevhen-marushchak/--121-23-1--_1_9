using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.WebAPI.Models;
using Hospital.DAL;
using Moq;

namespace Hospital.BLL.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUnitOfWork> _mockUow = null!;
        private UserService _service = null!;
        private User _testUser = null!;

        [SetUp]
        public void Setup()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _service = new UserService(_mockUow.Object);
            _testUser = new User
            {
                Id = 1,
                Login = "testuser",
                PasswordHash = HashPassword("password"),
                FirstName = "John",
                LastName = "Doe",
                RoleId = 3,
                Role = new Role { Id = 3, Name = "Patient" }
            };
        }

        private static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            return System.Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        [Test]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsUser()
        {
            _mockUow.Setup(u => u.Users.GetByLoginAsync("testuser"))
                .ReturnsAsync(_testUser);

            var result = await _service.AuthenticateAsync("testuser", "password");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Login, Is.EqualTo("testuser"));
            Assert.That(result.Role, Is.EqualTo("Patient"));
        }

        [Test]
        public async Task AuthenticateAsync_InvalidPassword_ReturnsNull()
        {
            _mockUow.Setup(u => u.Users.GetByLoginAsync("testuser"))
                .ReturnsAsync(_testUser);

            var result = await _service.AuthenticateAsync("testuser", "wrongpassword");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateAsync_UserNotFound_ReturnsNull()
        {
            _mockUow.Setup(u => u.Users.GetByLoginAsync("nonexistent"))
                .ReturnsAsync((User)null!);

            var result = await _service.AuthenticateAsync("nonexistent", "password");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RegisterAsync_UniqueLogin_CreatesUser()
        {
            var dto = new UserRegisterDto
            {
                Login = "newuser",
                Password = "password",
                FirstName = "Jane",
                LastName = "Smith"
            };

            // Налаштування послідовності викликів GetByLoginAsync
            _mockUow.SetupSequence(u => u.Users.GetByLoginAsync("newuser"))
                .ReturnsAsync((User)null!) // Перший виклик: логін не зайнятий
                .ReturnsAsync(new User // Другий виклик: повернення створеного користувача
                {
                    Id = 100,
                    Login = "newuser",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Role = new Role { Name = "Patient" }
                });

            _mockUow.Setup(u => u.Users.AnyUserWithRoleAsync("Administrator"))
                .ReturnsAsync(true);
            _mockUow.Setup(u => u.Users.GetRoleByNameAsync("Patient"))
                .ReturnsAsync(new Role { Id = 3, Name = "Patient" });
            _mockUow.Setup(u => u.Users.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => u.Id = 100)
                .Returns(Task.CompletedTask);

            var result = await _service.RegisterAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(result.Login, Is.EqualTo("newuser"));
            _mockUow.Verify(u => u.Users.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RegisterAsync_LoginTaken_ReturnsNull()
        {
            var dto = new UserRegisterDto
            {
                Login = "testuser",
                Password = "password",
                FirstName = "Jane",
                LastName = "Smith"
            };

            _mockUow.Setup(u => u.Users.GetByLoginAsync("testuser"))
                .ReturnsAsync(_testUser);

            var result = await _service.RegisterAsync(dto);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void RegisterAsync_RoleNotFound_ThrowsException()
        {
            var dto = new UserRegisterDto
            {
                Login = "newuser",
                Password = "password",
                FirstName = "Jane",
                LastName = "Smith"
            };

            _mockUow.Setup(u => u.Users.GetByLoginAsync("newuser"))
                .ReturnsAsync((User)null!);
            _mockUow.Setup(u => u.Users.AnyUserWithRoleAsync("Administrator"))
                .ReturnsAsync(true);
            _mockUow.Setup(u => u.Users.GetRoleByNameAsync("Patient"))
                .ReturnsAsync((Role)null!);

            var ex = Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(dto));
            Assert.That(ex.Message, Is.EqualTo("Role 'Patient' not found."));
        }

        [Test]
        public async Task ChangePasswordAsync_AdminRole_Success()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.ChangePasswordAsync(1, 99, "newpassword", null, "Administrator");

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.Users.Update(_testUser), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task ChangePasswordAsync_ManagerRole_Patient_Success()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.ChangePasswordAsync(1, 99, "newpassword", null, "Manager");

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.Users.Update(_testUser), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task ChangePasswordAsync_ManagerRole_NonPatient_ReturnsFalse()
        {
            var user = new User { Id = 1, Role = new Role { Name = "Administrator" } };
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(user);

            var result = await _service.ChangePasswordAsync(1, 99, "newpassword", null, "Manager");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ChangePasswordAsync_PatientSelf_WrongCurrentPassword_ReturnsFalse()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.ChangePasswordAsync(1, 1, "newpassword", "wrongpassword", "Patient");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ChangePasswordAsync_PatientSelf_ValidPassword_Success()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.ChangePasswordAsync(1, 1, "newpassword", "password", "Patient");

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.Users.Update(_testUser), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteAccountAsync_AdminRole_Success()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.DeleteAccountAsync(1, 99, "Administrator", null);

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.Users.Remove(_testUser), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteAccountAsync_PatientSelf_WrongPassword_ReturnsFalse()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.DeleteAccountAsync(1, 1, "Patient", "wrongpassword");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            var users = new List<User> { new User(), new User() };
            _mockUow.Setup(u => u.Users.GetAllWithRolesAsync())
                .ReturnsAsync(users);

            var result = await _service.GetAllUsersAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserByIdAsync_Found_ReturnsUser()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(1))
                .ReturnsAsync(_testUser);

            var result = await _service.GetUserByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Login, Is.EqualTo("testuser"));
        }

        [Test]
        public async Task GetUserByIdAsync_NotFound_ReturnsNull()
        {
            _mockUow.Setup(u => u.Users.GetByIdWithRoleAsync(99))
                .ReturnsAsync((User)null!);

            var result = await _service.GetUserByIdAsync(99);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ChangeRoleAsync_ValidData_Success()
        {
            _mockUow.Setup(u => u.Users.GetByIdAsync(1))
                .ReturnsAsync(_testUser);
            _mockUow.Setup(u => u.Users.GetRoleByNameAsync("Manager"))
                .ReturnsAsync(new Role { Id = 2, Name = "Manager" });

            var result = await _service.ChangeRoleAsync(1, "Manager");

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.Users.Update(_testUser), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task ChangeRoleAsync_RoleNotFound_ReturnsFalse()
        {
            _mockUow.Setup(u => u.Users.GetByIdAsync(1))
                .ReturnsAsync(_testUser);
            _mockUow.Setup(u => u.Users.GetRoleByNameAsync("NonExistent"))
                .ReturnsAsync((Role)null!);

            var result = await _service.ChangeRoleAsync(1, "NonExistent");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ChangeRoleAsync_UserNotFound_ReturnsFalse()
        {
            _mockUow.Setup(u => u.Users.GetByIdAsync(99))
                .ReturnsAsync((User)null!);

            var result = await _service.ChangeRoleAsync(99, "Manager");

            Assert.That(result, Is.False);
        }
    }
}