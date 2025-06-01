namespace Hospital.BLL
{
    public class SlotTakenException : Exception
    {
        public SlotTakenException(string message) : base(message) { }
    }

    public class InvalidAppointmentTimeException : Exception
    {
        public InvalidAppointmentTimeException(string message) : base(message) { }
    }
}
