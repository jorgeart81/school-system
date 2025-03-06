using System;
using Infrastructure.Tenancy;

namespace Infrastructure.OpenApi;

public class TennantHeaderAttribute()
     : SwaggerHeaderAttribute(
        headerName: TenancyConstans.TenantIdName,
        description: "Enter your tenant name to access this API",
        defaultValue: string.Empty,
        isRequired: true)
{

}
