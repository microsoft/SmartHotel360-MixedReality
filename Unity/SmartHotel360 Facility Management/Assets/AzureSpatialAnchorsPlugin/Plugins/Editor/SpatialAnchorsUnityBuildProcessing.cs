using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Manages build processing to enable Azure Spatial Anchors to build as easily as possible.
/// </summary>
public class SpatialAnchorsUnityBuildProcessing : IActiveBuildTargetChanged, IPostprocessBuildWithReport
{
    private const string ARCorePluginFolder = @"Assets\GoogleARCore";
    private const string ARKitPluginFolder = @"Assets\UnityARKitPlugin";
    private const string AzureSpatialAnchorsPackage = "Microsoft.Azure.SpatialAnchors.WinCPP";
    private const string UnityRelativePodFilePath = "Assets/AzureSpatialAnchorsPlugin/Plugins/iOS/Podfile";
    private const string UnityRelativePackageVersionFilePath = @"Assets\AzureSpatialAnchorsPlugin\Plugins\HoloLens\version.txt";

    public int callbackOrder
    {
        get
        {
            return 1;
        }
    }

    /// <summary>
    /// Currently the ARCore Plugin does not build when the build target is set to UWP.
    /// This script will hide the ARCore plugin so long as the current build target is not Android
    /// </summary>
    /// <param name="previousTarget"></param>
    /// <param name="newTarget"></param>
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        string ARCoreAssetsDir = Path.Combine(Directory.GetCurrentDirectory(), ARCorePluginFolder);
        string ARKitAssetsDir = Path.Combine(Directory.GetCurrentDirectory(), ARKitPluginFolder);
        bool NeedRefresh = false;
        if (newTarget == BuildTarget.Android)
        {
            if (Directory.Exists(ARCoreAssetsDir) == false)
            {
                Debug.LogError($"Please put the ARCore 1.5 plugin in {ARCoreAssetsDir}");
            }
            else
            {
                Debug.Log("Enabling the ARCore SDK");
                ClearHiddenAttributeOnFileOrFolder(ARCoreAssetsDir);
                NeedRefresh = true;
            }
        }
        else if (Directory.Exists(ARCoreAssetsDir))
        {
            Debug.Log("Disabling the ARCore SDK");
            SetHiddenAttributeOnFileOrFolder(ARCoreAssetsDir);
            NeedRefresh |= (previousTarget == BuildTarget.Android);
        }

        if (newTarget == BuildTarget.iOS)
        {
            if (Directory.Exists(ARKitAssetsDir) == false)
            {
                Debug.LogError($"Please put the ARKit plugin in {ARKitAssetsDir}");
            }
            else
            {
                Debug.Log("Enabling the ARKit SDK");
                ClearHiddenAttributeOnFileOrFolder(ARKitAssetsDir);
                NeedRefresh = true;
            }
        }
        else if (Directory.Exists(ARKitAssetsDir))
        {
            Debug.Log("Disabling the ARKit SDK");
            SetHiddenAttributeOnFileOrFolder(ARKitAssetsDir);
            NeedRefresh |= (previousTarget == BuildTarget.iOS);
        }

