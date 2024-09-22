using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using static PriceTweaker.Plugin;

namespace PriceTweaker
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static Plugin PluginInstance;
        public static ManualLogSource LoggerInstance;
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;

            harmony.PatchAll();

            // Finished
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }
    }

    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Terminal.Awake))]
        private static void AwakePostfix(Terminal __instance)
        {
            if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                LoggerInstance.LogDebug("Configuring item prices...");
                foreach (var item in __instance.buyableItemsList)
                {
                    ConfigEntry<int> price = PluginInstance.Config.Bind<int>(item.itemName, "Price", item.creditsWorth, "Price of " + item.itemName + " in credits");
                    item.creditsWorth = price.Value;
                }
            }
        }
    }
}
