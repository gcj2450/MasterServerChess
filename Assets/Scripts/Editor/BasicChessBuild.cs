﻿#if MIRROR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MasterServerToolkit.MasterServer.Examples.BasicMirrorRoomsAndLobbies
{
    public class BasicChessBuild
    {
        [MenuItem("CM/Build/BasicChess/Game (Headless)")]
        private static void BuildRoomForWindowsHeadless()
        {
            BuildRoomForWindows(true);
        }

        private static void BuildRoomForWindows(bool isHeadless)
        {
            string buildFolder = Path.Combine("Builds", "BasicChess", "Chess");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] {
                    "Assets/Scenes/BasicChess/Room/Chess.unity"
                },
                locationPathName = Path.Combine(buildFolder, "Chess.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = isHeadless ? BuildOptions.ShowBuiltPlayer | BuildOptions.EnableHeadlessMode : BuildOptions.ShowBuiltPlayer
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string appConfig = Mst.Args.AppConfigFile(buildFolder);

                MstProperties properties = new MstProperties();
                properties.Add(Mst.Args.Names.StartClientConnection, true);
                properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
                properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
                properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
                properties.Add(Mst.Args.Names.RoomPort, Mst.Args.RoomPort);

                File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));

                Debug.Log("Room build succeeded: " + (summary.totalSize / 1024) + " kb");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Room build failed");
            }
        }

        // [MenuItem(Mst.ToolMenu + "Build/Demos for Mirror/Basic Rooms And Lobbies/Master Server and Spawner")]
        // private static void BuildMasterAndSpawnerForWindows()
        // {
        //     string buildFolder = Path.Combine("Builds", "BasicMirrorRoomsAndLobbies", "MasterAndSpawner");
        //     string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "BasicMirrorRoomsAndLobbies", "Room", "Room.exe");
        //
        //     BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        //     {
        //         scenes = new[] { "Assets/MasterServerToolkit/Bridges/Mirror/BasicRoomsAndLobbies/Scenes/Master/Master.unity" },
        //         locationPathName = Path.Combine(buildFolder, "MasterAndSpawner.exe"),
        //         target = BuildTarget.StandaloneWindows64,
        //         options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
        //     };
        //
        //     BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        //     BuildSummary summary = report.summary;
        //
        //     if (summary.result == BuildResult.Succeeded)
        //     {
        //         MstProperties properties = new MstProperties();
        //         properties.Add(Mst.Args.Names.StartMaster, true);
        //         properties.Add(Mst.Args.Names.StartSpawner, true);
        //         properties.Add(Mst.Args.Names.StartClientConnection, true);
        //         properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
        //         properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
        //         properties.Add(Mst.Args.Names.RoomExecutablePath, roomExePath);
        //         properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
        //         properties.Add(Mst.Args.Names.RoomRegion, Mst.Args.RoomRegion);
        //
        //         File.WriteAllText(Path.Combine(buildFolder, "application.cfg"), properties.ToReadableString("\n", "="));
        //
        //         Debug.Log("Master Server build succeeded: " + (summary.totalSize / 1024) + " kb");
        //     }
        //
        //     if (summary.result == BuildResult.Failed)
        //     {
        //         Debug.Log("Master Server build failed");
        //     }
        // }
        //
        // [MenuItem(Mst.ToolMenu + "Build/Demos for Mirror/Basic Rooms And Lobbies/Spawner")]
        // private static void BuildSpawnerForWindows()
        // {
        //     string buildFolder = Path.Combine("Builds", "BasicMirrorRoomsAndLobbies", "Spawner");
        //     string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "BasicMirrorRoomsAndLobbies", "Room", "Room.exe");
        //
        //     BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        //     {
        //         scenes = new[] {
        //             "Assets/MasterServerToolkit/Bridges/Mirror/BasicRoomsAndLobbies/Scenes/Spawner/Spawner.unity"
        //         },
        //         locationPathName = Path.Combine(buildFolder, "Spawner.exe"),
        //         target = BuildTarget.StandaloneWindows64,
        //         options = BuildOptions.ShowBuiltPlayer | BuildOptions.EnableHeadlessMode
        //     };
        //
        //     BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        //     BuildSummary summary = report.summary;
        //
        //     if (summary.result == BuildResult.Succeeded)
        //     {
        //         string appConfig = Mst.Args.AppConfigFile(buildFolder);
        //
        //         MstProperties properties = new MstProperties();
        //         properties.Add(Mst.Args.Names.StartSpawner, true);
        //         properties.Add(Mst.Args.Names.StartClientConnection, true);
        //         properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
        //         properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
        //         properties.Add(Mst.Args.Names.RoomExecutablePath, roomExePath);
        //         properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
        //         properties.Add(Mst.Args.Names.RoomRegion, Mst.Args.RoomRegion);
        //
        //         File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));
        //
        //         Debug.Log("Spawner build succeeded: " + (summary.totalSize / 1024) + " kb");
        //     }
        //
        //     if (summary.result == BuildResult.Failed)
        //     {
        //         Debug.Log("Spawner build failed");
        //     }
        // }
        //
        // [MenuItem(Mst.ToolMenu + "Build/Demos for Mirror/Basic Rooms And Lobbies/Client")]
        // private static void BuildClientForWindows()
        // {
        //     string buildFolder = Path.Combine("Builds", "BasicMirrorRoomsAndLobbies", "Client");
        //
        //     BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        //     {
        //         scenes = new[] {
        //             "Assets/MasterServerToolkit/Bridges/Mirror/BasicRoomsAndLobbies/Scenes/Client/Client.unity",
        //             "Assets/MasterServerToolkit/Bridges/Mirror/BasicRoomsAndLobbies/Scenes/Room/Room.unity"
        //         },
        //         locationPathName = Path.Combine(buildFolder, "Client.exe"),
        //         target = BuildTarget.StandaloneWindows64,
        //         options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
        //     };
        //
        //     BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        //     BuildSummary summary = report.summary;
        //
        //     if (summary.result == BuildResult.Succeeded)
        //     {
        //         string appConfig = Mst.Args.AppConfigFile(buildFolder);
        //
        //         MstProperties properties = new MstProperties();
        //         properties.Add(Mst.Args.Names.StartClientConnection, true);
        //         properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
        //         properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
        //
        //         File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));
        //
        //         Debug.Log("Client build succeeded: " + (summary.totalSize / 1024) + " kb");
        //     }
        //
        //     if (summary.result == BuildResult.Failed)
        //     {
        //         Debug.Log("Client build failed");
        //     }
        // }
    }
}
#endif