using System;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using VRC;
using VRC.Core;

namespace ReModCE.Components
{
    internal sealed class DisableChairComponent : ModComponent
    {
        private static ConfigValue<bool> ChairsDisabled;

        private static ReMenuToggle _disableChairToggle;

        public DisableChairComponent()
        {
            if (ReModCE.IsComponentToggleLoaded)
            {
                ReLogger.Msg(ConsoleColor.Yellow, "Found ComponentToggle Mod. Not loading DisableChairComponent.");
                return;
            }

            ReModCE.Harmony.Patch(typeof(VRC_StationInternal).GetMethod(nameof(VRC_StationInternal.Method_Public_Boolean_Player_Boolean_0)),
                GetLocalPatch(nameof(PlayerCanUseStation)));

            ChairsDisabled = new ConfigValue<bool>(nameof(ChairsDisabled), false);
            ChairsDisabled.OnValueChanged += () => _disableChairToggle.Toggle(ChairsDisabled);
        }

        private static bool PlayerCanUseStation(ref bool __result, VRC_StationInternal __instance, Player __0, bool __1)
        {
            if (!ChairsDisabled) return true;
            if (__0 == null) return true;
            if (__0.GetAPIUser().id != APIUser.CurrentUser.id) return true;

            __result = false;
            return false;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var othersMenu = uiManager.MainMenu.GetCategoryPage("Utility").GetCategory("Quality of Life");
            _disableChairToggle = othersMenu.AddToggle("Disable Chairs", "Toggle Chairs. Because fuck chairs.", ChairsDisabled);
        }
    }
}
