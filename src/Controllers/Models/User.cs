namespace parishdirectoryapi.Controllers.Models
{
    public enum UserRole
    {
        Admin,
        User
    }

    public class User
    {
        public string LoginId { get; set; }
        public string FamlyId { get; set; }
        public string ChurchId { get; set; }
        public UserRole Role { get; set; }
        public string Email { get; set; }
    }
}
