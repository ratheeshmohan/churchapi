using System.Collections.Generic;
using parishdirectoryapi.Models;

namespace parishdirectoryapi.Controllers.Models
{
    public class FamilyViewModel
    {
        public Address Address { get; set; }
        public Parish HomeParish { get; set; }
        public string PhotoUrl { get; set; }
        public IEnumerable<MemberViewModel> Members { get; set; }

        /*  public MemberViewModel Husband { get; set; }
          public MemberViewModel Wife { get; set; }

          public IEnumerable<MemberViewModel> Parents { get; set; }
          public IEnumerable<MemberViewModel> GrandParents { get; set; }
          public IEnumerable<MemberViewModel> InLaws { get; set; }
          public IEnumerable<MemberViewModel> Childrens { get; set; }
          public IEnumerable<MemberViewModel> GrandChildrens { get; set; }*/
    }
}