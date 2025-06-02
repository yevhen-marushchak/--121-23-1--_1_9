using Hospital.DAL;
using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.WebAPI.Models;
using Moq;

namespace Hospital.BLL.Tests.Services
{
    [TestFixture]
    public class AppointmentServiceTests
    {
        private Mock<IUnitOfWork> _mockUow = null!;
        private AppointmentService _service = null!;
        private Appointment _testAppointment = null!;

        [SetUp]
        public void Setup()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _service = new AppointmentService(_mockUow.Object);
            _testAppointment = new Appointment { Id = 1, PatientId = 2, DoctorId = 3 };
        }

        [Test]
        public void CreateAsync_PastDate_ThrowsInvalidAppointmentTimeException()
        {
            var dto = new AppointmentCreateDto
            {
                DoctorId = 1,
                Date = DateTime.Today.AddDays(-1),
                Time = new TimeSpan(10, 0, 0)
            };

            var ex = Assert.ThrowsAsync<InvalidAppointmentTimeException>(() =>
                _service.CreateAsync(dto, 1));
            Assert.That(ex!.Message, Is.EqualTo("Cannot book appointments in the past."));
        }

        [Test]
        public void CreateAsync_InvalidTime_ThrowsInvalidAppointmentTimeException()
        {
            var dto = new AppointmentCreateDto
            {
                DoctorId = 1,
                Date = DateTime.Today,
                Time = new TimeSpan(19, 0, 0)
            };

            var ex = Assert.ThrowsAsync<InvalidAppointmentTimeException>(() =>
                _service.CreateAsync(dto, 1));
            Assert.That(ex!.Message, Is.EqualTo("Invalid appointment time. Only :00 or :30 from 8:00 to 18:30 allowed."));
        }

