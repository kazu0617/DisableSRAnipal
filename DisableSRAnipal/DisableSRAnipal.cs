using HarmonyLib;
using ResoniteModLoader;
using UnityFrooxEngineRunner;
using FrooxEngine;

namespace DisableSRAnipal;

public partial class DisableSRAnipal : ResoniteMod {
	public override string Name => "DisableSRAnipal";
	public override string Author => "Delta";
	public override string Version => "1.0.0";
	public override string Link => "https://github.com/XDelta/DisableSRAnipal";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> Enabled = new("Enabled", "Should SRAnipal setup be enabled (restart required)", () => true);

	internal static ModConfiguration Config;

	public override void OnEngineInit() {
		Config = GetConfiguration();
		Config.Save(true);

		Harmony harmony = new("net.deltawolf.DisableSRAnipal");

		TryPatchIfTypeExists(harmony, "UnityLaunchOptions", "LaunchOptions");
		TryPatchIfTypeExists(harmony, "ViveProEyeTrackingDriver", "ProEyeTracking");
	}

    private void TryPatchIfTypeExists(Harmony harmony, string typeName, string patchCategory)
    {
		// typeof が使える場合はこちら（グローバル型想定）
		try {
			// 例外が出ればその型は存在しないとみなす
			System.Type t = System.Type.GetType(typeName);
			if (t != null) {
				harmony.PatchCategory(patchCategory);
			}
			Msg($"Type '{typeName}' not found, skipping patching for {patchCategory}.");

		}
		catch {
			// Harmony が null MethodBase にパッチしないように保険
			Warn($"E when patching for {patchCategory}, threw warning: Type '{typeName}' does not exist.");
		}
    }

	[HarmonyPatchCategory("LaunchOptions")]
	[HarmonyPatch(typeof(UnityLaunchOptions), "ForceSRAnipal", MethodType.Getter)]
	private class UnityLaunchOptionsForceSRAnipalPatch {
		public static bool Prefix(ref bool __result) {
			if (Config.GetValue(Enabled)) {
				__result = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatchCategory("ProEyeTracking")]

	[HarmonyPatch(typeof(ViveProEyeTrackingDriver), "ShouldRegister", MethodType.Getter)]
	private class ViveProEyeTrackingDriverShouldRegisterPatch {
		public static bool Prefix(ref bool __result) {
			if (Config.GetValue(Enabled)) {
				__result = false;
				return false;
			}
			return true;
		}
	}
}
