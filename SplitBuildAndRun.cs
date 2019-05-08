using System;
using System.IO;
using Unity.TestFramework.Build;
using UnityEditor;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.TestTools;

[assembly: TestPlayerBuildModifier(typeof(SplitBuildAndRun))]
[assembly: PostBuildCleanup(typeof(SplitBuildAndRun))]

namespace Unity.TestFramework.Build
{
    /// <summary>
    /// Setups Unity Test Framework in a way that it only runs test.
    /// </summary>
    public class SplitBuildAndRun : ITestPlayerBuildModifier, IPostBuildCleanup
    {
        /// <summary>
        /// Gives platforms an opportunity to modify build options
        /// and disables test run.
        /// </summary>
        /// <param name="playerOptions">options to modify</param>
        /// <returns></returns>
        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
        {
            var buildDirectoryPath = GetPlayerBuildDirectoryPath();
            if (buildDirectoryPath == null)
            {
                return playerOptions;
            }

            // Do not launch the player after the build is done.
            playerOptions.options &= ~BuildOptions.AutoRunPlayer;
            playerOptions.locationPathName = Path.HasExtension(playerOptions.locationPathName) ? Path.Combine(buildDirectoryPath, Path.GetFileName(playerOptions.locationPathName)) : buildDirectoryPath;

            playerOptions = ModifyOptionsPlatformSpecific(playerOptions);

            return playerOptions;
        }

        /// <summary>
        /// Invoked at the end of test run by testing framework.
        /// </summary>
        public void Cleanup()
        {
            if (GetPlayerBuildDirectoryPath() != null)
            {
                // Exit Unity on the next update, allowing for other PostBuildCleanup steps to run.
                EditorApplication.update += () => { EditorApplication.Exit(0); };
            }
        }

        private BuildPlayerOptions ModifyOptionsPlatformSpecific(BuildPlayerOptions playerOptions)
        {
            switch (playerOptions.target)
            {
                case BuildTarget.iOS:
                    playerOptions.options |= BuildOptions.SymlinkLibraries;
                    playerOptions.options &= ~BuildOptions.ConnectToHost;
                    break;
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.XboxOne:
                    PlayerSettings.productName = "PlayerWithTests";
                    break;
                default:
                    break;
            }

            return playerOptions;
        }

        private static string GetPlayerBuildDirectoryPath()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            for (var i = 0; i < commandLineArgs.Length - 1; i++)
            {
                if (commandLineArgs[i] == "-testPlayerPath")
                {
                    return commandLineArgs[i + 1];
                }
            }

            return null;
        }
    }
}
