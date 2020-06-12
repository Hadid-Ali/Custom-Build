using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using UnityEditor.Build.Reporting;

#if UNITY_5_3
using UnityEditor.SceneManagement;
#endif

public class CustomMenus : MonoBehaviour
{
    [MenuItem("Custom Build/Build Android")]
    public static void AndroidBuild()
    {
        if(!EditorUserBuildSettings.selectedBuildTargetGroup.Equals(BuildTargetGroup.Android))
        {
            if (EditorUtility.DisplayDialog("Switch Platform", "Convert Project to Target Android", "Yes", "No"))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            else
                return;
        }
        PlayerSettings.Android.useAPKExpansionFiles = EditorUtility.DisplayDialog("Build Option", "You Want To Create Build Apk with Split Binray", "Split Binary", "Single APK");
        bool scripting = EditorUtility.DisplayDialog("Build Option", "Select Scripting version for build process", "IL2CPP", "Mono");

        string path = EditorUtility.SaveFilePanel("Choose Where you want to Create the Build", "", "","");

        List<EditorBuildSettingsScene> scenesL = new List<EditorBuildSettingsScene>();
        scenesL.AddRange(EditorBuildSettings.scenes);

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, scripting ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

        List<string> scenesS = new List<string>();

        for(int i=0;i<scenesL.Count;i++)
        {
            scenesS.Add(scenesL[i].path);
            Debug.LogError(scenesS[i]);
        }
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenesS.ToArray();
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        buildPlayerOptions.locationPathName = path+".apk";

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result == BuildResult.Succeeded)
        {
            string buildPath = path + ".apk";

            if (PlayerSettings.Android.useAPKExpansionFiles)
            {
                RenameAndPlaceBuild(buildPath, path);
            }
        }
    }

    public static void RenameAndPlaceBuild(string buildPath,string expansionPath)
    {
        string[] buildPathSplit = buildPath.Split('/');
        Debug.LogError(buildPath);
        string folderPath = "";
        for (int i = 0; i < buildPathSplit.Length - 1; i++)
        {
            folderPath += buildPathSplit[i]+"/";
        }
        Debug.LogError(folderPath);
        string buildIdentifier = buildPathSplit[buildPathSplit.Length - 1];

        string buildDate = DateTime.Now.Date.ToString().Split(' ')[0].Replace('/','-');
        string stringCurrent = DateTime.Now.TimeOfDay.Hours+"-"+ DateTime.Now.TimeOfDay.Minutes+"-" + DateTime.Now.TimeOfDay.Seconds;

        string folderToCreate = string.Format("Build {0} {1}", buildDate, stringCurrent);
        string completeFolder = string.Format("{0}/{1}", folderPath, folderToCreate);
        string expansionLocation = string.Format("{0}/{1}", completeFolder, PlayerSettings.applicationIdentifier);


        if (!Directory.Exists(completeFolder))
        {
            Directory.CreateDirectory(completeFolder);
        }

        if(!Directory.Exists(expansionLocation))
        {
            Directory.CreateDirectory(expansionLocation);
        }

        FileInfo fInfo = new FileInfo(buildPath);
        fInfo.MoveTo(completeFolder + "/" + buildIdentifier);

        string expectedExpansion = string.Format("main.{0}.{1}.obb", PlayerSettings.Android.bundleVersionCode, PlayerSettings.applicationIdentifier);
        File.Move(string.Format("{0}.main.obb", expansionPath), expansionLocation + "/" + expectedExpansion);
        System.Diagnostics.Process.Start(completeFolder);
    }
}
