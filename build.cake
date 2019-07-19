// dotnet cake build.cake --target=GitVersion --verbosity=Verbose
// dotnet cake build.cake --target=clean --verbosity=Verbose



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
// Build
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
// Test
///////////////////////////////////////////////////////////////////////////////
Task("Test")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("GitVersion") 
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = configuration,
        EnvironmentVariables = new Dictionary<string, string> {
            { "NuGetVersionV2", GetVersion() },
            { "AssemblySemVer", GetAssemblyVersion() }
        },
        NoBuild = true,
        NoRestore = true
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
});

///////////////////////////////////////////////////////////////////////////////
// Pack
///////////////////////////////////////////////////////////////////////////////
Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("GitVersion")  
    .Does( () => {

    // var projects = GetFiles(publishProject);
    // foreach(var project in projects)
    // {
    //     Information("Starting DotNetCorePublish on Project: {0}", project.FullPath);
    //     var projName = project.GetFilenameWithoutExtension().ToString();
    //     var outputDir = artifactsDirectory + Directory(projName);
    //     Information("Output Dir: {0}", outputDir);

    //     if (!isDryRun) {

    //         var settings = new DotNetCorePublishSettings
    //         {
    //             Configuration = configuration,
    //             OutputDirectory = outputDir,
    //             EnvironmentVariables = new Dictionary<string, string> {
    //                 { "NuGetVersionV2", GetVersion() },
    //                 { "AssemblySemVer", GetAssemblyVersion() }
    //             },
    //             NoRestore = true
    //         };

    //         DotNetCorePublish(project.FullPath, settings);

    //         Zip(outputDir, artifactsDirectory + File($"{projName}-{GetVersion()}.zip") );
    //     }
    //     else {
    //         Verbose("Dry Run, skipping DotNetCorePublish");
    //     }
    // }

    // {
    //     var dir1 = artifactsDirectory + Directory("devops");
    //     CleanDirectories(new DirectoryPath[] { dir1 });
    //     CopyFiles("./devops/*.*", dir1 );
    // }
});

///////////////////////////////////////////////////////////////////////////////
// RunTarget
///////////////////////////////////////////////////////////////////////////////
Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Pack") 
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
