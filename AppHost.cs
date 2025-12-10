#:project ChaosMonkey.Web

var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Projects.ChaosMonkey.Web>("web")
	.WithExternalHttpEndpoint();

builder.Build().Run();
