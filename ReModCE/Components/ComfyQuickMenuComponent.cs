using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using ReModCE.Core;
using HarmonyLib;
using ReModCE.Managers;
using ReModCE.UI;

namespace ReModCE.Components
{
    internal class ComfyQuickMenuComponent : ModComponent
    {
        private static ConfigValue<bool> ComfyQuickMenuEnabled;
        private static ReQuickToggle _comfyQMToggle;

        public ComfyQuickMenuComponent()
        {
            ComfyQuickMenuEnabled = new ConfigValue<bool>(nameof(ComfyQuickMenuEnabled), false);
            ComfyQuickMenuEnabled.OnValueChanged += () => _comfyQMToggle.Toggle(ComfyQuickMenuEnabled);
            var harmony = new HarmonyLib.Harmony("ReModCE");
            harmony.Patch(typeof(QuickMenu).GetMethod(nameof(QuickMenu.Method_Private_Void_Boolean_0)), new HarmonyMethod(typeof(ComfyQuickMenuComponent).GetMethod(nameof(SetupForDesktopOrHMDPatch), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var qolMenu = uiManager.MainMenu.AddSubMenu("QoL", "Access Quality of Life adjustments.");
            _comfyQMToggle = qolMenu.AddToggle("Comfy QuickMenu", "Always force the one handed QuickMenu.",
                ComfyQuickMenuEnabled.SetValue, ComfyQuickMenuEnabled);
        }

        private static void SetupForDesktopOrHMDPatch(ref bool __0)
        {
            if (ComfyQuickMenuEnabled)
            {
                __0 = true;
            }
        }
    }
}
