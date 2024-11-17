var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.VoiceLauncher>("voicelauncher");

//builder.AddProject<Projects.DataAccessLibrary>("dataaccesslibrary");

builder.Build().Run();
