// dotnet cake build.cake --target=GitVersion --verbosity=Verbose
// dotnet cake build.cake --target=Build --verbosity=Verbose --skiptest
// dotnet cake build.cake --target=PublishLocalPackages --verbosity=Verbose --skiptest --configuration=Debug



///////////////////////////////////////////////////////////////////////////////
// AddIns
///////////////////////////////////////////////////////////////////////////////
#addin "nuget:?package=Cake.Docker&version=0.10.0"
#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"
#addin "nuget:?package=Newtonsoft.Json&version=11.0.2"
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"

///////////////////////////////////////////////////////////////////////////////
// Params
///////////////////////////////////////////////////////////////////////////////
var target              = Argument<string>("target", "Default");
var configuration       = Argument<string>("configuration", "Release");

var isDryRun            = HasArgument("dryrun1");

var skipTest            = HasArgument("skiptest");

var nugetApiKey         = Argument("nugetapikey", EnvironmentVariable("NUGET_API_KEY") );
var mygetApiKey         = Argument("mygetapikey", EnvironmentVariable("MYGET_API_KEY") );

///////////////////////////////////////////////////////////////////////////////
// Variables
///////////////////////////////////////////////////////////////////////////////
var tfsBuild            = HasEnvironmentVariable("TF_BUILD");
var artifactsDirectory  = Directory("./artifacts");
var isWindows           = IsRunningOnWindows();
var solutionFile        = File("./SoftwarePioniere.Fx.sln");
GitVersion gitVersion;


///////////////////////////////////////////////////////////////////////////////
// Setup
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{

    if (tfsBuild) {
        GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = false,
            OutputType = GitVersionOutput.BuildServer
        });
    }   

    gitVersion = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = false
    });

    Information("Version: {0}" , GetVersion() );
    Information("AssemblyVersion: {0}" , GetAssemblyVersion() );
});

///////////////////////////////////////////////////////////////////////////////
// GitVersion
///////////////////////////////////////////////////////////////////////////////
Task("GitVersion")
    .IsDependentOn("Clean")
    .Does( () => {

    var json = Newtonsoft.Json.JsonConvert.SerializeObject(gitVersion);

    System.IO.File.WriteAllText("./GitVersion.json", json); 
    CopyFiles("./GitVersion.json", artifactsDirectory );
});


///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { artifactsDirectory });
});

///////////////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("GitVersion") 
    .Does(() =>
{
    
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        // NoRestore = true,
        EnvironmentVariables = new Dictionary<string, string> {
            { "NuGetVersionV2", GetVersion() },
            { "AssemblySemVer", GetAssemblyVersion() }
        }
    };

    Information("Starting DotNetCoreBuild on Solution: {0}", solutionFile.Path.FullPath);
    if (!isDryRun) {
            DotNetCoreBuild( solutionFile.Path.FullPath, settings);
        }
        else {
            Verbose("Dry Run, skipping Build");      
    }

});

///////////////////////////////////////////////////////////////////////////////

Task("Test")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("GitVersion") 
    .WithCriteria(!skipTest)
    .Does(() =>
{

    // StopTestEnv();
    StartTestEnv();

    var settings = new DotNetCoreTestSettings
    {
        Configuration = configuration,
        EnvironmentVariables = new Dictionary<string, string> {
            { "NuGetVersionV2", GetVersion() },
            { "AssemblySemVer", GetAssemblyVersion() }
        },
        NoBuild = true,
        NoRestore = true,
        Logger = "trx"
    };

    var projects = GetFiles("./**/test/**/*.csproj");
    foreach(var project in projects)
    {
        Information("Starting DotNetCoreTest on Project: {0}", project.GetDirectory().FullPath);

         if (!isDryRun) {
            DotNetCoreTest(project.FullPath, settings);
        }
        else {
            Verbose("Dry Run, skipping Build");
        }
    }
})
.Finally(() =>
{
    StopTestEnv();
});

