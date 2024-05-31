using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using Team17.Online;

namespace HostUtilities
{
    public class ForceHost
    {
        public static void Log(string mes) => MODEntry.LogInfo(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogE(string mes) => MODEntry.LogError(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static void LogW(string mes) => MODEntry.LogWarning(MethodBase.GetCurrentMethod().DeclaringType.Name, mes);
        public static Harmony HarmonyInstance { get; set; }
        public static ConfigEntry<string> ValueList;
        private static string[] strList = {
            "游戏默认逻辑",
            "强制主机",
            //"强制客机"
        };
        public static void Awake()
        {
            ValueList = MODEntry.Instance.Config.Bind("01-功能开关", "00-切换默认主机/客机角色:", strList[0], new ConfigDescription("选择状态", new AcceptableValueList<string>(strList)));
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony[MethodBase.GetCurrentMethod().DeclaringType.Name] = HarmonyInstance;
        }
        public static void Update()
        {
            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    ClientLobbyFlowController lobbyFlowController = ClientLobbyFlowController.Instance;

            //    if (lobbyFlowController != null)
            //    {
            //        Log"call TryJoinGame()");
            //        lobbyFlowController.TryJoinGame();
            //    }
            //    else
            //    {
            //        Log"未找到 ClientLobbyFlowController 实例");
            //    }
            //}
            //else if (Input.GetKeyDown(KeyCode.H))
            //{
            //    ClientLobbyFlowController lobbyFlowController = ClientLobbyFlowController.Instance;

            //    if (lobbyFlowController != null)
            //    {
            //        Log"call HostGame()");
            //        lobbyFlowController.HostGame();
            //    }
            //    else
            //    {
            //        Log"未找到 ClientLobbyFlowController 实例");
            //    }
            //}
            //else if (Input.GetKeyDown(KeyCode.K))
            //{
            //    ClientLobbyFlowController __instance = ClientLobbyFlowController.Instance;
            //    ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Offline, null, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestOfflineStateFollowingFailureComplete));
            //    IPlayerManager playerManager = GameUtils.RequireManagerInterface<IPlayerManager>();
            //    IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
            //    IOnlineMultiplayerSessionCoordinator onlineMultiplayerSessionCoordinator = onlinePlatformManager.OnlineMultiplayerSessionCoordinator();
            //    if (onlineMultiplayerSessionCoordinator != null)
            //    {
            //        ServerOptions serverOptions = default(ServerOptions);
            //        serverOptions.gameMode = ((!__instance.m_bIsCoop) ? GameMode.Versus : GameMode.Party);
            //        if (__instance.m_lobbyInfo.m_visiblity == OnlineMultiplayerSessionVisibility.ePrivate)
            //        {
            //            serverOptions.visibility = OnlineMultiplayerSessionVisibility.eClosed;
            //        }
            //        else
            //        {
            //            serverOptions.visibility = __instance.m_lobbyInfo.m_visiblity;
            //        }
            //        serverOptions.hostUser = playerManager.GetUser(EngagementSlot.One);
            //        serverOptions.connectionMode = __instance.m_lobbyInfo.m_connectionMode;
            //        ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Server, serverOptions, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestConnectionStateServerComplete));
            //        ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Matchmake, new MatchmakeData
            //        {
            //            gameMode = ((!__instance.m_bIsCoop) ? GameMode.Versus : GameMode.Party),
            //            User = playerManager.GetUser(EngagementSlot.One),
            //            connectionMode = __instance.m_lobbyInfo.m_connectionMode
            //        }, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestConnectionStateJoinComplete));
            //    }
            //}
        }
        public static string joinReturnCode = "还未返回值";
        //[HarmonyPatch(typeof(ClientLobbyFlowController), "OnRequestConnectionStateJoinComplete")]
        //[HarmonyPrefix]
        //private static bool ClientLobbyFlowController_OnRequestConnectionStateJoinComplete_Prefix(ClientLobbyFlowController __instance, IConnectionModeSwitchStatus status)
        //{
        //    bool flag = ForceHost.ValueList.Value.Equals("强制客机");
        //    if (!flag) { Log"未开启强制客机,不拦截"); return true; }
        //    if (ServerUserSystem.m_Users.Count > 1) { Log"用户数量大于1,不启用强制客机"); return true; }
        //    Log"进入 OnRequestConnectionStateJoinComplete");
        //    //ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Offline, null, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnLeaveConfirmedOfflineConnectionState));
        //    //__instance.TryJoinGame();

        //    if (status.GetResult() == eConnectionModeSwitchResult.Success)
        //    {
        //        GameUtils.SendDiagnosticEvent("Automatchmake:Success");
        //        if (ConnectionStatus.isHost())
        //        {
        //            __instance.HostGame();
        //        }
        //        else
        //        {
        //            __instance.SetState(LobbyFlowController.LobbyState.OnlineSetup);
        //            __instance.m_message.m_type = LobbyClientMessage.LobbyMessageType.StateRequest;
        //            ClientMessenger.LobbyMessage(__instance.m_message);
        //            __instance.m_lobbyFlow.RefreshUserColours(__instance.m_bIsCoop);
        //            __instance.UpdateUIColours();

        //            Log"Automatchmake:Success");
        //        }
        //    }
        //    else if (status.DisplayPlatformDialog())
        //    {
        //        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:PlatformError");
        //        __instance.Leave();
        //    }
        //    else
        //    {
        //        CompositeStatus compositeStatus = status as CompositeStatus;
        //        JoinSessionStatus joinSessionStatus = compositeStatus?.m_TaskSubStatus as JoinSessionStatus ?? compositeStatus?.m_TaskSubStatus as AutoMatchmakingStatus;

        //        if (joinSessionStatus != null &&
        //            (joinSessionStatus.sessionJoinResult.m_returnCode == OnlineMultiplayerSessionJoinResult.eLostNetwork ||
        //             joinSessionStatus.sessionJoinResult.m_returnCode == OnlineMultiplayerSessionJoinResult.eApplicationSuspended ||
        //             joinSessionStatus.sessionJoinResult.m_returnCode == OnlineMultiplayerSessionJoinResult.eGoneOffline ||
        //             joinSessionStatus.sessionJoinResult.m_returnCode == OnlineMultiplayerSessionJoinResult.eLoggedOut))
        //        {
        //            switch (joinSessionStatus.sessionJoinResult.m_returnCode)
        //            {
        //                case OnlineMultiplayerSessionJoinResult.eLostNetwork:
        //                    GameUtils.SendDiagnosticEvent("Automatchmake:Failure:eLostNetwork");
        //                    break;
        //                case OnlineMultiplayerSessionJoinResult.eApplicationSuspended:
        //                    GameUtils.SendDiagnosticEvent("Automatchmake:Failure:eApplicationSuspended");
        //                    break;
        //                case OnlineMultiplayerSessionJoinResult.eGoneOffline:
        //                    GameUtils.SendDiagnosticEvent("Automatchmake:Failure:eGoneOffline");
        //                    break;
        //                case OnlineMultiplayerSessionJoinResult.eLoggedOut:
        //                    GameUtils.SendDiagnosticEvent("Automatchmake:Failure:eLoggedOut");
        //                    break;
        //                default:
        //                    GameUtils.SendDiagnosticEvent("Automatchmake:Failure:Generic");
        //                    break;
        //            }
        //            __instance.m_lastStatus = status.Clone();
        //            ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Offline, null, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestOfflineStateFollowingFailureComplete));

        //        }
        //        else
        //        {
        //            if (joinSessionStatus != null)
        //            {
        //                switch (joinSessionStatus.sessionJoinResult.m_returnCode)
        //                {
        //                    case OnlineMultiplayerSessionJoinResult.eClosed:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eClosed");
        //                        Log("eClosed: OnlineMultiplayerSessionJoinResult.eClosed");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eFull:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eFull");
        //                        Log("主机满: OnlineMultiplayerSessionJoinResult.eFull");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eNoLongerExists:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eNoLongerExists");
        //                        Log("战局不存在: OnlineMultiplayerSessionJoinResult.eNoLongerExists");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eNoHostConnection:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eNoHostConnection");
        //                        Log("NoHostConnection: OnlineMultiplayerSessionJoinResult.eNoHostConnection");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eLoggedOut:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eLoggedOut");
        //                        Log("已登出: OnlineMultiplayerSessionJoinResult.eLoggedOut");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eCodeVersionMismatch:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eCodeVersionMismatch");
        //                        Log("版本不匹配: OnlineMultiplayerSessionJoinResult.eCodeVersionMismatch");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eGenericFailure:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eGenericFailure");
        //                        Log("Generic失败: OnlineMultiplayerSessionJoinResult.eGenericFailure");
        //                        break;
        //                    case OnlineMultiplayerSessionJoinResult.eNotEnoughRoomForAllLocalUsers:
        //                        GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_eNotEnoughRoomForAllLocalUsers");
        //                        Log("位置不足: OnlineMultiplayerSessionJoinResult.eNotEnoughRoomForAllLocalUsers");
        //                        break;
        //                }
        //                joinReturnCode = joinSessionStatus.sessionJoinResult.m_returnCode.ToString();
        //            }
        //            else if (!GameUtils.s_RoomSearch_NoneAvailable)
        //            {
        //                GameUtils.SendDiagnosticEvent("Automatchmake:Failure:NonFatal_NotSpecified");
        //                Log"不知道什么错误: Automatchmake:Failure:NonFatal_NotSpecified");
        //                joinReturnCode = "NonFatal_NotSpecified";
        //            }
        //            Log"改变状态重新调用joinGame");
        //            //ServerGameSetup.Mode = GameMode.OnlineKitchen;
        //            //ServerGameSetup.Mode = GameMode.Party;
        //            //__instance.TryJoinGame();
        //            ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Offline, null, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnLeaveConfirmedOfflineConnectionState));

        //            IPlayerManager playerManager = GameUtils.RequireManagerInterface<IPlayerManager>();
        //            IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
        //            IOnlineMultiplayerSessionCoordinator onlineMultiplayerSessionCoordinator = onlinePlatformManager.OnlineMultiplayerSessionCoordinator();
        //            if (onlineMultiplayerSessionCoordinator != null)
        //            {
        //                ServerOptions serverOptions = default(ServerOptions);
        //                serverOptions.gameMode = ((!__instance.m_bIsCoop) ? GameMode.Versus : GameMode.Party);
        //                if (__instance.m_lobbyInfo.m_visiblity == OnlineMultiplayerSessionVisibility.ePrivate)
        //                {
        //                    serverOptions.visibility = OnlineMultiplayerSessionVisibility.eClosed;
        //                }
        //                else
        //                {
        //                    serverOptions.visibility = __instance.m_lobbyInfo.m_visiblity;
        //                }
        //                serverOptions.hostUser = playerManager.GetUser(EngagementSlot.One);
        //                serverOptions.connectionMode = __instance.m_lobbyInfo.m_connectionMode;
        //                ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Server, serverOptions, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestConnectionStateServerComplete));
        //                ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Matchmake, new MatchmakeData
        //                {
        //                    gameMode = ((!__instance.m_bIsCoop) ? GameMode.Versus : GameMode.Party),
        //                    User = playerManager.GetUser(EngagementSlot.One),
        //                    connectionMode = __instance.m_lobbyInfo.m_connectionMode
        //                }, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestConnectionStateJoinComplete));
        //            }
        //            //__instance.TryJoinGame();

        //        }
        //    }
        //    return false;
        //}


        [HarmonyPatch(typeof(ClientLobbyFlowController), "TryJoinGame")]
        [HarmonyPrefix]
        private static bool ClientLobbyFlowController_TryJoinGame_Prefix(ClientLobbyFlowController __instance)
        {
            try
            {
                bool flag = ForceHost.ValueList.Value.Equals("强制主机");
                if (flag)
                {
                    Log("强制主机已生效");
                    //_MODEntry.ShowWarningDialog("强制主机已生效。");
                    __instance.HostGame();
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                LogE($"An error occurred: \n{e.Message}");
                LogE($"Stack trace: \n{e.StackTrace}");
                return true;
            }
        }

        [HarmonyPatch(typeof(ClientLobbyFlowController), "HostGame")]
        [HarmonyPostfix]
        private static void ClientLobbyFlowController_HostGame_Prefix(ClientLobbyFlowController __instance)
        {
            try
            {
                if (ServerUserSystem.m_Users.Count > 1) { Log("用户数量大于1,不启用强制客机"); return; }
                bool flag = ForceHost.ValueList.Value.Equals("强制客机");
                if (flag)
                {
                    Log("强制客机已生效");
                    ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Offline, null, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestOfflineStateFollowingFailureComplete));
                    IPlayerManager playerManager = GameUtils.RequireManagerInterface<IPlayerManager>();
                    IOnlinePlatformManager onlinePlatformManager = GameUtils.RequireManagerInterface<IOnlinePlatformManager>();
                    IOnlineMultiplayerSessionCoordinator onlineMultiplayerSessionCoordinator = onlinePlatformManager.OnlineMultiplayerSessionCoordinator();
                    if (onlineMultiplayerSessionCoordinator != null)
                    {
                        //ServerOptions serverOptions = default(ServerOptions);
                        //serverOptions.gameMode = ((!__instance.m_bIsCoop) ? GameMode.Versus : GameMode.Party);
                        //if (__instance.m_lobbyInfo.m_visiblity == OnlineMultiplayerSessionVisibility.ePrivate)
                        //{
                        //    serverOptions.visibility = OnlineMultiplayerSessionVisibility.eClosed;
                        //}
                        //else
                        //{
                        //    serverOptions.visibility = __instance.m_lobbyInfo.m_visiblity;
                        //}
                        //serverOptions.hostUser = playerManager.GetUser(EngagementSlot.One);
                        //serverOptions.connectionMode = __instance.m_lobbyInfo.m_connectionMode;
                        //ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Server, serverOptions, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestConnectionStateServerComplete));
                        ConnectionModeSwitcher.RequestConnectionState(NetConnectionState.Matchmake, new MatchmakeData
                        {
                            gameMode = ((!__instance.m_bIsCoop) ? GameMode.Versus : GameMode.Party),
                            User = playerManager.GetUser(EngagementSlot.One),
                            connectionMode = __instance.m_lobbyInfo.m_connectionMode
                        }, new GenericVoid<IConnectionModeSwitchStatus>(__instance.OnRequestConnectionStateJoinComplete));
                    }
                }
            }
            catch (Exception e)
            {
                LogE($"An error occurred: \n{e.Message}");
                LogE($"Stack trace: \n{e.StackTrace}");
            }
        }

        //private static readonly MethodInfo HostGame = AccessTools.Method(typeof(ClientLobbyFlowController), "HostGame", null, null);
        //// Token: 0x040000D6 RID: 214
        //private static readonly MethodInfo SetState = AccessTools.Method(typeof(ClientLobbyFlowController), "SetState", null, null);
    }
}
