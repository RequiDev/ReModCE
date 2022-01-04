using ReMod.Core;
using ReMod.Core.Managers;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MelonLoader;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.Unity;
using ReMod.Core.VRChat;
using UnityEngine;
using VRC.Core;

namespace ReModCE.Components
{
    [ComponentPriority(int.MaxValue)]
    internal sealed class RestartButtonComponent : ModComponent
    {
        private string _lastRoomID;

        private bool _shouldTeleport;

        private ConfigValue<bool> ShouldRejoin;
        private ReMenuToggle _shouldRejoinToggle;

        private ConfigValue<bool> ShouldTeleport;
        private ReMenuToggle _shouldTeleportToggle;

        private ConfigValue<bool> ShouldConfirm;
        private ReMenuToggle _shouldConfirmToggle;

        private Vector3 _toTeleportTo;
        private Vector3 _toRotateTo;

        private bool _isVR = false;
        
        public RestartButtonComponent()
        {
            ShouldRejoin = new ConfigValue<bool>(nameof(ShouldRejoin), false);
            ShouldRejoin.OnValueChanged += () => _shouldRejoinToggle?.Toggle(ShouldRejoin);

            ShouldTeleport = new ConfigValue<bool>(nameof(ShouldTeleport), true);
            ShouldTeleport.OnValueChanged += () => _shouldTeleportToggle?.Toggle(ShouldTeleport);

            ShouldConfirm = new ConfigValue<bool>(nameof(ShouldConfirm), true);
            ShouldConfirm.OnValueChanged += () => _shouldConfirmToggle?.Toggle(ShouldConfirm);
            
            ParseCommandLine();
        }

        public void ParseCommandLine()
        {
            var checkForVR = Array.Find(Environment.GetCommandLineArgs(), m => m.Contains("--no-vr"));
            _isVR = string.IsNullOrEmpty(checkForVR);
            
            var origPosCommand = Array.Find(Environment.GetCommandLineArgs(), m => m.Contains("-origpos"));
            if (string.IsNullOrEmpty(origPosCommand)) return;

            _shouldTeleport = true;
            _toTeleportTo = ReconstructVector3FromString(origPosCommand.Split('=')[1]);

            var origRotCommand = Array.Find(Environment.GetCommandLineArgs(), m => m.Contains("-origrot"));
            if (string.IsNullOrEmpty(origRotCommand)) return;

            _toRotateTo = ReconstructVector3FromString(origRotCommand.Split('=')[1]);
        }

        public override void OnEnterWorld(ApiWorld world, ApiWorldInstance instance)
        {
            _lastRoomID = $"vrchat://launch?id={world.id}:{instance.instanceId}";
        }

        public override void OnAvatarIsReady(VRCPlayer player)
        {
            if (_shouldTeleport && player == VRCPlayer.field_Internal_Static_VRCPlayer_0)
            {
                _shouldTeleport = false;
                MelonCoroutines.Start(WaitUntilTeleport(player));
            }
        }

        private IEnumerator WaitUntilTeleport(VRCPlayer player)
        {
            yield return new WaitForSeconds(1);
            player.transform.position = _toTeleportTo;
            player.transform.eulerAngles = _toRotateTo;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var utilityPage = uiManager.MainMenu.GetCategoryPage("Utility").AddCategory("Application");
            var template = $"Restart in {(_isVR ? "Desktop" : "VR")} mode";

            utilityPage.AddButton("Restart", "Restart the game.", () => Restart(false), ResourceManager.GetSprite("remodce.reload"));
            utilityPage.AddButton(template, template+".", () => Restart(true), ResourceManager.GetSprite("remodce.reload"));
            
            ReModCE.WingMenu.AddButton("Restart", "Restart the game.", RestartConfirm, ResourceManager.GetSprite("remodce.reload"), false);

            _shouldRejoinToggle = utilityPage.AddToggle("Rejoin World", "On restart, rejoin the current world.", ShouldRejoin);
            _shouldTeleportToggle = utilityPage.AddToggle("Teleport Back", "On restart, teleport back to the original location.", ShouldTeleport);
            _shouldConfirmToggle = utilityPage.AddToggle("Confirm Restart", "Ask for confirmation to restart.", ShouldConfirm);
        }

        public Vector3 ReconstructVector3FromString(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Input string is null or empty.");
            var realString = input.Split(',');
            if (realString.Length != 3) throw new ArgumentException("Input string contained less/more than 3 floats.");

            if (float.TryParse(realString[0], out float x) && float.TryParse(realString[1], out float y) &&
                float.TryParse(realString[2], out float z))
            {
                return new Vector3(x, y, z);
            }

            throw new ArgumentException("Input string did not contain floats.");
        }
        
        private void RestartConfirm()
        {
            if (ShouldConfirm)
            {
                var template = $"Restart in {(_isVR ? "Desktop" : "VR")} mode";
                QuickMenuEx.Instance.ShowConfirmDialogWithCancel("Restart",
                    "Are you sure you want to restart?",
                    template,
                    "Restart",
                    "Cancel",
                    ()=>Restart(true),
                    ()=>Restart(),
                    null);
            }
            else
            {
                Restart();
            }
        }

        public void Restart(bool swap = false)
        {
            var originalargs = Environment.GetCommandLineArgs();
            string path = $"\"{Directory.GetCurrentDirectory()}\\VRChat.exe\"";

            var args = originalargs.Where(t =>
                    !t.Contains("vrchat://launch?id=") &&
                    !t.Contains("-origpos") &&
                    !t.Contains("-origrot"))
                .ToList();

            if (swap)
            {
                if (_isVR)
                {
                    args.Add("--no-vr");
                }
                else
                {
                    args.Remove("--no-vr");
                }
            }

            if (ShouldRejoin)
            {
                args.Add(_lastRoomID);
                if (ShouldTeleport)
                {
                    var position = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position.ToCleanString();
                    args.Add($"-origpos={position}");

                    var rotation = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.eulerAngles.ToCleanString();
                    args.Add($"-origrot={rotation}");
                }
            }

            var cmd = String.Join(" ", args);
            cmd = cmd.Replace(path, "");

            System.Diagnostics.Process.Start(path, cmd);
            Application.Quit();
        }
    }
}
