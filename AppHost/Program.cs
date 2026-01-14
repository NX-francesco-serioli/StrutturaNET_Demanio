var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddContainer("db", "postgis/postgis", "16-3.4")
    .WithContainerName("adsp_mds_demaniodigitale_db_local")
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
    .WithVolume("adsp_mds_demaniodigitale_db_data_local", "/var/lib/postgresql/data");

var rabbit = builder.AddContainer("rabbitmq", "rabbitmq", "3.13-management")
    .WithContainerName("adsp_mds_demaniodigitale_rabbitmq_local")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "admin")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "Nexus2025!")
    .WithEntrypoint("sh")
    .WithArgs("-c", "rabbitmq-plugins enable --offline rabbitmq_shovel rabbitmq_shovel_management && rabbitmq-server")
    .WithEndpoint(name: "amqp", targetPort: 5672, port: 5672, isExternal: true, isProxied: false)
    .WithEndpoint(name: "ui", targetPort: 15672, scheme: "http", isExternal: true, isProxied: false)
    .WithVolume("adsp_mds_demaniodigitale_rabbitmq_data_local", "/var/lib/rabbitmq");

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
    .WithEnvironment("RabbitMq__Host", "localhost")
    .WithEnvironment("RabbitMq__Username", "admin")
    .WithEnvironment("RabbitMq__Password", "Nexus2025!")
    .WithEnvironment("RabbitMq__VirtualHost", "/")
    .WaitFor(db)
    .WaitFor(rabbit);

var worker = builder.AddProject<Projects.Worker>("worker")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment(
        "ConnectionStrings__DefaultConnection",
        "Host=localhost;Port=55432;Database=adsp_mds_demaniodigitale;Username=adspadmin;Password=Nexus2025!")
    .WithEnvironment("RabbitMq__Host", "localhost")
    .WithEnvironment("RabbitMq__Username", "admin")
    .WithEnvironment("RabbitMq__Password", "Nexus2025!")
    .WithEnvironment("RabbitMq__VirtualHost", "/")
    .WaitFor(rabbit);

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
