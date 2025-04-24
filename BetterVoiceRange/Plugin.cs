using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
namespace BetterVoiceRange;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static ConfigEntry<float> spatialBlend;
    private static ConfigEntry<float> lowPassVolumeMultiplier;
    private static ConfigEntry<float> lowPassFallOffMultiplier;

    private static FieldInfo audioSourceField;
    private static FieldInfo ttsAudioSourceField;
    private static FieldInfo lowPassLogicField;

    private void Awake()
    {
        spatialBlend = Config.Bind("Options", "SpatialBlend", 0.5f, "Controls how much voice volume decreases with distance. The original game value is 1.0.");
        lowPassVolumeMultiplier = Config.Bind("Options", "LowPassVolumeMultiplier", 0.75f, "Multiplier applied to the voice volume when behind walls. The original game value is 0.5.");
        lowPassFallOffMultiplier = Config.Bind("Options", "LowPassFallOffMultiplier", 0.9f, "Controls how quickly the low-pass effect increases with distance when the speaker is behind a wall. The original game value is 0.8.");

        Type playerVoiceChatType = typeof(PlayerVoiceChat);

        audioSourceField = playerVoiceChatType.GetField("audioSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        ttsAudioSourceField = playerVoiceChatType.GetField("ttsAudioSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        lowPassLogicField = playerVoiceChatType.GetField("lowPassLogic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Plugin));
    }

    [HarmonyPatch(typeof(PlayerVoiceChat), "Update")]
    [HarmonyPostfix]
    private static void UpdateSpatialBlend(PlayerVoiceChat __instance)
    {
        AudioSource audioSource = audioSourceField?.GetValue(__instance) as AudioSource;
        AudioSource ttsAudioSource = ttsAudioSourceField?.GetValue(__instance) as AudioSource;
        AudioLowPassLogic lowPassLogic = lowPassLogicField?.GetValue(__instance) as AudioLowPassLogic;

        audioSource.spatialBlend = spatialBlend.Value;
        ttsAudioSource.spatialBlend = audioSource.spatialBlend;

        FieldInfo volumeMultiplierField = lowPassLogic.GetType().GetField("VolumeMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        volumeMultiplierField.SetValue(lowPassLogic, lowPassVolumeMultiplier.Value);

        FieldInfo falloffMultiplierField = lowPassLogic.GetType().GetField("FalloffMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        falloffMultiplierField.SetValue(lowPassLogic, lowPassFallOffMultiplier.Value);
    }
}
