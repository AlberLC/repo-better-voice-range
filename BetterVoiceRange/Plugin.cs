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
    private static FieldInfo volumeMultiplierField;
    private static FieldInfo falloffMultiplierField;

    private void Awake()
    {        
        lowPassFallOffMultiplier = Config.Bind("Options", "LowPassFallOffMultiplier", 1f, new ConfigDescription("This is the important value. It's the multiplier applied to voice volume over distance through walls. The higher it is, the greater the voice range. Original game value: 0.8.", new AcceptableValueRange<float>(0f, 3f)));
        lowPassVolumeMultiplier = Config.Bind("Options", "LowPassVolumeMultiplier", 0.5f, new ConfigDescription("Multiplier applied to the voice volume when behind walls. Original game value: 0.5.", new AcceptableValueRange<float>(0f, 3f)));        
        spatialBlend = Config.Bind("Options", "SpatialBlend", 0.1f, new ConfigDescription("This value might bug the rest. Controls how much voice volume decreases with distance. Original game value: 1.0.", new AcceptableValueRange<float>(0f, 3f)));        

        Type playerVoiceChatType = typeof(PlayerVoiceChat);
        audioSourceField = playerVoiceChatType.GetField("audioSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        ttsAudioSourceField = playerVoiceChatType.GetField("ttsAudioSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        lowPassLogicField = playerVoiceChatType.GetField("lowPassLogic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Type audioLowPassLogicType = typeof(AudioLowPassLogic);
        volumeMultiplierField = audioLowPassLogicType.GetField("VolumeMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        falloffMultiplierField = audioLowPassLogicType.GetField("FalloffMultiplier", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Plugin));
    }

    [HarmonyPatch(typeof(PlayerVoiceChat), "Update")]
    [HarmonyPostfix]
    private static void UpdateSpatialBlend(PlayerVoiceChat __instance)
    {
        AudioSource audioSource = audioSourceField?.GetValue(__instance) as AudioSource;
        AudioSource ttsAudioSource = ttsAudioSourceField?.GetValue(__instance) as AudioSource;        
        audioSource.spatialBlend = spatialBlend.Value;
        ttsAudioSource.spatialBlend = audioSource.spatialBlend;

        AudioLowPassLogic lowPassLogic = lowPassLogicField?.GetValue(__instance) as AudioLowPassLogic;
        volumeMultiplierField.SetValue(lowPassLogic, lowPassVolumeMultiplier.Value);        
        falloffMultiplierField.SetValue(lowPassLogic, lowPassFallOffMultiplier.Value);
    }
}
