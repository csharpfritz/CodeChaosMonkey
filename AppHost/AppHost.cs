var builder = DistributedApplication.CreateBuilder(args);

var ghUser= builder.AddParameterFromConfiguration("GithubOwner", "Parameters:GitHub:Owner");
var ghRepo= builder.AddParameterFromConfiguration("GithubRepository", "Parameters:GitHub:Repository");
var ghToken = builder.AddParameterFromConfiguration("GitHubToken", "Parameters:GitHub:Token", true);


var web = builder.AddProject<Projects.ChaosMonkey_Web>("web")
	.WithEnvironment("GitHub:Owner", ghUser)
	.WithEnvironment("GitHub:Repository", ghRepo)
	.WithEnvironment("GitHub:Token", ghToken)
	.WithEnvironment("ChaosMonkey:MinimulDonationThreshold", "Parameters:ChaosMonkey:MinimulDonationThreshold")
	.WithExternalHttpEndpoints();


var tunnel = builder.AddDevTunnel("chaosmonkey-web-tunnel")
	.WithAnonymousAccess()
	.WithReference(web);

builder.Build().Run();
