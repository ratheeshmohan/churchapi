using parishdirectoryapi.Controllers.Models;

namespace parishdirectoryapi.Security
{
    public class UserContext
    {
        public string FamilyId { get; set; }
        public string LoginId { get; set; }
        public string ChurchId { get; set; }

        public UserRole Role { get; set; }

        public override string ToString()
        {
            return $"ChurchId = {ChurchId} FamilyId = {FamilyId} " +
            $"LoginId = {LoginId} Role={Role}";
        }
    }
}