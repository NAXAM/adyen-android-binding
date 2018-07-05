#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

// Cake Addins
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var VERSION = "1.14.2";
var CSE_VERSION = "1.0.5";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var slnPath = "./adyen-droid.sln";
var artifacts = new [] {
    new Artifact {
        Name = "CSE",
        Version = CSE_VERSION,
        AssemblyInfoPath = "./Naxam.AdyenCSE.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyencse.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcse%2Fadyen-cse%2F{0}%2Fadyen-cse-{0}.aar",
        JarPath = "./Naxam.AdyenCSE.Droid/Jars/adyen-cse.aar",
        Dependencies = new []{
            new NuSpecDependency {
                Id = "Xamarin.Android.Support.v7.AppCompat",
                Version = "25.4.0.2"
            }
        }
    },
    new Artifact {
        Name = "Core",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenCore.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyencore.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Fcore%2F{0}%2Fcore-{0}.aar",
        JarPath = "./Naxam.AdyenCore.Droid/Jars/core.aar",
        Dependencies = new []{
            new NuSpecDependency {
                Id = "Xamarin.Android.Support.v7.AppCompat",
                Version = "25.4.0.2"
            },
            new NuSpecDependency {
                Id = "Xamarin.Android.Support.CustomTabs",
                Version = "25.4.0.2"
            },
            new NuSpecDependency {
                Id = "Xamarin.Android.Support.Annotations",
                Version = "25.4.0.2"
            },
            new NuSpecDependency {
                Id = "Naxam.ReactiveStreams.Droid",
                Version = "1.0.0"
            },
            new NuSpecDependency {
                Id = "Naxam.RxAndroid.Droid",
                Version = "2.0.1"
            },
            new NuSpecDependency {
                Id = "Naxam.RxJava2.Droid",
                Version = "2.1.2"
            },
            new NuSpecDependency {
                Id = "Naxam.AdyenCSE.Droid",
                Version = CSE_VERSION
            }
        }
    },
    new Artifact {
        Name = "Utils",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenUtils.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyenutils.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Futils%2F{0}%2Futils-{0}.aar",
        JarPath = "./Naxam.AdyenUtils.Droid/Jars/utils.aar",
        Dependencies = new []{
            new NuSpecDependency {
                Id = "Xamarin.Android.Support.v7.AppCompat",
                Version = "25.4.0.2"
            }
        }
    },
    new Artifact {
        Name = "CardScan",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenCardScan.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyencardscan.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Fcardscan%2F{0}%2Fcardscan-{0}.aar",
        JarPath = "./Naxam.AdyenCardScan.Droid/Jars/cardscan.aar",
        Dependencies = new []{
            new NuSpecDependency {
                Id = "Xamarin.Android.Support.v7.AppCompat",
                Version = "25.4.0.2"
            }
        }
    },
    new Artifact {
        Name = "UI",
        Version = VERSION,
        AssemblyInfoPath = "./Naxam.AdyenUI.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./adyenui.nuspec",
        DownloadUrl = "https://bintray.com/adyen/adyen/download_file?file_path=com%2Fadyen%2Fcheckout%2Fui%2F{0}%2Fui-{0}.aar",
        JarPath = "./Naxam.AdyenUI.Droid/Jars/ui.aar",
        Dependencies = new []{
            new NuSpecDependency {
                Id = "Naxam.AdyenCSE.Droid",
                Version = CSE_VERSION
            },
            new NuSpecDependency {
                Id = "Naxam.AdyenCardCore.Droid",
                Version = VERSION
            },
            new NuSpecDependency {
                Id = "Naxam.AdyenCardScan.Droid",
                Version = VERSION
            },
            new NuSpecDependency {
                Id = "Naxam.AdyenUtils.Droid",
                Version = VERSION
            }
        }
    }
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Downloads")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        var downloadUrl = string.Format(artifact.DownloadUrl, artifact.Version);
        var jarPath = string.Format(artifact.JarPath, artifact.Version);

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
    NuGetRestore(slnPath);
});

Task("Build")
    .IsDependentOn("Downloads")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(slnPath, settings => settings.SetConfiguration(configuration));
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
            Version = artifact.Version,
            Title = $"Adyen for Android - {artifact.Name}",
            Description = $"Xamarin.Android binding library - Adyen for Android - {artifact.Name}",
            Dependencies = artifact.Dependencies,
            ReleaseNotes = new [] {
                $"Adyen for Android - {artifact.Name} v{artifact.Version}"
            }
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