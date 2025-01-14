﻿using System;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace RandomEncounters.Patch
{
    public delegate void OnUpdateEventHandler(World world);
    public delegate void ServerStartupStateChangeEventHandler(LoadPersistenceSystemV2 sender, ServerStartupState.State serverStartupState);
    public delegate void DeathEventHandler(DeathEventListenerSystem sender, NativeArray<DeathEvent> deathEvents);

    public static class ServerEvents
    {
        public static event OnUpdateEventHandler OnUpdate;
        public static event DeathEventHandler OnDeath;
        public static event ServerStartupStateChangeEventHandler OnServerStartupStateChanged;
        
        [HarmonyPatch(typeof(LoadPersistenceSystemV2), nameof(LoadPersistenceSystemV2.SetLoadState))]
        [HarmonyPrefix]
        private static void ServerStartupStateChange_Prefix(ServerStartupState.State loadState, LoadPersistenceSystemV2 __instance)
        {
            try
            {
                OnServerStartupStateChanged?.Invoke(__instance, loadState);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }

        [HarmonyPatch(typeof(ServerTimeSystem_Server), nameof(ServerTimeSystem_Server.OnUpdate))]
        [HarmonyPostfix]
        private static void ServerTimeSystemOnUpdate_Postfix(ServerTimeSystem_Server __instance)
        {
            try
            {
                OnUpdate?.Invoke(__instance.World);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }


        [HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void DeathEventListenerSystemPatch_Postfix(DeathEventListenerSystem __instance)
        {
            try
            {
                var deathEvents =
                    __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
                if (deathEvents.Length > 0)
                {
                    OnDeath?.Invoke(__instance, deathEvents);
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }
    }
}