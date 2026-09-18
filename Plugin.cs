using SPTarkov.Reflection.Patching;
using BepInEx;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using System.Reflection;
using BepInEx.Configuration;
using System;

namespace ImprovedSelfcare
{
	public class Globals
	{
		public static Player player { get; private set; }
		public static ActiveHealthController activeHealthController { get; private set; }
		public static void SetPlayer(Player p) => player = p;
		public static void SetPlayerHealthController(ActiveHealthController controller) => activeHealthController = controller;
	}

	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		private void Awake()
		{
			Config.SaveOnConfigSet = true;
			SetupConfig();

			new HealingAutoCancelPatch().Enable();
		}

		internal static ConfigEntry<bool> EnableAutoHealCanceling;

		private void SetupConfig()
		{
			EnableAutoHealCanceling = Config.Bind("Heal", "Enable automatic heal canceling", true);
		}
	}

	internal class HealingAutoCancelPatch : AbstractPatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);
		}

		[PatchPostfix]
		static void PostFix()
		{
			GameWorld gameWorld = Singleton<GameWorld>.Instance;

			Globals.SetPlayer(gameWorld.MainPlayer);
			Globals.SetPlayerHealthController(gameWorld.MainPlayer.ActiveHealthController);
			Globals.activeHealthController.HealthChangedEvent += ActiveHealthController_HealthChangedEvent;
		}

		private static void ActiveHealthController_HealthChangedEvent(EBodyPart bodyPart, float amount, EFT.Ballistics.DamageInfo damageInfo)
		{
			if (damageInfo.DamageType != EDamageType.Medicine)
				return;

			EFT.InventoryLogic.Meds medkitInHands = Globals.player.TryGetItemInHands<EFT.InventoryLogic.Meds>();

			//Try to ignore any healing done by stims and ensure we do not try to cancel fixing a broken limb
			if (medkitInHands != null && !Globals.activeHealthController.IsBodyPartBroken(bodyPart))
			{
				ValueStruct bodyPartHealth = Globals.activeHealthController.GetBodyPartHealth(bodyPart);

				//There might be a better way to check bleeding status
				//This works though
				var effects = Globals.activeHealthController.BodyPartEffects.Effects[bodyPart];
				bool bleeding = effects.ContainsKey("LightBleeding") || effects.ContainsKey("HeavyBleeding");

				//Feels like this is not working correctly
				//Autocancel should trigger when medkit runs out
				bool healingItemDepleted = medkitInHands.MedKitComponent.HpResource < 1;

				if ((bodyPartHealth.AtMaximum && !bleeding) || healingItemDepleted)
					//This is the magical part! Woooaahh
					Globals.activeHealthController.RemoveMedEffect();
			}
		}
	}
}