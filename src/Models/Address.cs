using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Models
{
    public class Address
    {
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public long Pincode { get; set; }
    }
}