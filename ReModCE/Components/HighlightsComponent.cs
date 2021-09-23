using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using VRC;
using VRC.Core;

namespace ReModCE.Components
{
    internal class HighlightsComponent : ModComponent
    {
        private HighlightsFXStandalone _friendsHighlights;
        private HighlightsFXStandalone _othersHighlights;

        private ConfigValue<Vector4> FriendsColor;
        private ConfigValue<Vector4> OthersColor;
        private ConfigValue<bool> ESPEnabled;
        private ReQuickToggle _espToggle;

        public HighlightsComponent()
        {
            FriendsColor = new ConfigValue<Vector4>(nameof(FriendsColor), Color.yellow.ToVector4());
            OthersColor = new ConfigValue<Vector4>(nameof(OthersColor), Color.magenta.ToVector4());

            ESPEnabled = new ConfigValue<bool>(nameof(ESPEnabled), false);
            ESPEnabled.OnValueChanged += () => _espToggle.Toggle(ESPEnabled);

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                _espToggle.Interactable = allowed;
                if (!allowed)
                    ESPEnabled.SetValue(false);
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var highlightsFx = HighlightsFX.field_Private_Static_HighlightsFX_0;

            _friendsHighlights = highlightsFx.gameObject.AddComponent<HighlightsFXStandalone>();
            _friendsHighlights.highlightColor = ((Vector4)FriendsColor).ToColor();
            _othersHighlights = highlightsFx.gameObject.AddComponent<HighlightsFXStandalone>();
            _othersHighlights.highlightColor = ((Vector4)OthersColor).ToColor();

            var menu = uiManager.MainMenu.GetSubMenu("Visuals");
            _espToggle = menu.AddToggle("ESP", "Enable ESP (Highlight players through walls)", b =>
            {
                ESPEnabled.SetValue(b);
                ToggleESP(b);
            }, ESPEnabled);
        }

        private void ToggleESP(bool enabled)
        {
            var playerManager = PlayerManager.field_Private_Static_PlayerManager_0;
            if (playerManager == null)
                return;

            foreach (var player in playerManager.GetPlayers())
            {
                HighlightPlayer(player, enabled);
            }
        }

        private void HighlightPlayer(Player player, bool highlighted)
        {
            if (!RiskyFunctionsManager.Instance.RiskyFunctionAllowed)
                return;

            if (player.field_Private_APIUser_0.IsSelf)
                return;

            var selectRegion = player.transform.Find("SelectRegion");
            if (selectRegion == null)
                return;

            GetHighlightsFX(player.field_Private_APIUser_0).Method_Public_Void_Renderer_Boolean_0(selectRegion.GetComponent<Renderer>(), highlighted);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (!ESPEnabled)
                return;

            HighlightPlayer(player, ESPEnabled);
        }

        private HighlightsFXStandalone GetHighlightsFX(APIUser apiUser)
        {
            if (APIUser.IsFriendsWith(apiUser.id))
                return _friendsHighlights;

            return _othersHighlights;
        }
    }
}
