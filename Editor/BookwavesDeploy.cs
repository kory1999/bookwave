using System.IO;
using DP.Deploy.Building;
using DP.Deploy.Plans;
using DP.Deploy.Utils;
using UnityEditor;
using UnityEngine;

namespace BeWild.AIBook.Editor
{
    [InitializeOnLoad]
    public static class BookwavesDeploy
    {
        private const string LogHeader = nameof(BookwavesDeploy);
        
        private const string PlistAdditionsPath = "Assets/Editor/Deploy/iOS/Plist additions.xml";
        private const string PlistName = "Info.plist";
        
        static BookwavesDeploy()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            
            DPDeployBuildManager.OnPreprocessBuildCompleted += OnPreprocessBuildCompleted;
            DPDeployBuildManager.OnPostprocessBuildCompleted += OnPostprocessBuildCompleted;
        }

        // add exported=true to all activities for GooglePlay
        [MenuItem("BeWild/AIBook/ProcessAndroidManifest")]
        private static void OnPreprocessBuildCompleted()
        {
#if UNITY_ANDROID
            AndroidManifestHelper.ProcessAndroid12Attributes();
#endif
        }
        private static void OnPostprocessBuildCompleted()
        {
            if (DPDeployBuildManager.CurrentPlan is IOS)
            {
                // AddXcodeAllowHttp();

                DisableXcodeUnityFrameworkEmbedSwift();
            }
        }

        // since PublicAPIManager.ProdBaseUrl is still in http, we need to enable http for xcode
        private static void AddXcodeAllowHttp()
        {
            string plistPath = Path.Combine(DPDeployBuildManager.BuildLocation, PlistName);
            TextAsset sourceFile = AssetDatabase.LoadAssetAtPath<TextAsset>(PlistAdditionsPath);
            PListParser.UpdatePList(sourceFile.bytes, plistPath);
        }

        // remove swift libs for UnityFramework for Apple 90206 error code.
        private static void DisableXcodeUnityFrameworkEmbedSwift()
        {
            Log($"DisableXcodeUnityFrameworkEmbedSwift: BuildLocation is {DPDeployBuildManager.BuildLocation}");
            
            IOSXCodeHelper.SetBuildPropertyInUnityFrameworkTarget(DPDeployBuildManager.BuildLocation, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
        }

        private static void Log(string info)
        {
            Debug.Log($"{LogHeader}: {info}");
        }
    }
}