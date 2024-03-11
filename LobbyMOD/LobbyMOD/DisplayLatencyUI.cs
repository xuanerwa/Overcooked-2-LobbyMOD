using BepInEx.Configuration;
using System.Collections.Generic;
using Team17.Online.Multiplayer.Connection;
using Team17.Online;
using UnityEngine;
using Team17.Online.Multiplayer;
using HarmonyLib;
using System;
using System.Reflection;

namespace LobbyMODS
{
    public class DisplayLatencyUI
    {
        public static Harmony HarmonyInstance { get; set; }
        private static MyOnScreenDebugDisplay onScreenDebugDisplay;
        private static NetworkStateDebugDisplay NetworkDebugUI = null;
        public static ConfigEntry<bool> ShowEnabled;
        public static ConfigEntry<bool> isShowDebugInfo;
        public static bool canAdd;



        //public static void add_m_Text(string str) => NetworkDebugUI?.add_m_Text(str);
        //public static void change_m_Text(string str) => NetworkDebugUI?.change_m_Text(str);

        public static void Awake()
        {
            ShowEnabled = MODEntry.Instance.Config.Bind<bool>("00-UI", "03-屏幕右上角显示延迟", true);
            isShowDebugInfo = MODEntry.Instance.Config.Bind<bool>("00-UI", "04-屏幕右上角增加显示调试信息", false);
            canAdd = false;
            onScreenDebugDisplay = new MyOnScreenDebugDisplay();
            onScreenDebugDisplay.Awake();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public static void Update()
        {
            onScreenDebugDisplay.Update();

            if (NetworkDebugUI != null && !ShowEnabled.Value)
            {
                RemoveNetworkDebugUI();
            }
            else if (NetworkDebugUI == null && ShowEnabled.Value && canAdd)
            {
                AddNetworkDebugUI();
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MetaGameProgress), "ByteLoad")]
        public static void MetaGameProgressByteLoadPatch(MetaGameProgress __instance)
        {
            canAdd = true;
        }

        public static void OnGUI() => onScreenDebugDisplay.OnGUI();

        private static void AddNetworkDebugUI()
        {
            NetworkDebugUI = new NetworkStateDebugDisplay();
            onScreenDebugDisplay.AddDisplay(NetworkDebugUI);
            //NetworkDebugUI.init_m_Text();
        }

        private static void RemoveNetworkDebugUI()
        {
            onScreenDebugDisplay.RemoveDisplay(NetworkDebugUI);
            NetworkDebugUI.OnDestroy();
            NetworkDebugUI = null;
        }


        private class MyOnScreenDebugDisplay
        {
            private readonly List<DebugDisplay> m_Displays = new List<DebugDisplay>();
            private readonly GUIStyle m_GUIStyle = new GUIStyle();
            public void AddDisplay(DebugDisplay display)
            {
                if (display != null)
                {
                    display.OnSetUp();
                    m_Displays.Add(display);
                }
            }

            public void RemoveDisplay(DebugDisplay display) => m_Displays.Remove(display);

            public void Awake()
            {
                m_GUIStyle.alignment = TextAnchor.UpperRight;
                m_GUIStyle.fontSize = Mathf.RoundToInt(MODEntry.defaultFontSize.Value * MODEntry.dpiScaleFactor);
                try
                {
                    this.m_GUIStyle.normal.textColor = HexToColor(MODEntry.defaultFontColor.Value);
                }
                catch
                {
                    this.m_GUIStyle.normal.textColor = HexToColor("#FFFFFF");
                }
                m_GUIStyle.richText = false;
            }

            public void Update()
            {
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnUpdate();
            }

            public void OnGUI()
            {
                m_GUIStyle.fontSize = Mathf.RoundToInt(MODEntry.defaultFontSize.Value * MODEntry.dpiScaleFactor);
                try
                {
                    this.m_GUIStyle.normal.textColor = HexToColor(MODEntry.defaultFontColor.Value);
                }
                catch
                {
                    this.m_GUIStyle.normal.textColor = HexToColor("#FFFFFF");
                }
                Rect rect = new Rect(0f, 0f, Screen.width, m_GUIStyle.fontSize);
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnDraw(ref rect, m_GUIStyle);
            }
        }


        public class NetworkStateDebugDisplay : DebugDisplay
        {
            // Token: 0x06002A63 RID: 10851 RVA: 0x000C6400 File Offset: 0x000C4800
            public override void OnSetUp()
            {
                m_MultiplayerController = GameUtils.RequireManager<MultiplayerController>();
                IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
                m_ConnectionModeCoordinator = onlinePlatformManager.OnlineMultiplayerConnectionModeCoordinator();
            }

            // Token: 0x06002A64 RID: 10852 RVA: 0x000C642A File Offset: 0x000C482A
            public override void OnUpdate()
            {
            }

