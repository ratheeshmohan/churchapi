using parishdirectoryapi.Models;

namespace parishdirectoryapi.Controllers.Models
{
    public class FamilyProfileViewModel
    {
        public Address Address { get; set; }
        public Parish HomeParish { get; set; }
        public string PhotoUrl { get; set; }
    }
}
