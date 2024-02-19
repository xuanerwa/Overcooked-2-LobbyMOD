using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Team17.Online;
using Team17.Online.Multiplayer;
using Team17.Online.Multiplayer.Connection;
using UnityEngine;

namespace LobbyMODS
{
    public class PlayerInfo
    {
        public string Nickname { get; set; }
        public float Latency { get; set; }

        public float Between { get; set; }
    }

    public static class HostPing
    {
        public static void log(string mes) => MODEntry.LogInfo(mes);
        //public static ConfigEntry<KeyCode> pingkey;
        //public static MultiplayerController MultiplayerController
        //{
        //    get
        //    {
        //        return GameUtils.RequireManager<MultiplayerController>();
        //    }
        //}

        public static void Awake()
        {
            //NetworkStateDebugDisplay networkStateDebugDisplay = new NetworkStateDebugDisplay();
            //pingkey = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "12-查看玩家延迟", KeyCode.F5, "按键显示");
            Harmony.CreateAndPatchAll(typeof(HostPing));
        }


        // 获取自己的延迟
        public static PlayerInfo GetClientLatency()
        {
            MultiplayerController multiplayerController = GameUtils.RequestManager<MultiplayerController>();
            Client client = multiplayerController.m_LocalClient;
            if (client != null)
            {
                ConnectionStats connectionStats = client.GetConnectionStats(bReliable: true);
                int latency = (int)Math.Round(connectionStats.m_fLatency * 1000, 2);
                int between = (int)Math.Round(connectionStats.m_fMaxTimeBetweenReceives * 1000, 2);
                PlayerInfo playerInfo = new PlayerInfo { Nickname = "自己的", Latency = latency, Between = between };
                return playerInfo;
            }
            return new PlayerInfo { Nickname = "自己的", Latency = 0f, Between = 0f };
        }


        // 获取服务器端到所有客户端的延迟
        public static List<PlayerInfo> GetServerToAllClientsLatency()
        {
            List<PlayerInfo> latencyList = new List<PlayerInfo>();

            MultiplayerController multiplayerController = GameUtils.RequestManager<MultiplayerController>();
            Server server = multiplayerController.m_LocalServer;
            Dictionary<IOnlineMultiplayerSessionUserId, NetworkConnection> remoteClientConnectionsDict = server.m_RemoteClientConnections;

            if (server != null)
            {
                foreach (var kvp in remoteClientConnectionsDict)
                {
                    IOnlineMultiplayerSessionUserId sessionUserId = kvp.Key;
                    NetworkConnection connection = kvp.Value;

                    // 获取客户端玩家昵称
                    string playerName = sessionUserId.DisplayName;

                    // 获取服务器到客户端的延迟
                    float latency = connection.GetConnectionStats(bReliable: true).m_fLatency;
                    float between = connection.GetConnectionStats(bReliable: true).m_fMaxTimeBetweenReceives;


                    // 创建PlayerInfo对象，并添加到列表中
                    PlayerInfo playerInfo = new PlayerInfo { Nickname = playerName, Latency = latency, Between = between };
                    latencyList.Add(playerInfo);
                }
            }

            return latencyList;
        }

        public static void Update()
        {
            if (DisplayLatencyUI.ShowEnabled.Value)
            {
                string latencyText = string.Empty;
                if (!MODEntry.IsHost)
                {
                    // 获取自己的延迟
                    PlayerInfo playerinfo = GetClientLatency();
                    latencyText += $"{playerinfo.Nickname}ping：{playerinfo.Latency} ms,  MaxTimeBetweenReceives:{playerinfo.Between} ms";
                }
                else
                {
                    // 获取服务器到所有客户端的延迟
                    List<PlayerInfo> allClientsLatency = GetServerToAllClientsLatency();
                    if (allClientsLatency.Count > 0)
                    {
                        latencyText += "客机的延迟：\n";
                        foreach (var playerInfo in allClientsLatency)
                        {
                            latencyText += $"{playerInfo.Nickname}    ping：{(int)Math.Round(playerInfo.Latency * 1000, 2)} ms    MaxTimeBetweenReceives:{(int)Math.Round(playerInfo.Between * 1000, 2)}\n";
                        }
                    }
                }
                //log(latencyText);
                DisplayLatencyUI.change_m_Text(latencyText);
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ClientTime), "OnTimeSyncReceived")]
        //public static void ClientTime_OnTimeSyncReceived_Patch()
        //{
        //    if (DisplayLatencyUI.ShowEnabled.Value)
        //    {
        //        string latencyText = string.Empty;
        //        if (!MODEntry.IsHost)
        //        {
        //            // 获取自己的延迟
        //            PlayerInfo playerinfo = GetClientLatency();
        //            latencyText += $"{playerinfo.Nickname}ping：{playerinfo.Latency} ms,  MaxTimeBetweenReceives:{playerinfo.Between} ms";
        //        }
        //        else
        //        {
        //            // 获取服务器到所有客户端的延迟
        //            List<PlayerInfo> allClientsLatency = GetServerToAllClientsLatency();
        //            if (allClientsLatency.Count > 0)
        //            {
        //                latencyText += "客机的延迟：\n";
        //                foreach (var playerInfo in allClientsLatency)
        //                {
        //                    latencyText += $"{playerInfo.Nickname}    ping：{(int)Math.Round(playerInfo.Latency * 1000, 2)} ms    MaxTimeBetweenReceives:{(int)Math.Round(playerInfo.Between * 1000, 2)}\n";
        //                }
        //            }
        //        }
        //        //log(latencyText);
        //        DisplayLatencyUI.change_m_Text(latencyText);
        //    }
        //}

        // 静态字段引用
        //private static readonly FieldInfo m_LocalClient = AccessTools.Field(typeof(MultiplayerController), "m_LocalClient");
        //private static readonly FieldInfo m_RemoteClientConnections = AccessTools.Field(typeof(Server), "m_RemoteClientConnections");
        //private static readonly FieldInfo m_LocalServer = AccessTools.Field(typeof(MultiplayerController), "m_LocalServer");
    }
}