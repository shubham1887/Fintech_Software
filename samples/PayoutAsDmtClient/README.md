# Payout as DMT — C# reference client

A dependency-free client for `POST /api/v1/dmt/transfer` on a payout-as-DMT platform: one signed
call moves the money, with no sender registration, no beneficiary registration and no OTP.

| File | What it is |
| --- | --- |
| `FinocoreDmtClient.cs` | The client. **This is the deliverable** — copy this one file into your project |
| `Program.cs` | A runnable example and the offline self-test |
| `PayoutAsDmtClient.csproj` | Only so the example runs on its own. Your project does not need it |

## Run the self-test first

No credentials, no network. It reproduces the worked example from the integration guide:

```
dotnet run -- --selftest
```

`MATCH` means your copy of the signing code is correct. Run it again the day signatures start
failing — it separates a signing bug from a credential or clock problem in one second.

## Send a real transfer

```
dotnet run -- --base https://api.finocore.net --mobile 9000012345 --token YOUR_TOKEN \
              --sendermobile 9876543210 --account 50100234567890 --ifsc HDFC0000123 \
              --name "Sunita Devi" --amount 100
```

Every argument also reads from the environment (`FINO_BASE`, `FINO_MOBILE`, `FINO_TOKEN`,
`FINO_SENDERMOBILE`, `FINO_ACCOUNT`, `FINO_IFSC`, `FINO_NAME`, `FINO_AMOUNT`, `FINO_LAT`,
`FINO_LONG`), so the token never has to sit in shell history.

## This project needs no Docker

If you created the project yourself in Visual Studio and left **"Enable Container Support"**
(older wizards: "Enable Docker Support") ticked, the build fails before it compiles anything:

```
Error CTC1002  Visual Studio container tools require Docker Desktop.
               …\microsoft.visualstudio.azure.containers.tools.targets\…\Container.targets
```

That is the wizard's choice, not this client's. Nothing here is containerised. Remove the three
things the wizard added to your `.csproj`:

```xml
<!-- delete these two properties -->
<DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
<DockerfileContext>.</DockerfileContext>

<!-- delete this whole ItemGroup -->
<ItemGroup>
  <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.22.1" />
</ItemGroup>
```

Then delete the `Dockerfile` and `.dockerignore` the wizard dropped in the project folder, and do
**Build → Clean Solution** before rebuilding — the stale targets stay loaded until you do.

A working `.csproj` is exactly this, and nothing more:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

Simplest route of all: do not create a project for this. Drop `FinocoreDmtClient.cs` into the
application you already have.

## Requirements

.NET 8 or later (the supplied project targets `net9.0`; the client itself uses nothing newer than
.NET 6). `dotnet build` from a terminal works without Visual Studio.
