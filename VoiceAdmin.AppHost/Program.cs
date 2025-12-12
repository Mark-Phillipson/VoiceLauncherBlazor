var builder = DistributedApplication.CreateBuilder(args);

// Removed reference to Projects.VoiceLauncher (project removed)
// builder.AddProject<Projects.VoiceAdmin>("voiceadmin");

//builder.AddProject<Projects.DataAccessLibrary>("dataaccesslibrary");

builder.Build().Run();
