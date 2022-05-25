using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.UI.Wings;
using ReMod.Core.Unity;
using ReMod.Core.VRChat;
using ReModCE.Managers;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace ReModCE.Components
{
    internal class HighlightsComponent : ModComponent
    {
        private HighlightsFXStandalone _friendsHighlights;
        private HighlightsFXStandalone _othersHighlights;

        private ConfigValue<Color> FriendsColor;
        private ConfigValue<Color> OthersColor;
        private ConfigValue<bool> ESPEnabled;
        private ConfigValue<KeyCode> ESPHotkey;
        
        private ReMirroredWingToggle _espMirroredToggle;
        private ReMenuToggle _espToggle;
        private ReMenuButton _friendsColorButton;
        private ReMenuButton _othersColorButton;

        public HighlightsComponent()
        {
            FriendsColor = new ConfigValue<Color>(nameof(FriendsColor), Color.yellow);
            OthersColor = new ConfigValue<Color>(nameof(OthersColor), Color.magenta);

            ESPEnabled = new ConfigValue<bool>(nameof(ESPEnabled), false);
            ESPEnabled.OnValueChanged += () => _espToggle.Toggle(ESPEnabled);

            ESPHotkey = new ConfigValue<KeyCode>(nameof(ESPHotkey), KeyCode.E);

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                if (_espToggle != null)
                {
                    _espToggle.Interactable = allowed;
                }
                if(_espMirroredToggle != null)
                {
                    _espMirroredToggle.Interactable = allowed;
                }
                
                if (!allowed)
                    ESPEnabled.SetValue(false);
            };
        }

        public override void OnUiManagerInitEarly()
        {
            var highlightsFx = HighlightsFX.field_Private_Static_HighlightsFX_0;

            _friendsHighlights = highlightsFx.gameObject.AddComponent<HighlightsFXStandalone>();
            _friendsHighlights.highlightColor = FriendsColor;
            _othersHighlights = highlightsFx.gameObject.AddComponent<HighlightsFXStandalone>();
            _othersHighlights.highlightColor = OthersColor;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("ESP/Highlights");
            _espToggle = menu.AddToggle("ESP/Highlights", "Enable ESP (Highlight players through walls)", b =>
            {
                ESPEnabled.SetValue(b);
                ToggleESP(b);
            }, ESPEnabled);

            _espMirroredToggle = ReModCE.WingMenu.AddToggle("ESP", "Enable/Disable ESP", ESPEnabled.SetValue, ESPEnabled);
            
            _friendsColorButton = menu.AddButton($"<color=#{FriendsColor.Value.ToHex()}>Friends</color> Color",
                $"Set your <color=#{FriendsColor.Value.ToHex()}>friends</color> highlight color",
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowColorInputPopup(_friendsColorButton, "Friends", FriendsColor);
                }, ResourceManager.GetSprite("remodce.palette"));

            _othersColorButton = menu.AddButton($"<color=#{OthersColor.Value.ToHex()}>Others</color> Color",
                $"Set <color=#{OthersColor.Value.ToHex()}>other</color> peoples highlight color",
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowColorInputPopup(_othersColorButton, "Others", OthersColor);
                }, ResourceManager.GetSprite("remodce.palette"));
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

        public override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(ESPHotkey))
            {
                ESPEnabled.SetValue(!ESPEnabled);
            }
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
