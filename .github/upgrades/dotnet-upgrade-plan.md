# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade CarMaintenanceDiary.Shared\CarMaintenanceDiary.Shared.csproj
4. Upgrade CarMaintenanceDiary.Core\CarMaintenanceDiary.Core.csproj
5. Upgrade CarMaintenanceDiary.Infrastructure\CarMaintenanceDiary.Infrastructure.csproj
6. Upgrade CarMaintenanceDiary.Application\CarMaintenanceDiary.Application.csproj
7. Upgrade CarMaintenanceDiary.Mobile\CarMaintenanceDiary.Mobile.csproj
8. Upgrade CarMaintenanceDiary.Api\CarMaintenanceDiary.Api.csproj
9. Upgrade CarMaintenanceDiary.Web\CarMaintenanceDiary.Web.csproj
10. Upgrade CarMaintenanceDiary.Tests\CarMaintenanceDiary.Tests.csproj

## Settings

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|


### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                           | Current Version | New Version | Description                                   |
|:-------------------------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer          |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.AspNetCore.Components.Authorization          |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.AspNetCore.Components.Web                    |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore      |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.AspNetCore.Identity.UI                       |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.AspNetCore.OpenApi                           |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.EntityFrameworkCore.Design                   |     9.0.4/9.0.9 |  10.0.0     | Replace with EF Core 10 design package        |
| Microsoft.EntityFrameworkCore.SqlServer                |     9.0.4       |  10.0.0     | Replace with EF Core 10 SQL Server provider   |
| Microsoft.EntityFrameworkCore.Tools                    |     9.0.4       |  10.0.0     | Replace with EF Core 10 tools package         |
| Microsoft.Extensions.Configuration                     |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Configuration.FileExtensions      |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Configuration.Json                |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Http                              |     9.0.4       |  10.0.0     | Recommended upgrade for .NET 10.0             |
| Microsoft.Extensions.Logging.Debug                     |     9.0.0       |  10.0.0     | Recommended upgrade for .NET 10.0             |


### Project upgrade details

#### CarMaintenanceDiary.Shared\CarMaintenanceDiary.Shared.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - No package updates required directly in the shared project (verify transitive deps after other projects upgraded).

Other changes:
  - Verify any obsolete APIs and fix compile errors after upgrade.


#### CarMaintenanceDiary.Core\CarMaintenanceDiary.Core.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - No direct package changes detected but run build and address any breaking changes.

Other changes:
  - Check for API changes or package compatibility issues.


#### CarMaintenanceDiary.Infrastructure\CarMaintenanceDiary.Infrastructure.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore.Design -> 10.0.0
  - Microsoft.EntityFrameworkCore.SqlServer -> 10.0.0
  - Microsoft.EntityFrameworkCore.Tools -> 10.0.0
  - Microsoft.Extensions.Configuration -> 10.0.0
  - Microsoft.Extensions.Configuration.FileExtensions -> 10.0.0
  - Microsoft.Extensions.Configuration.Json -> 10.0.0
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore -> 10.0.0

Feature upgrades:
  - Update any EF Core usage if breaking changes exist in EF Core 10.

Other changes:
  - Re-run migrations build and tests after package updates.


#### CarMaintenanceDiary.Application\CarMaintenanceDiary.Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Http -> 10.0.0

Other changes:
  - Ensure HttpClientFactory integrations remain compatible.


#### CarMaintenanceDiary.Mobile\CarMaintenanceDiary.Mobile.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0` to `net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0;net10.0-windows`

NuGet packages changes:
  - Microsoft.Extensions.Logging.Debug -> 10.0.0

Feature upgrades:
  - Add `net10.0-windows` TFM only if you need new Windows-specific APIs; validate MAUI compatibility with .NET 10.


#### CarMaintenanceDiary.Api\CarMaintenanceDiary.Api.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.OpenApi -> 10.0.0
  - Microsoft.EntityFrameworkCore.Design -> 10.0.0
  - Microsoft.AspNetCore.Authentication.JwtBearer -> 10.0.0

Other changes:
  - Ensure Startup/Program initialization patterns remain compatible. Review any obsolete APIs in ASP.NET Core 10.


#### CarMaintenanceDiary.Web\CarMaintenanceDiary.Web.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore -> 10.0.0
  - Microsoft.AspNetCore.Identity.UI -> 10.0.0
  - Microsoft.AspNetCore.Components.Web -> 10.0.0
  - Microsoft.AspNetCore.Components.Authorization -> 10.0.0

Other changes:
  - Verify Blazor Bootstrap and Radzen compatibility with .NET 10.


#### CarMaintenanceDiary.Tests\CarMaintenanceDiary.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Update test framework packages if needed after upgrading target framework.


