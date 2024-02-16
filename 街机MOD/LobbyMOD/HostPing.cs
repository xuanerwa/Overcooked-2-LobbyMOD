using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
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
    }

    public static class HostPing
    {
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static ConfigEntry<KeyCode> pingkey;
        public static MultiplayerController MultiplayerController
        {
            get
            {
                return GameUtils.RequireManager<MultiplayerController>();
            }
        }

        public static void Awake()
        {
            pingkey = MODEntry.Instance.Config.Bind<KeyCode>("01-按键绑定", "12-查看玩家延迟", KeyCode.F5, "按键显示");
            Harmony.CreateAndPatchAll(typeof(HostPing));
        }


        // 获取自己的延迟
        public static float GetClientLatency()
        {
            MultiplayerController multiplayerController = GameUtils.RequestManager<MultiplayerController>();
            Client client = m_LocalClient.GetValue(multiplayerController) as Client;
            if (client != null)
            {
                ConnectionStats connectionStats = client.GetConnectionStats(bReliable: false);
                return connectionStats.m_fLatency;
            }
            log("无法获取客户端延迟");
            return 0f; // 如果无法获取延迟，则返回默认延迟值
        }


        // 获取服务器端到所有客户端的延迟
        public static List<PlayerInfo> GetServerToAllClientsLatency()
        {
            List<PlayerInfo> latencyList = new List<PlayerInfo>();

            MultiplayerController multiplayerController = GameUtils.RequestManager<MultiplayerController>();
            Server server = m_LocalServer.GetValue(multiplayerController) as Server;
            Dictionary<IOnlineMultiplayerSessionUserId, NetworkConnection> remoteClientConnectionsDict = m_RemoteClientConnections.GetValue(server) as Dictionary<IOnlineMultiplayerSessionUserId, NetworkConnection>;

            if (server != null)
            {
                foreach (var kvp in remoteClientConnectionsDict)
                {
                    IOnlineMultiplayerSessionUserId sessionUserId = kvp.Key;
                    NetworkConnection connection = kvp.Value;

                    // 获取客户端玩家昵称
                    string playerName = sessionUserId.DisplayName;

                    // 获取服务器到客户端的延迟
                    float latency = connection.GetConnectionStats(bReliable: false).m_fLatency;

                    // 创建PlayerInfo对象，并添加到列表中
                    PlayerInfo playerInfo = new PlayerInfo { Nickname = playerName, Latency = latency };
                    latencyList.Add(playerInfo);
                }
            }

            return latencyList;
        }
        public static void Update()
        {
            if (Input.GetKeyDown(pingkey.Value))
            {
                // 获取自己的延迟
                float clientLatency = GetClientLatency();
                log("自己的延迟：" + clientLatency);
                // 获取服务器到所有客户端的延迟
                List<PlayerInfo> allClientsLatency = GetServerToAllClientsLatency();
                if (allClientsLatency.Count > 0)
                {
                    log("服务器到所有客户端的延迟：");
                    foreach (var playerInfo in allClientsLatency)
                    {
                        log($"玩家昵称：{playerInfo.Nickname}, 延迟：{playerInfo.Latency}");
                    }
                }
                else
                {
                    log("无法获取服务器到所有客户端的延迟");
                }
            }
        }


        // 静态字段引用
        private static readonly FieldInfo m_LocalClient = AccessTools.Field(typeof(MultiplayerController), "m_LocalClient");
        private static readonly FieldInfo m_RemoteClientConnections = AccessTools.Field(typeof(Server), "m_RemoteClientConnections");
        private static readonly FieldInfo m_LocalServer = AccessTools.Field(typeof(MultiplayerController), "m_LocalServer");
        
    }
}