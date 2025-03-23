# script-deploy-tools-sources
Sources for library script-deploy-tools:
- ScriptDeployTools.Sources.Embedded - ![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Tsaritsin/script-deploy-tools-sources/tagged.yml) - store scripts like EmbeddedResources

## Define script

Used class (implementation of IScript) for describe script's properties:
```csharp
internal record DeviceTypes() : ScriptBase("DEVICE_TYPES")
{
    public override string DependsOn => "IDENTITY_COMMON";

    public override string Source => "SqlServerDeploy.Scripts.v1_0_0.Tables.DeviceTypes.sql";
}
```
Property Source is resource name

## Used
```csharp
var deployService = new DeployBuilder()
    .AddLogger(loggerFactory.CreateLogger<IDeploymentService>())
    .AddOptions(new DeploymentOptions
    {
        InsertMigrationScript = scripts.FirstOrDefault(x =>
            x is { IsService: true, ScriptKey: "INSERT_MIGRATION" })
    })
    .FromEmbeddedResources(options =>
    {
        options.Assemblies = [typeof(DeployHelper).Assembly];
    })
    .ToSqlServer(options =>
    {
        options.ConnectionString = connectionString;
        options.GetDeployedInfoScript = scripts.FirstOrDefault(x =>
            x is { IsService: true, ScriptKey: "GET_DEPLOYED_SCRIPTS" });
    })
    .Build();

await deployService.Deploy(scripts, cancellationToken);
```
A more detailed example is available [in this repository](https://github.com/Tsaritsin/script-deploy-tools/tree/main/Samples/SqlServerDeploy).
