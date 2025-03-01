using System;
using Finbuckle.MultiTenant.Abstractions;

namespace Infrastructure.Tenancy;

public class SchoolSystemTenantInfo : ITenantInfo
{
    public string? Id { get; set; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public string? ConnectionString { get; set; }
    public string? Email { get; set; }
    public string? FirsName { get; set; }
    public string? LastName { get; set; }
    public DateTime ValidUpTo { get; set; }
    public bool IsActive { get; set; }
}
