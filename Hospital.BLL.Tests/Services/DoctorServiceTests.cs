using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.WebAPI.Models;
using Hospital.DAL;
using Moq;

namespace Hospital.BLL.Tests.Services
{
    [TestFixture]
    public class DoctorServiceTests
    {
        private Mock<IUnitOfWork> _mockUow = null!;
        private DoctorService _service = null!;
        private Doctor _testDoctor = null!;

        [SetUp]
        public void Setup()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _service = new DoctorService(_mockUow.Object);
            _testDoctor = new Doctor
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Description = "Cardiologist",
                GroupId = 1,
                Group = new DoctorGroup { Id = 1, Name = "Cardiology" }
            };
        }

        [Test]
        public void CreateAsync_GroupNotFound_ThrowsException()
        {
            var dto = new DoctorCreateDto { GroupId = 99 };
            _mockUow.Setup(uow => uow.DoctorGroups.GetByIdAsync(99))
                .ReturnsAsync((DoctorGroup)null!);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateAsync(dto));
            Assert.That(ex.Message, Is.EqualTo("Doctor group not found"));
        }

        [Test]
        public void UpdateAsync_GroupNotFound_ThrowsException()
        {
            var doctor = new Doctor { Id = 1 };
            var dto = new DoctorUpdateDto { GroupId = 99 };

            _mockUow.Setup(uow => uow.Doctors.GetByIdAsync(1))
                .ReturnsAsync(doctor);

            _mockUow.Setup(uow => uow.DoctorGroups.GetByIdAsync(99))
                .ReturnsAsync((DoctorGroup)null!);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(1, dto));
            Assert.That(ex.Message, Is.EqualTo("Doctor group not found"));
        }

        [Test]
        public async Task SearchAsync_FiltersApplied_ReturnsFilteredResults()
        {
            var doctors = new List<Doctor>
            {
                new Doctor { LastName = "Smith", Group = new DoctorGroup { Name = "Cardiology" } },
                new Doctor { LastName = "Johnson", Group = new DoctorGroup { Name = "Neurology" } }
            };

            _mockUow.Setup(uow => uow.Doctors.SearchAsync("Cardiology", "Smith", null))
                .ReturnsAsync(doctors.Where(d =>
                    d.Group != null &&
                    d.Group.Name == "Cardiology" &&
                    d.LastName == "Smith").ToList());

            var result = await _service.SearchAsync("Cardiology", "Smith", null);

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllDoctors()
        {
            var doctors = new List<Doctor> { new Doctor(), new Doctor() };
            _mockUow.Setup(u => u.Doctors.GetAllAsync())
                .ReturnsAsync(doctors);

            var result = await _service.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetByGroupAsync_ReturnsGroupDoctors()
        {
            var doctors = new List<Doctor> { new Doctor(), new Doctor() };
            _mockUow.Setup(u => u.Doctors.GetByGroupAsync(1))
                .ReturnsAsync(doctors);

            var result = await _service.GetByGroupAsync(1);

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_Found_ReturnsDoctor()
        {
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(1))
                .ReturnsAsync(_testDoctor);

            var result = await _service.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.LastName, Is.EqualTo("Doe"));
            Assert.That(result.GroupName, Is.EqualTo("Cardiology"));
        }

        [Test]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(99))
                .ReturnsAsync((Doctor)null!);

            var result = await _service.GetByIdAsync(99);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ValidData_CreatesDoctor()
        {
            var dto = new DoctorCreateDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                Description = "Neurologist",
                GroupId = 2
            };

            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(2))
                .ReturnsAsync(new DoctorGroup());
            _mockUow.Setup(u => u.Doctors.AddAsync(It.IsAny<Doctor>()))
                .Callback<Doctor>(d =>
                {
                    d.Id = 100;
                })
                .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(result.LastName, Is.EqualTo("Smith"));
            _mockUow.Verify(u => u.Doctors.AddAsync(It.IsAny<Doctor>()), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ValidData_UpdatesDoctor()
        {
            var dto = new DoctorUpdateDto
            {
                FirstName = "James",
                LastName = "Brown",
                Description = "Updated description",
                GroupId = 3
            };

            _mockUow.Setup(u => u.Doctors.GetByIdAsync(1))
                .ReturnsAsync(_testDoctor);
            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(3))
                .ReturnsAsync(new DoctorGroup { Id = 3, Name = "Pediatrics" });

            var result = await _service.UpdateAsync(1, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FirstName, Is.EqualTo("James"));
            Assert.That(result.LastName, Is.EqualTo("Brown"));
            Assert.That(result.Description, Is.EqualTo("Updated description"));
            Assert.That(result.GroupId, Is.EqualTo(3));
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_PartialData_UpdatesDoctor()
        {
            var dto = new DoctorUpdateDto
            {
                LastName = "Taylor"
            };

            _mockUow.Setup(u => u.Doctors.GetByIdAsync(1))
                .ReturnsAsync(_testDoctor);

            var result = await _service.UpdateAsync(1, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FirstName, Is.EqualTo("John"));
            Assert.That(result.LastName, Is.EqualTo("Taylor"));
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_NotFound_ReturnsNull()
        {
            var dto = new DoctorUpdateDto();
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(99))
                .ReturnsAsync((Doctor)null!);

            var result = await _service.UpdateAsync(99, dto);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_Found_ReturnsTrue()
        {
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(1))
                .ReturnsAsync(_testDoctor);

            var result = await _service.DeleteAsync(1);

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.Doctors.Remove(_testDoctor), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(99))
                .ReturnsAsync((Doctor)null!);

            var result = await _service.DeleteAsync(99);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SearchAsync_EmptyFilters_ReturnsAll()
        {
            var doctors = new List<Doctor> { new Doctor(), new Doctor() };
            _mockUow.Setup(u => u.Doctors.SearchAsync(null, null, null))
                .ReturnsAsync(doctors);

            var result = await _service.SearchAsync(null, null, null);

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SearchAsync_FirstNameFilter_ReturnsFiltered()
        {
            var doctors = new List<Doctor>
            {
                new Doctor { FirstName = "John", LastName = "Doe" },
                new Doctor { FirstName = "Jane", LastName = "Smith" }
            };

            _mockUow.Setup(u => u.Doctors.SearchAsync(null, null, "John"))
                .ReturnsAsync(doctors.Where(d => d.FirstName == "John").ToList());

            var result = await _service.SearchAsync(null, null, "John");

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FirstName, Is.EqualTo("John"));
        }

        [Test]
        public async Task SearchAsync_MultipleFilters_ReturnsFiltered()
        {
            var doctors = new List<Doctor>
            {
                new Doctor
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Group = new DoctorGroup { Name = "Cardiology" }
                },
                new Doctor
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Group = new DoctorGroup { Name = "Neurology" }
                }
            };

            _mockUow.Setup(u => u.Doctors.SearchAsync("Cardiology", "Doe", "John"))
                .ReturnsAsync(doctors.Where(d =>
                    d.Group?.Name == "Cardiology" &&
                    d.LastName == "Doe" &&
                    d.FirstName == "John").ToList());

            var result = await _service.SearchAsync("Cardiology", "Doe", "John");

            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
