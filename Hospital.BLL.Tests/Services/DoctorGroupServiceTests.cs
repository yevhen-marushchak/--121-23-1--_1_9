using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.WebAPI.Models;
using Hospital.DAL;
using Moq;

namespace Hospital.BLL.Tests.Services
{
    [TestFixture]
    public class DoctorGroupServiceTests
    {
        private Mock<IUnitOfWork> _mockUow = null!;
        private DoctorGroupService _service = null!;
        private DoctorGroup _testDoctorGroup = null!;

        [SetUp]
        public void Setup()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _service = new DoctorGroupService(_mockUow.Object);
            _testDoctorGroup = new DoctorGroup
            {
                Id = 1,
                Name = "Cardiology"
            };
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllGroups()
        {
            var groups = new List<DoctorGroup> { new DoctorGroup(), new DoctorGroup() };
            _mockUow.Setup(u => u.DoctorGroups.GetAllAsync())
                .ReturnsAsync(groups);

            var result = await _service.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_Found_ReturnsGroup()
        {
            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(1))
                .ReturnsAsync(_testDoctorGroup);

            var result = await _service.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Cardiology"));
        }

        [Test]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(99))
                .ReturnsAsync((DoctorGroup)null!);

            var result = await _service.GetByIdAsync(99);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ValidData_CreatesGroup()
        {
            var dto = new DoctorGroupCreateDto
            {
                Name = "Neurology"
            };

            _mockUow.Setup(u => u.DoctorGroups.AddAsync(It.IsAny<DoctorGroup>()))
                .Callback<DoctorGroup>(g => g.Id = 100)
                .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(result.Name, Is.EqualTo("Neurology"));
            _mockUow.Verify(u => u.DoctorGroups.AddAsync(It.IsAny<DoctorGroup>()), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ValidData_UpdatesGroup()
        {
            var dto = new DoctorGroupUpdateDto
            {
                Name = "Pediatrics"
            };

            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(1))
                .ReturnsAsync(_testDoctorGroup);

            var result = await _service.UpdateAsync(1, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Pediatrics"));
            _mockUow.Verify(u => u.DoctorGroups.Update(_testDoctorGroup), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_NotFound_ReturnsNull()
        {
            var dto = new DoctorGroupUpdateDto
            {
                Name = "Pediatrics"
            };

            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(99))
                .ReturnsAsync((DoctorGroup)null!);

            var result = await _service.UpdateAsync(99, dto);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_Found_ReturnsTrue()
        {
            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(1))
                .ReturnsAsync(_testDoctorGroup);

            var result = await _service.DeleteAsync(1);

            Assert.That(result, Is.True);
            _mockUow.Verify(u => u.DoctorGroups.Remove(_testDoctorGroup), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            _mockUow.Setup(u => u.DoctorGroups.GetByIdAsync(99))
                .ReturnsAsync((DoctorGroup)null!);

            var result = await _service.DeleteAsync(99);

            Assert.That(result, Is.False);
        }
    }
}