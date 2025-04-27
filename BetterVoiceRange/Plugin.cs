using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
namespace BetterVoiceRange;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static ConfigEntry<float> lowPassFallOffMultiplier;
    private static ConfigEntry<float> lowPassVolumeMultiplier;

    private static FieldInfo inLobbyMixerField;
    private static FieldInfo lowPassLogicField;
    private static FieldInfo falloffMultiplierField;
    private static FieldInfo volumeMultiplierField;

    private void Awake()
    {
        lowPassFallOffMultiplier = Config.Bind("Options", "LowPassFallOffMultiplier", 1.0f, new ConfigDescription("This is the important value. It's the multiplier applied to voice volume over distance through walls. The higher it is, the greater the voice range. Original game value: 0.8.", new AcceptableValueRange<float>(0f, 3f)));
        lowPassVolumeMultiplier = Config.Bind("Options", "LowPassVolumeMultiplier", 0.5f, new ConfigDescription("Multiplier applied to the voice volume when behind walls. Original game value: 0.5.", new AcceptableValueRange<float>(0f, 3f)));

        Type playerVoiceChatType = typeof(PlayerVoiceChat);
        inLobbyMixerField = playerVoiceChatType.GetField("inLobbyMixer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        lowPassLogicField = playerVoiceChatType.GetField("lowPassLogic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Type audioLowPassLogicType = typeof(AudioLowPassLogic);
        falloffMultiplierField = audioLowPassLogicType.GetField("FalloffMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        volumeMultiplierField = audioLowPassLogicType.GetField("VolumeMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Plugin));
    }

    [HarmonyPatch(typeof(PlayerVoiceChat), "Update")]
    [HarmonyPostfix]
    private static void Update(PlayerVoiceChat __instance)
    {
        bool inLobbyMixer = (bool) inLobbyMixerField?.GetValue(__instance);
        if (inLobbyMixer)
        {
            return;
        }

        AudioLowPassLogic lowPassLogic = lowPassLogicField?.GetValue(__instance) as AudioLowPassLogic;
        falloffMultiplierField.SetValue(lowPassLogic, lowPassFallOffMultiplier.Value);
        volumeMultiplierField.SetValue(lowPassLogic, lowPassVolumeMultiplier.Value);
    }
}
