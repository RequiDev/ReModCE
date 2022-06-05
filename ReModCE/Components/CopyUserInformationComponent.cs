using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.VRChat;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.UI;

namespace ReModCE.Components
{
    internal sealed class CopyUserInformationComponent : ModComponent
    {
        private static ReUiButton _copyAvatarIDButton;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            uiManager.TargetMenu.AddButton("Copy User ID", "Copies the selected users User ID.", () =>
            {
                var user = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                if (user == null)
                    return;

                GUIUtility.systemCopyBuffer = user.GetUserID();
            }, ResourceManager.GetSprite("remodce.copy"));

            uiManager.TargetMenu.AddButton("Copy Avatar ID", "Copies the selected users Avatar ID", () =>
            {
                var user = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                if (user == null)
                    return;

                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(user.prop_String_0);
                if (player == null) return;

                var apiAvatar = player.GetApiAvatar();
                if (apiAvatar == null)
                    return;

                GUIUtility.systemCopyBuffer = apiAvatar.id;
            }, ResourceManager.GetSprite("remodce.copy"));
        }

        public override void OnUiManagerInitEarly()
        {
            var userInfoTransform = VRCUiManagerEx.Instance.MenuContent().transform.Find("Screens/UserInfo");

            var pageUserInfo = userInfoTransform.GetComponent<PageUserInfo>();
            var buttonContainer = userInfoTransform.Find("Buttons/RightSideButtons/RightUpperButtonColumn/");

            ReModCE.SocialMenuButtons.Add(new ReUiButton("Copy User ID", Vector2.zero, new Vector2(0.68f, 1.2f), () =>
            {
                var user = pageUserInfo.field_Private_IUser_0;
                if (user == null)
                    return;

                GUIUtility.systemCopyBuffer = user.GetUserID();
            }, buttonContainer));

            _copyAvatarIDButton = new ReUiButton("Copy Avatar ID", Vector2.zero, new Vector2(0.68f, 1.2f), () =>
            {
                var user = pageUserInfo.field_Private_IUser_0;
                if (user == null)
                    return;
                
                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(user.prop_String_0);
                if (player == null) return;

                var apiAvatar = player.GetApiAvatar();
                if (apiAvatar == null)
                    return;

                GUIUtility.systemCopyBuffer = apiAvatar.id;
            }, buttonContainer);
            
            ReModCE.SocialMenuButtons.Add(_copyAvatarIDButton);
        }

        public override void OnSetupUserInfo(APIUser apiUser)
        {
            _copyAvatarIDButton.Active = APIUser.CurrentUser.id != apiUser.id && PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(apiUser.id) != null;
        }
    }
}
