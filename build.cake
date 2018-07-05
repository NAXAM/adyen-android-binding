#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

// Cake Addins
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var VERSION = "1.14.2";
var CSE_VERSION = "";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var slnPath = "./adyen-droid.sln";
var artifacts = new [] {
    new Artifact {
        Name = "CSE",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenCSE.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyencse.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcse%2Fadyen-cse%2F{0}%2Fadyen-cse-{0}.aar",
        JarPath = "./Naxam.AdyenCSE.Droid/Jars/adyen-cse.aar"
    },
    new Artifact {
        Name = "Core",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenCore.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyencore.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Fcore%2F{0}%2Fcore-{0}.aar",
        JarPath = "./Naxam.AdyenCore.Droid/Jars/core.aar"
    },
    new Artifact {
        Name = "Utils",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenUtils.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyenutils.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Futils%2F{0}%2Futils-{0}.aar",
        JarPath = "./Naxam.AdyenUtils.Droid/Jars/utils.aar"
    },
    new Artifact {
        Name = "UI",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenUI.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyenui.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Fui%2F{0}%2Fui-{0}.aar",
        JarPath = "./Naxam.AdyenUI.Droid/Jars/ui.aar"
    },
    new Artifact {
        Name = "CardScan",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenCardScan.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyencardscan.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Fcardscan%2F{0}%2Fcardscan-{0}.aar",
        JarPath = "./Naxam.AdyenCardScan.Droid/Jars/ui.aar"
    }
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Downloads")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        var downloadUrl = string.Format(artifact.DownloadUrl, VERSION);
        var jarPath = string.Format(artifact.JarPath, VERSION);

        DownloadFile(downloadUrl, jarPath);
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./packages");

    var nugetPackages = GetFiles("./*.nupkg");

    foreach (var package in nugetPackages)
    {
        DeleteFile(package);
    }
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetRestore(artifact.SolutionPath);
    }
    foreach(var artifact in artifacts) {
        NuGetUpdate(artifact.SolutionPath, new NuGetUpdateSettings {
            Id = new [] {
                "Naxam.Paypal.OneTouch"
            }, 
        });
    }
});

Task("Build")
    .IsDependentOn("Downloads")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        MSBuild(artifact.SolutionPath, settings => settings.SetConfiguration(configuration));
    }
});

Task("UpdateVersion")
    .Does(() => 
{
    foreach(var artifact in artifacts) {
        ReplaceRegexInFiles(artifact.AssemblyInfoPath, "\\[assembly\\: AssemblyVersion([^\\]]+)\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", VERSION));
    }
});

Task("Pack")
    .IsDependentOn("UpdateVersion")
    .IsDependentOn("Build")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetPack(artifact.NuspecPath, new NuGetPackSettings {
            Version = VERSION,
            // Dependencies = new []{
            //     new NuSpecDependency {
            //         Id = "Naxam.BrainTree.Core",
            //         Version = VERSION
            //     },
            //     new NuSpecDependency {
            //         Id = "Naxam.Paypal.OneTouch",
            //         Version = VERSION
            //     }
            // }
        });
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

class Artifact {
    public string AssemblyInfoPath { get; set; }

    public string SolutionPath { get; set; }

    public string DownloadUrl  { get; set; }

    public string JarPath { get; set; }

    public string NuspecPath { get; set; }

    public string Version { get; set; }

    public string ReleaseNote { get; set; }

    public string Name { get; set; }

    public NuSpecDependency[] Dependencies { get; set; } = new NuSpecDependency[0];
}