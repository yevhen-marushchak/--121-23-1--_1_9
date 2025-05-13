using System;
using System.Threading.Tasks;
using Hospital.DAL.Entities;
using Hospital.DAL.Repositories;

namespace Hospital.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Doctor> Doctors { get; }
        IRepository<Patient> Patients { get; }
        IRepository<Appointment> Appointments { get; }
        Task<int> CompleteAsync();
    }
}