///////////////////////////////////////////////////////////////////////////////

Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("GitVersion")  
    .Does( () => {

    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        EnvironmentVariables = new Dictionary<string, string> {
            { "NuGetVersionV2", GetVersion() },
            { "AssemblySemVer", GetAssemblyVersion() }
        },
        NoBuild = true,
        NoRestore = true,
        IncludeSymbols = true,
        IncludeSource = true,
        OutputDirectory = artifactsDirectory + Directory("packages")
    };

    Information("Starting DotNetCorePack");

        if (!isDryRun) {
        DotNetCorePack(solutionFile, settings);
    }
    else {
        Verbose("Dry Run, skipping Build");
    }

});

///////////////////////////////////////////////////////////////////////////////

Task("PublishLocalPackages")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("GitVersion")  
    .IsDependentOn("Pack") 
    .Does( () => {

    var outDir = Directory(@"c:/temp/packages-debug");

    if (!DirectoryExists(outDir)) {
        CreateDirectory(outDir);
    }

    var dir = MakeAbsolute(outDir).FullPath;
    Information("OutDir: {0}", dir);
    
    var settings = new DotNetCoreNuGetPushSettings {
        Source = dir
    };

    var pkgs = GetFiles($"{artifactsDirectory.Path.FullPath}/packages/**/*.symbols.nupkg");

    foreach(var pk in pkgs)
    {
        Information("Starting DotNetCoreNuGetPush on Package: {0}", pk.FullPath);

        if (!isDryRun) {
            DotNetCoreNuGetPush(pk.FullPath, settings);
        } else {
            Verbose("Dry Run, skipping DotNetCoreNuGetPush");
        }
    }

});

///////////////////////////////////////////////////////////////////////////////

Task("PublishPackages")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("GitVersion")  
    .IsDependentOn("Pack") 
    .Does( () => {

  
    var settings = new DotNetCoreNuGetPushSettings {
        Source = "https://api.nuget.org/v3/index.json",
        ApiKey = nugetApiKey
    };


    if (gitVersion.BranchName == "dev") {
        Verbose("dev branch, publish to azure devops");

        settings.Source = "https://www.myget.org/F/softwarepioniere/api/v3/index.json";
        settings.ApiKey = mygetApiKey;
    }

    var pkgs = GetFiles($"{artifactsDirectory.Path.FullPath}/packages/**/*.symbols.nupkg");

    foreach(var pk in pkgs)
    {
        Information("Starting DotNetCoreNuGetPush on Package: {0}", pk.FullPath);

        if (!isDryRun) {
            DotNetCoreNuGetPush(pk.FullPath, settings);
        } else {
            Verbose("Dry Run, skipping DotNetCoreNuGetPush");
        }
    }

});

///////////////////////////////////////////////////////////////////////////////
// RunTarget
///////////////////////////////////////////////////////////////////////////////
Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Pack") 
  ;

Task("BuildAndPublish")  
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Pack") 
  .IsDependentOn("PublishPackages") 
  ;

// Task("DockerPublish")
//   .IsDependentOn("Default")
//   .IsDependentOn("BuildDockerImage")
//   .IsDependentOn("PushDockerImage")
//   ;


RunTarget(target);

///////////////////////////////////////////////////////////////////////////////
// Utils
///////////////////////////////////////////////////////////////////////////////
private string GetVersion() {
    Verbose("Reading Version");

    var v = gitVersion.NuGetVersionV2;
    Verbose("NuGetVersionV2: {0}", v);
    return v;
}

private string GetAssemblyVersion() {
    Verbose("Reading AssemblyVersion");
    var v = gitVersion.AssemblySemVer;
    Verbose("AssemblySemVer: {0}", v);
    return v;
}


private void StartTestEnv(){
    Information("Starting Test Environment");

    DockerComposeUp( new DockerComposeUpSettings{
                Files = new [] { "docker-compose.yml" },
                DetachedMode = true,
                ForceRecreate = true
        });
}

private void StopTestEnv(){
    Information("Stopping Test Environment");

    DockerComposeDown( new DockerComposeDownSettings{
            Files = new [] { "docker-compose.yml" },
            RemoveOrphans = true
    });
}