        if (NeedRefresh)
        {
            Debug.Log("Rescanning scripts, errors before this line may be benign");
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Called after a build completes.
    /// </summary>
    /// <param name="report">A BuildReport containing information about the build, such as the target platform and output path.</param>
    public void OnPostprocessBuild(BuildReport report)
    {       
        if (report.summary.result == BuildResult.Failed
            || report.summary.result == BuildResult.Cancelled)
        {
            return;
        }

        if (report.summary.platform == BuildTarget.iOS)
        {
            Debug.Log($"Configuring iOS project at '{report.summary.outputPath}'.");
            ConfigureIOSProject(report.summary.outputPath);
        }
        else if (report.summary.platform == BuildTarget.WSAPlayer
            && (report.summary.platformGroup == BuildTargetGroup.WSA))
        {
            Debug.Log($"Configuring HoloLens project at '{report.summary.outputPath}'.");
            ConfigureHoloLensProject(Application.productName, report.summary.outputPath);
        }
        else
        {
            Debug.Log("No additional configuration necessary for Spatial Anchors.");
        }
    }

    /// <summary>
    /// Configures a HoloLens project for Spatial Anchors.
    /// </summary>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="buildOutputPath">The build output path.</param>
    private static void ConfigureHoloLensProject(string projectName, string buildOutputPath)
    {
        if (!Directory.Exists(buildOutputPath))
        {
            Debug.LogWarning($"Unable to configure the HoloLens project. Output path does not exist: '{buildOutputPath}'.");
            return;
        }

        string packageVersion = GetPackageVersionFromVersionFile();
        
        if (packageVersion == null)
        {
            Debug.LogWarning("Unable to determine Spatial Anchors version.");
            return;
        }

        string projectJsonFile = Path.Combine(buildOutputPath, projectName, "project.json");

        if (!File.Exists(projectJsonFile))
        {
            Debug.LogWarning($"Unable to locate the project.json file to patch at: {projectJsonFile}");
            return;
        }

        Debug.Log($"Patching {projectJsonFile} with {AzureSpatialAnchorsPackage} {packageVersion}");
        PatchProjectJsonFile(projectJsonFile, packageVersion);
    }

    /// <summary>
    /// Configures an iOS Project for Spatial Anchors.
    /// </summary>
    /// <param name="buildOutputPath">Build output path.</param>
    private static void ConfigureIOSProject(string buildOutputPath)
    {
        if (!Directory.Exists(buildOutputPath))
        {
            Debug.LogWarning($"Unable to configure the iOS project. Output path does not exist: '{buildOutputPath}'.");
            return;
        }

        string podFileName = Path.GetFileName(UnityRelativePodFilePath);

        string outputPodFilePath = Path.Combine(buildOutputPath, podFileName);

        if (!File.Exists(outputPodFilePath))
        {
            string inputPodFilePath = Path.Combine(Directory.GetCurrentDirectory(), UnityRelativePodFilePath);
            File.Copy(inputPodFilePath, outputPodFilePath);
            Debug.Log($"Spatial Anchors pod file copied to project path: '{outputPodFilePath}'.");
        }
        else
        {
            Debug.Log($"Spatial Anchors pod file already exists.");
        }
    }

    private static string GetPackageVersionFromVersionFile()
    {
        string versionFilePath = Path.Combine(Directory.GetCurrentDirectory(), UnityRelativePackageVersionFilePath);

        if (!File.Exists(versionFilePath))
        {
            return null;
        }

        return File.ReadAllText(versionFilePath).Trim();
    }

    private static void PatchProjectJsonFile(string projectJsonFilePath, string packageVersion)
    {
        if (!File.Exists(projectJsonFilePath))
        {
            Debug.LogWarning($"Can't find the specified project.json file: '{projectJsonFilePath}'.");
            return;
        }

        string[] lines = File.ReadAllLines(projectJsonFilePath);

        // Looks for:         "dependencies": {
        Regex dependenciesStart = new Regex(@"\s*""dependencies"":\s*\{\s*", RegexOptions.Compiled);

        // Looks for:         },
        Regex dependenciesStop = new Regex(@"\s*\}\s*,\s*", RegexOptions.Compiled);

        // Looks for:         "Package.Name": "0.0.0",
        Regex dependencyLine = new Regex(@"""(?<dep>.+)""\s*:\s*""(?<version>.+)""\s*,?\s*", RegexOptions.Compiled);

        Dictionary<string, string> dependencies = new Dictionary<string, string>(StringComparer.Ordinal);
        bool trackingDependencies = false;
        foreach (string line in lines)
        {
            if (trackingDependencies)
            {
                if (dependenciesStop.IsMatch(line))
                {
                    break;
                }

                var match = dependencyLine.Match(line);

                if (!match.Success)
                {
                    Debug.LogWarning($"Unable to understand the project.json file: {line}");
                    return;
                }

                dependencies.Add(match.Groups["dep"].Value, match.Groups["version"].Value);
            }

            if (dependenciesStart.IsMatch(line))
            {
                trackingDependencies = true;
            }
        }

        if (dependencies.Count == 0)
        {
            Debug.LogWarning("Unable to understand the project.json file. No depdendencies were found.");
            return;
        }

        if (dependencies.ContainsKey(AzureSpatialAnchorsPackage))
        {
            // The package is already present. Does it need updating?
            string existingPackageVersion = dependencies[AzureSpatialAnchorsPackage];

            if (existingPackageVersion == packageVersion)
            {
                // Nothing to do since we're up to date.
                return;
            }
            else
            {
                // Update the version in the file.
                Debug.Log($"Updating the version of {AzureSpatialAnchorsPackage} in the project.json file.");
                dependencies[AzureSpatialAnchorsPackage] = packageVersion;
            }
        }
        else
        {
            // Add the full dependency.
            Debug.Log($"Adding the {AzureSpatialAnchorsPackage} {packageVersion} dependency to the project.json file.");
            dependencies.Add(AzureSpatialAnchorsPackage, packageVersion);
        }

        trackingDependencies = false;
        List<string> newLines = new List<string>();
        foreach (string line in lines)
        {
            if (trackingDependencies)
            {
                if (dependenciesStop.IsMatch(line))
                {
                    newLines.Add(line);
                    trackingDependencies = false;
                }
            }
            else if (dependenciesStart.IsMatch(line))
            {
                newLines.Add(line);
                foreach (var dependency in dependencies.OrderBy(d => d.Key))
                {
                    newLines.Add($"    \"{dependency.Key}\": \"{dependency.Value}\",");
                }
                trackingDependencies = true;
            }
            else
            {
                newLines.Add(line);
            }
        }

        File.WriteAllLines(projectJsonFilePath, newLines);
    }

    private void SetHiddenAttributeOnFileOrFolder(string FileOrFolderName)
    {
        FileAttributes fileAttributes = File.GetAttributes(FileOrFolderName);
        File.SetAttributes(FileOrFolderName, fileAttributes | FileAttributes.Hidden);
    }

    private void ClearHiddenAttributeOnFileOrFolder(string FileOrFolderName)
    {
        FileAttributes fileAttributes = File.GetAttributes(FileOrFolderName);
        File.SetAttributes(FileOrFolderName, fileAttributes & ~FileAttributes.Hidden);
    }
}