        [Test]
        public void CreateAsync_DoctorNotFound_ThrowsException()
        {
            var dto = new AppointmentCreateDto
            {
                DoctorId = 99,
                Date = DateTime.Today,
                Time = new TimeSpan(10, 0, 0)
            };

            _mockUow.Setup(uow => uow.Doctors.GetByIdAsync(99))
                .ReturnsAsync((Doctor)null!);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateAsync(dto, 1));
            Assert.That(ex!.Message, Is.EqualTo("Doctor not found."));
        }

        [Test]
        public async Task GetByIdAsync_PatientAccessDenied_ReturnsNull()
        {
            var appointment = new Appointment { Id = 1, PatientId = 2 };
            _mockUow.Setup(uow => uow.Appointments.GetByIdAsync(1))
                .ReturnsAsync(appointment);

            var result = await _service.GetByIdAsync(1, 1, "Patient");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_NotOwnerPatient_ReturnsNull()
        {
            var appointment = new Appointment { Id = 1, PatientId = 2 };
            var dto = new AppointmentUpdateDto();

            _mockUow.Setup(uow => uow.Appointments.GetByIdAsync(1))
                .ReturnsAsync(appointment);

            var result = await _service.UpdateAsync(1, dto, 1, "Patient");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_NotOwnerPatient_ReturnsFalse()
        {
            var appointment = new Appointment { Id = 1, PatientId = 2 };
            _mockUow.Setup(uow => uow.Appointments.GetByIdAsync(1))
                .ReturnsAsync(appointment);

            var result = await _service.DeleteAsync(1, 1, "Patient");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetAppointmentsByDoctorAsync_WithDate_ReturnsFilteredAppointments()
        {
            var date = DateTime.Today;
            var appointments = new List<Appointment> { new Appointment() };
            _mockUow.Setup(u => u.Appointments.GetByDoctorAsync(1, date))
                .ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByDoctorAsync(1, date);

            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public async Task GetAppointmentsByDoctorAsync_WithoutDate_ReturnsAllAppointments()
        {
            var appointments = new List<Appointment> { new Appointment() };
            _mockUow.Setup(u => u.Appointments.GetByDoctorAsync(1, null))
                .ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByDoctorAsync(1);

            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public async Task GetAppointmentsByPatientAsync_ReturnsAppointments()
        {
            var appointments = new List<Appointment> { new Appointment() };
            _mockUow.Setup(u => u.Appointments.GetByPatientAsync(1))
                .ReturnsAsync(appointments);

            var result = await _service.GetAppointmentsByPatientAsync(1);

            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public async Task GetByIdAsync_PatientOwner_ReturnsAppointment()
        {
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(_testAppointment);

            var result = await _service.GetByIdAsync(1, 2, "Patient");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_DoctorRole_ReturnsAppointment()
        {
            _testAppointment.DoctorId = 3;
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(_testAppointment);

            var result = await _service.GetByIdAsync(1, 3, "Doctor");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_AdminRole_ReturnsAppointment()
        {
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(_testAppointment);

            var result = await _service.GetByIdAsync(1, 99, "Admin");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync((Appointment)null!);

            var result = await _service.GetByIdAsync(1, 1, "Patient");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ValidData_CreatesAppointment()
        {
            var dto = new AppointmentCreateDto
            {
                DoctorId = 1,
                Date = DateTime.Today.AddDays(1),
                Time = new TimeSpan(10, 0, 0)
            };

            _mockUow.Setup(u => u.Doctors.GetByIdAsync(1))
                .ReturnsAsync(new Doctor());
            _mockUow.Setup(u => u.Appointments.IsSlotTakenAsync(1, It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(false);

            var result = await _service.CreateAsync(dto, 1);

            Assert.That(result, Is.Not.Null);
            _mockUow.Verify(u => u.Appointments.AddAsync(It.IsAny<Appointment>()), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void CreateAsync_SlotTaken_ThrowsSlotTakenException()
        {
            var dto = new AppointmentCreateDto
            {
                DoctorId = 1,
                Date = DateTime.Today,
                Time = new TimeSpan(10, 0, 0)
            };

            _mockUow.Setup(u => u.Doctors.GetByIdAsync(1))
                .ReturnsAsync(new Doctor());
            _mockUow.Setup(u => u.Appointments.IsSlotTakenAsync(1, dto.Date, dto.Time))
                .ReturnsAsync(true);

            var ex = Assert.ThrowsAsync<SlotTakenException>(() =>
                _service.CreateAsync(dto, 1));
            Assert.That(ex!.Message, Is.EqualTo("This time slot is already taken."));
        }

        [Test]
        public async Task UpdateAsync_PatientOwner_SuccessfullyUpdates()
        {
            var dto = new AppointmentUpdateDto { Date = DateTime.Today.AddDays(1) };
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(_testAppointment);
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Doctor());
            _mockUow.Setup(u => u.Appointments.IsSlotTakenAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(false);

            var result = await _service.UpdateAsync(1, dto, 2, "Patient");

            Assert.That(result, Is.Not.Null);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_AdminRole_SuccessfullyUpdates()
        {
            var dto = new AppointmentUpdateDto { Date = DateTime.Today.AddDays(1) };
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(_testAppointment);
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Doctor());
            _mockUow.Setup(u => u.Appointments.IsSlotTakenAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(false);

            var result = await _service.UpdateAsync(1, dto, 99, "Admin");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void UpdateAsync_PastDate_ThrowsInvalidAppointmentTimeException()
        {
            var dto = new AppointmentUpdateDto { Date = DateTime.Today.AddDays(-1) };
            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(_testAppointment);

            var ex = Assert.ThrowsAsync<InvalidAppointmentTimeException>(() =>
                _service.UpdateAsync(1, dto, 2, "Patient"));
            Assert.That(ex!.Message, Is.EqualTo("Cannot move appointment to date in the past."));
        }

        [Test]
        public void UpdateAsync_InvalidTime_ThrowsInvalidAppointmentTimeException()
        {
            var appointment = new Appointment
            {
                Id = 1,
                PatientId = 2,
                DoctorId = 1,
                Date = DateTime.Today.AddDays(1),
                Time = new TimeSpan(10, 0, 0)
            };

            var dto = new AppointmentUpdateDto
            {
                Date = DateTime.Today.AddDays(2),
                Time = new TimeSpan(19, 0, 0) // некоректний час
            };

            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                .ReturnsAsync(appointment);
            _mockUow.Setup(u => u.Doctors.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Doctor());
            _mockUow.Setup(u => u.Appointments.IsSlotTakenAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<InvalidAppointmentTimeException>(() =>
                _service.UpdateAsync(1, dto, 2, "Patient"));

            Assert.That(ex!.Message, Is.EqualTo("Invalid appointment time. Only :00 or :30 from 8:00 to 18:30 allowed."));
        }

        [Test]
        public async Task UpdateAsync_SlotTaken_ThrowsSlotTakenException()
        {
            var dto = new AppointmentUpdateDto
            {
                Date = DateTime.Today.AddDays(1),
                Time = new TimeSpan(10, 0, 0)
            };

            var appointment = new Appointment
            {
                Id = 1,
                PatientId = 2,
                DoctorId = 3,
                Date = DateTime.Today,
                Time = new TimeSpan(9, 0, 0)
            };

            _mockUow.Setup(u => u.Appointments.GetByIdAsync(1))
                    .ReturnsAsync(appointment);

            _mockUow.Setup(u => u.Doctors.GetByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new Doctor());

            _mockUow.Setup(u => u.Appointments.IsSlotTakenAsync(
                    appointment.DoctorId,
                    dto.Date!.Value,
                    dto.Time!.Value))
                    .ReturnsAsync(true);

            var ex = Assert.ThrowsAsync<SlotTakenException>(async () =>
                await _service.UpdateAsync(1, dto, 2, "Patient"));

            Assert.That(ex!.Message, Is.EqualTo("This time slot is already taken."));
        }
    }
}
