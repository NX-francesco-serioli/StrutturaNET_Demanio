var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddContainer("db", "postgis/postgis", "16-3.4")
    .WithEnvironment("POSTGRES_USER", "adspadmin")
    .WithEnvironment("POSTGRES_PASSWORD", "Nexus2025!")
    .WithEnvironment("POSTGRES_DB", "adsp_mds_demaniodigitale")
    .WithEndpoint(
        targetPort: 5432,
        port: 55432,
        scheme: "tcp",
        name: "postgres",
        isExternal: true,
        isProxied: false)
    .WithVolume("adsp_mds_data", "/var/lib/postgresql/data");

var api = builder.AddProject<Projects.Api>("api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment(
        "ConnectionStrings__DefaultConnection",
        "Host=localhost;Port=55432;Database=adsp_mds_demaniodigitale;Username=adspadmin;Password=Nexus2025!")
    .WithEnvironment("Jwt__Key", "5n9kV1Aq3Lx2Rz7Hf4Qp8sDd0Jt6eKcN")
    .WithEnvironment("Jwt__Issuer", "identity.demaniodigitale.adspmaredisardegna.it")
    .WithEnvironment("Jwt__Audience", "demaniodigitale.adspmaredisardegna.it")
    .WithEnvironment("Seed__AdminEmail", "admin@local.test")
    .WithEnvironment("Seed__AdminPassword", "Nexus2025!")
    .WaitFor(db);

builder.AddExecutable("frontend", "npm", "../frontend", "run", "start:local_https")
    .WithEnvironment("NODE_ENV", "development")
    .WithEndpoint(
        targetPort: 4200,
        port: 4200,
        scheme: "https",
        name: "frontend-https",
        isExternal: true,
        isProxied: false)
    .WaitFor(db)
    .WaitFor(api);

builder.Build().Run();
