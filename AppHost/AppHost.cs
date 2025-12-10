var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Projects.ChaosMonkey_Web>("web")
	.WithExternalHttpEndpoints();


var tunnel = builder.AddDevTunnel("chaosmonkey-web-tunnel")
	.WithAnonymousAccess()
	.WithReference(web);

builder.Build().Run();
