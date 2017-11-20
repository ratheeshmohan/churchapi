﻿namespace parishdirectoryapi.Security
{
    internal class AuthPolicy
    {
        public const string ChurchAdministratorPolicy = "ChurchAdministrator";

        public const string UserRoleClaimName = "custom:role";
        public const string ChurchIdClaimName = "custom:churchId";
        public const string FamilyClaimName = "custom:familyId";
        public const string EmailClaimName = "email";
    }
}
