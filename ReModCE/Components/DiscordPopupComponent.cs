using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.VRChat;
using UnityEngine;
using VRC.Core;

namespace ReModCE.Components
{
    internal sealed class DiscordPopupComponent : ModComponent
    {
        private bool _fired;
        private ConfigValue<bool> HadDiscordPopup;

        public DiscordPopupComponent()
        {
            HadDiscordPopup = new ConfigValue<bool>(nameof(HadDiscordPopup), false);
        }

        public override void OnAvatarIsReady(VRCPlayer vrcPlayer)
        {
            if (vrcPlayer != VRCPlayer.field_Internal_Static_VRCPlayer_0)
                return;

            if (_fired || HadDiscordPopup)
                return;

            MelonCoroutines.Start(ShowDiscordPopup());
        }

        private IEnumerator ShowDiscordPopup()
        {
            yield return new WaitForSeconds(3f);

            while (APIUser.CurrentUser == null)
                yield return null;

            _fired = true;
            VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.ShowStandardPopupV2("ReMod CE Discord", $"Hi there {APIUser.CurrentUser.displayName},\nReMod CE now has a dedicated Discord server you can join!\nYou can request features, report bugs or just chat with other ReMod CE users.\nIf you don't want to join, just close the popup. You can join later by opening the ReMod CE menu in the main menu.", "Join!",
                () =>
                {
                    Process.Start("https://discord.gg/KdTSGU4jt3");
                });
            HadDiscordPopup.SetValue(true);
        }
    }
}
