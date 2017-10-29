using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace parishdirectoryapi.Controllers.Models
{
    public class Church
    {
        [Required]
        public string ChurchId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }        
        [Required]
        public string Phone { get; set; }  
        [Required]
        public string EmailId { get; set; }
    }
}