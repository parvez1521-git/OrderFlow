# API File Guide

Every hand-authored C# source file begins with a learning header. This guide covers files that should not receive inline comments.

- `OrderFlow.Api.csproj`: XML project definition; lists the .NET target and NuGet packages.
- `appsettings.json`: shared runtime configuration, including the LocalDB connection string.
- `appsettings.Development.json`: development-only logging overrides.
- `Properties/launchSettings.json`: local API launch profiles and ports.
- `Migrations/*.cs`: generated EF Core schema history. Do not manually edit these; regenerate with `dotnet-ef` when the model changes.
- `*.http`: manual API request file; it has comment lines because this format supports them.

`bin/` and `obj/` are generated build output and are intentionally excluded from learning comments and source control.
