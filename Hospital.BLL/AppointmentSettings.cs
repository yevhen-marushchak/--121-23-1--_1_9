namespace Hospital.BLL
{
    public static class AppointmentSettings
    {
        public static readonly TimeSpan StartTime = new TimeSpan(8, 0, 0); // 8:00
        public static readonly TimeSpan EndTime = new TimeSpan(18, 30, 0); // 18:30
        public static readonly int[] AllowedMinutes = new int[] { 0, 30 }; // :00 або :30
    }
}
