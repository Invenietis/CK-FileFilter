﻿using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Common.Diagnostics;
using SimpleGitVersion;
using Code.Cake;
using Cake.Common.Build.AppVeyor;
using Cake.Common.Tools.NuGet.Pack;
using System;
using System.Linq;
using Cake.Common.Tools.SignTool;
using Cake.Core.Diagnostics;
using Cake.Common.Tools.NUnit;
using Cake.Common.Text;
using Cake.Common.Tools.NuGet.Push;
using System.IO;

namespace CodeCake
{
    [AddPath( "CodeCakeBuilder/Tools" )]
    [AddPath( "packages/**/tools*" )]
    public class Build : CodeCakeHost
    {
        public Build()
        {

            var nugetOutputDir = Cake.Directory( "CodeCakeBuilder/Release" );

            var projectsToPublish = Cake.ParseSolution( "CK-Globbing.sln" )
                                        .Projects
                                        .Where( p => p.Name != "CodeCakeBuilder"
                                                     && !p.Path.Segments.Contains( "Tests" ) );
            SimpleRepositoryInfo gitInfo = null;
            string configuration = null;

            Task( "Check-Repository" )
                .Does( () =>
                {
                    gitInfo = Cake.GetSimpleRepositoryInfo();
                    if( !gitInfo.IsValid ) throw new Exception( "Repository is not ready to be published." );
                    configuration = gitInfo.IsValidRelease && gitInfo.PreReleaseName.Length == 0 ? "Release" : "Debug";

                    Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}",
                        projectsToPublish.Count(),
                        gitInfo.SemVer,
                        configuration,
                        String.Join( ", ", projectsToPublish.Select( p => p.Name ) ) );
                } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( nugetOutputDir );
                } );

            Task( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    Cake.NuGetRestore( "CK-Globbing.sln" );
                } );

            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    // Builds the assemblies
                    // Building it, clears the xml documentation file generated with GenerateDocumentation.
                    // It is built only on actual release (not CI build and not on prerelease).
                    using( var tempSln = Cake.CreateTemporarySolutionFile( "CK-Globbing.sln" ) )
                    {
                        tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder" );
                        Cake.MSBuild( tempSln.FullPath, new MSBuildSettings()
                                .SetConfiguration( configuration )
                                .SetVerbosity( Verbosity.Minimal )
                                .SetMaxCpuCount( 1 )
                                // Always generates Xml documentation. Relies on this definition in the csproj files:
                                //
                                // <PropertyGroup Condition=" $(GenerateDocumentation) != '' ">
                                //   <DocumentationFile>bin\$(Configuration)\$(AssemblyName).xml</DocumentationFile>
                                // </PropertyGroup>
                                //
                                .WithProperty( "GenerateDocumentation", "true" ) );
                    }
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .Does( () =>
                {
                    Cake.CreateDirectory( nugetOutputDir );
                    Cake.NUnit( "Tests/*.Tests/bin/" + configuration + "/*.Tests.dll", new NUnitSettings()
                    {
                        Framework = "v4.5",
                        OutputFile = nugetOutputDir.Path + "/TestResult.txt",
                        StopOnError = true
                    } );
                } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    var settings = new NuGetPackSettings()
                    {
                        Version = gitInfo.NuGetVersion,
                        BasePath = Cake.Environment.WorkingDirectory,
                        OutputDirectory = nugetOutputDir
                    };

                    Cake.CopyFiles( "CodeCakeBuilder/NuSpec/*.nuspec", nugetOutputDir );
                    foreach( var nuspec in Cake.GetFiles( nugetOutputDir.Path + "/*.nuspec" ) )
                    {
                        Cake.TransformTextFile( nuspec, "{{", "}}" ).WithToken( "configuration", configuration ).Save( nuspec );
                        Cake.NuGetPack( nuspec, settings );
                    }
                    Cake.DeleteFiles( nugetOutputDir.Path + "/*.nuspec" );
                } );

            Task( "Push-NuGet-Packages" )
                .IsDependentOn( "Create-NuGet-Packages" )
                .WithCriteria( () => gitInfo.IsValidRelease )
                .Does( () =>
                {
                    if( Cake.IsInteractiveMode() )
                    {
                        string localFeed = Cake.FindDirectoryAbove( "LocalFeed" );
                        if( localFeed != null )
                        {
                            Cake.Information( "Local feed directory found: {0}", localFeed );
                            if( Cake.ReadInteractiveOption( "Press Y to copy nuget packages to LocalFeed.", 'Y', 'N' ) == 'Y' )
                            {
                                Cake.CopyFiles( nugetOutputDir.Path + "/*.nupkg", localFeed );
                            }
                        }
                    }
                    // Resolves the API key.
                    var apiKey = Cake.InteractiveEnvironmentVariable( "NUGET_API_KEY" );
                    if( string.IsNullOrEmpty( apiKey ) )
                    {
                        Cake.Information( "Could not resolve NuGet API key. Push to NuGet is skipped." );
                    }
                    else
                    {
                        var settings = new NuGetPushSettings
                        {
                            Source = "https://www.nuget.org/api/v2/package",
                            ApiKey = apiKey
                        };

                        foreach( var nupkg in Cake.GetFiles( nugetOutputDir.Path + "/*.nupkg" ) )
                        {
                            Cake.NuGetPush( nupkg, settings );
                        }
                    }
                } );

            // The Default task for this script can be set here.
            Task( "Default" )
                .IsDependentOn( "Push-NuGet-Packages" );
        }
    }
}
