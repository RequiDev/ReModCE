using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Managers;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.UI;

namespace ReModCE.Components
{
    internal sealed class TeleportComponent : ModComponent
    {
        private static ReUiButton _teleportMenuButton;
        private static ReMenuButton _teleportTargetButton;
        
        private static PageUserInfo _userInfoPage;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var targetMenu = uiManager.TargetMenu;

            var userInfoTransform = VRCUiManagerEx.Instance.MenuContent().transform.Find("Screens/UserInfo");
            _userInfoPage = userInfoTransform.GetComponent<PageUserInfo>();
            
            var buttonContainer = userInfoTransform.Find("Buttons/RightSideButtons/RightUpperButtonColumn/");
            
            _teleportMenuButton = new ReUiButton("Teleport", Vector2.zero, new Vector2(0.68f, 1.2f), TeleportMenuButtonOnClick, buttonContainer);
            ReModCE.SocialMenuButtons.Add(_teleportMenuButton);
            
            _teleportTargetButton = targetMenu.AddButton("Teleport", "Teleports to target.", TeleportTargetButtonOnClick, ResourceManager.GetSprite("remodce.teleport"));

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                _teleportMenuButton.Interactable = allowed;
                _teleportTargetButton.Interactable = allowed;
            };
        }
        
        private void TeleportMenuButtonOnClick()
        {
            var user = _userInfoPage.field_Private_IUser_0;
            if (user == null)
                return;
            
            TeleportToIUser(user);
        }
        
        private void TeleportTargetButtonOnClick()
        {
            var user = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
            if (user == null)
                return;
            
            TeleportToIUser(user);
        }
        
        private void TeleportToIUser(IUser user)
        {
            var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(user.prop_String_0)._vrcplayer;
            if (player == null)
                return;

            var transform = player.transform;
            var playerPosition = transform.position;

            var localTransform = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform;
            localTransform.position = playerPosition;
            
            VRCUiManagerEx.Instance.CloseUi();
        }


        public override void OnSetupUserInfo(APIUser apiUser)
        {
            _teleportMenuButton.Active = RiskyFunctionsManager.Instance.RiskyFunctionAllowed && APIUser.CurrentUser.id != apiUser.id && PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(apiUser.id) != null;
        }
    }
}