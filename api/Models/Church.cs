using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Models
{
    public class Church
    {
        [Required]
        public string ChurchId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Address Address { get; set; }        
        [Required]
        public string Phone { get; set; }  
        [Required]
        public string EmailId { get; set; }
    }
}