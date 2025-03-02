using System;

namespace Infrastructure.Tenancy;

public class TenancyConstans
{
    public const string TenantIdName = "tenant";
    public const string DefautlPassword = "P@s50rd@123";
    public const string FirsName = "Admin";
    public const string LastName = "Root";

    public static class Root
    {
        public const string Id = "root";
        public const string Name = "Root";
        public const string Email = "admin.root@schoolsystem.com";
    }
}
