namespace Hospital.DAL.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "Administrator", "Manager", "Patient"
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}