using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Models
{
    public enum FamilyRole
    {
        Husband,
        Wife,
        Father,
        GrandFather,
        Mother,
        GrandMother,
        Child,
        GrandChild,
        InLaw
    }
}