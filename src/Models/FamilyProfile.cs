using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace parishdirectoryapi.Models
{
    public class FamilyOverview
    {
        public string ChurchId { get; set; }
        public string FamilyId { get; set; }
       
    }
}