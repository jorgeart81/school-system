using System;
using System.Collections.ObjectModel;

namespace Infrastructure.Constants;

public static class SchoolAction
{
    public const string Read = nameof(Read);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class SchoolFeature
{
    public const string Tenants = nameof(Tenants);
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string UserRoles = nameof(UserRoles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Schools = nameof(Schools);
}

public record SchoolPermission(string Action, string Feature, string Description, string Group, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Feature);
    public static string NameFor(string action, string feature) => $"Permission.{feature}.{action}";
}

public static class SchoolPermissions
{
    private const string _academics = "Academics";
    private const string _systemAccess = "SystemAccess";
    private const string _tenancy = "Tenancy";
    private static readonly SchoolPermission[] _allPermissions = [
        new SchoolPermission(SchoolAction.Read,SchoolFeature.Tenants, "Read Tenants", _tenancy, IsRoot:true),
        new SchoolPermission(SchoolAction.Create,SchoolFeature.Tenants, "Create Tenants", _tenancy, IsRoot:true),
        new SchoolPermission(SchoolAction.Update,SchoolFeature.Tenants, "Update Tenants", _tenancy, IsRoot:true),
        new SchoolPermission(SchoolAction.UpgradeSubscription,SchoolFeature.Tenants, "Upgrade Tenant's Subscription", _tenancy, IsRoot:true),

        new SchoolPermission(SchoolAction.Read,SchoolFeature.Users, "Read Users", _systemAccess),
        new SchoolPermission(SchoolAction.Create,SchoolFeature.Users, "Create Users", _systemAccess),
        new SchoolPermission(SchoolAction.Update,SchoolFeature.Users, "Update Users", _systemAccess),
        new SchoolPermission(SchoolAction.Delete,SchoolFeature.Users, "Delete Users", _systemAccess),

        new SchoolPermission(SchoolAction.Read,SchoolFeature.UserRoles, "Create User Roles", _systemAccess),
        new SchoolPermission(SchoolAction.Update,SchoolFeature.UserRoles, "Create User Roles", _systemAccess),

        new SchoolPermission(SchoolAction.Read,SchoolFeature.Roles, "Read Roles", _systemAccess),
        new SchoolPermission(SchoolAction.Create,SchoolFeature.Roles, "Create Roles", _systemAccess),
        new SchoolPermission(SchoolAction.Update,SchoolFeature.Roles, "Update Roles", _systemAccess),
        new SchoolPermission(SchoolAction.Delete,SchoolFeature.Roles, "Delete Roles", _systemAccess),

        new SchoolPermission(SchoolAction.Read,SchoolFeature.RoleClaims, "Read Role Claims/Permissions", _systemAccess),
        new SchoolPermission(SchoolAction.Update,SchoolFeature.RoleClaims, "Update Role Claims/Permissions", _systemAccess),

        new SchoolPermission(SchoolAction.Read,SchoolFeature.Schools, "Read Schools", _academics),
        new SchoolPermission(SchoolAction.Create,SchoolFeature.Schools, "Create Schools", _academics, IsBasic:true),
        new SchoolPermission(SchoolAction.Update,SchoolFeature.Schools, "Update Schools", _academics),
        new SchoolPermission(SchoolAction.Delete,SchoolFeature.Schools, "Delete Schools", _academics),
    ];

    public static IReadOnlyList<SchoolPermission> All { get; }
        = new ReadOnlyCollection<SchoolPermission>(_allPermissions);

    public static IReadOnlyList<SchoolPermission> Root { get; }
         = new ReadOnlyCollection<SchoolPermission>(_allPermissions.Where(p => p.IsRoot).ToArray());

    public static IReadOnlyList<SchoolPermission> Admin { get; }
         = new ReadOnlyCollection<SchoolPermission>(_allPermissions.Where(p => !p.IsRoot).ToArray());

    public static IReadOnlyList<SchoolPermission> Basic { get; }
         = new ReadOnlyCollection<SchoolPermission>(_allPermissions.Where(p => p.IsBasic).ToArray());
}