            // Token: 0x06002A65 RID: 10853 RVA: 0x000C642C File Offset: 0x000C482C
            public override void OnDraw(ref Rect rect, GUIStyle style)
            {
                if (isShowDebugInfo.Value){
                    string text = string.Empty;
                    string text2 = string.Empty;
                    if (ConnectionModeSwitcher.GetRequestedConnectionState() == NetConnectionState.Server)
                    {
                        ServerOptions serverOptions = (ServerOptions)ConnectionModeSwitcher.GetAgentData();
                        text = ", visibility: " + serverOptions.visibility.ToString();
                        text2 = ", gameMode: " + serverOptions.gameMode.ToString();
                    }
                    else if (ConnectionModeSwitcher.GetRequestedConnectionState() == NetConnectionState.Matchmake)
                    {
                        MatchmakeData matchmakeData = (MatchmakeData)ConnectionModeSwitcher.GetAgentData();
                        if (ConnectionStatus.IsHost())
                        {
                            text = ",HostgameMode: " + OnlineMultiplayerSessionVisibility.eMatchmaking;
                        }
                        text2 = ",ClientgameMode: " + matchmakeData.gameMode.ToString();
                    }
                    DrawText(ref rect, style, string.Concat(new string[]
                    {
                    "RequestedConnectionState: ",
                    ConnectionModeSwitcher.GetRequestedConnectionState().ToString(),
                    text,
                    text2,
                    ",Progress: ",
                    ConnectionModeSwitcher.GetStatus().GetProgress().ToString(),
                    " Result: ",
                    ConnectionModeSwitcher.GetStatus().GetResult().ToString()
                    }));

                    //LobbyInfo
                    string Lobbymessage = "NotInLobby";
                    if (ClientLobbyFlowController.Instance != null)
                    {
                        Lobbymessage = ClientLobbyFlowController.Instance.m_state.ToString();
                    }
                    DrawText(ref rect, style, string.Concat(new string[]
                    {
                    "LobbyState: ",
                    Lobbymessage,
                    ",joinCode: ",
                    ForceHost.joinReturnCode
                    }));
                }
                DrawText(ref rect, style, ClientGameSetup.Mode + ", time: " + ClientTime.Time().ToString("00000.000"));
                if (ConnectionStatus.IsHost())
                {

                    MultiplayerController multiplayerController = GameUtils.RequestManager<MultiplayerController>();
                    Server server = multiplayerController.m_LocalServer;
                    Dictionary<IOnlineMultiplayerSessionUserId, NetworkConnection> remoteClientConnectionsDict = server.m_RemoteClientConnections;

                    FastList<ConnectionStats> serverConnectionStats = m_MultiplayerController.GetServerConnectionStats(true);
                    //FastList<ConnectionStats> serverConnectionStats2 = m_MultiplayerController.GetServerConnectionStats(false);

                    if (serverConnectionStats.Count > 0)
                    {
                        string empty = string.Empty;
                        for (int i = 0; i < serverConnectionStats.Count; i++)
                        {
                            try
                            {
                                DrawText(ref rect, style, string.Concat(new object[]
                                {
                                ServerUserSystem.m_Users._items[i+1].DisplayName,
                                $" {i+2}号位延迟: ",
                                (serverConnectionStats._items[i].m_fLatency * 1000f).ToString("000"),
                                " ms"
                                    //" Sequence: I",
                                    //serverConnectionStats._items[i].m_fIncomingSequenceNumber,
                                    //" / O",
                                    //serverConnectionStats._items[i].m_fOutgoingSequenceNumber
                                }));
                            }
                            catch (Exception)
                            {
                                //MODEntry.LogError($"{ex}");
                            }
                        }
                        //DrawText(ref rect, style, empty);
                        //empty = string.Empty;
                        //for (int j = 0; j < serverConnectionStats.Count; j++)
                        //{
                        //    DrawText(ref rect, style, string.Concat(new object[]
                        //    {
                        //        $"UnReliable {j+2}号位延迟: ",
                        //(serverConnectionStats2._items[j].m_fLatency * 1000f).ToString("000"),
                        //"ms  MaxWait: ",
                        //serverConnectionStats2._items[j].m_fMaxTimeBetweenReceives.ToString("00.00"),
                        //"  昵称: ",
                        //ServerUserSystem.m_Users._items[j+1].DisplayName

                        ////" Sequence: I",
                        ////serverConnectionStats2._items[j].m_fIncomingSequenceNumber,
                        ////" / O",
                        ////serverConnectionStats2._items[j].m_fOutgoingSequenceNumber
                        //    }));
                        //}
                    }


                }
                else if (ConnectionStatus.IsInSession())
                {
                    try
                    {
                        ConnectionStats clientConnectionStats = m_MultiplayerController.GetClientConnectionStats(true);

                        //ConnectionStats clientConnectionStats2 = m_MultiplayerController.GetClientConnectionStats(false);
                        DrawText(ref rect, style, string.Concat(
                            new object[]
                            {
                            "本机延迟: ",
                            (clientConnectionStats.m_fLatency * 1000f).ToString("000"),
                            " ms",
                                //clientConnectionStats.m_fMaxTimeBetweenReceives.ToString("00.00"),
                                //" Sequence: I",
                                //clientConnectionStats.m_fIncomingSequenceNumber,
                                //" / O",
                                //clientConnectionStats.m_fOutgoingSequenceNumber
                            }
                        ));

                    }
                    catch (Exception ex)
                    {
                        MODEntry.LogError($"{ex}");
                    }
                    //    DrawText(ref rect, style, string.Concat(new object[]
                    //    {
                    //"ULag: ",
                    //(clientConnectionStats2.m_fLatency * 1000f).ToString("000"),
                    //"ms MaxWait: ",
                    //clientConnectionStats2.m_fMaxTimeBetweenReceives.ToString("00.00"),
                    ////" Sequence: I",
                    ////clientConnectionStats2.m_fIncomingSequenceNumber,
                    ////" / O",
                    ////clientConnectionStats2.m_fOutgoingSequenceNumber
                    //    }));
                }
                if (m_ConnectionModeCoordinator != null)
                {
                    DrawText(ref rect, style, m_ConnectionModeCoordinator.DebugStatus());
                }
            }

            // Token: 0x04002170 RID: 8560
            private MultiplayerController m_MultiplayerController;

            // Token: 0x04002171 RID: 8561
            private IOnlineMultiplayerConnectionModeCoordinator m_ConnectionModeCoordinator;
        }



        private static Color HexToColor(string hex)
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }


    }
}
