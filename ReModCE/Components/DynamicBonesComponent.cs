using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.DataModel;

namespace ReModCE.Components
{
    internal class DynamicBonesComponent : ModComponent
    {
        [Flags]
        internal enum CollisionFlag
        {
            Self = (1 << 0),
            Whitelist = (1 << 1),
            Friends = (1 << 2),
            Others = (1 << 3)
        }
        
        internal enum ColliderOption
        {
            None = 0,
            Hands,
            HandsAndLowerBody, // if you need to kick someone's ears off
            All
        }

        internal class Settings
        {
            private const string SavePath = "UserData/ReModCE";
            private const string SaveFile = "dynamicbones.json";

            public Settings()
            {
                Directory.CreateDirectory(SavePath);
            }
            public static Settings FromFile(string filePath = SavePath + "/" + SaveFile)
            {
                if (!File.Exists(filePath)) return new Settings();

                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filePath));
                if (settings == null) return new Settings();

                settings._whitelistedUsers = settings._whitelistedUsers.Distinct().ToList();
                settings.Save();
                return settings;
            }

            private void Save()
            {
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }
                File.WriteAllText($"{SavePath}/{SaveFile}", JsonConvert.SerializeObject(this));
            }

            public CollisionFlag OwnCollisionFlag
            {
                get => _ownCollisionFlag;
                set
                {
                    _ownCollisionFlag = value;
                    Save();
                }
            }

            public CollisionFlag WhitelistCollisionFlag
            {
                get => _whitelistCollisionFlag;
                set
                {
                    _whitelistCollisionFlag = value;
                    Save();
                }
            }

            public CollisionFlag FriendsCollisionFlag
            {
                get => _friendsCollisionFlag;
                set
                {
                    _friendsCollisionFlag = value;
                    Save();
                }
            }

            public CollisionFlag OthersCollisionFlag
            {
                get => _othersCollisionFlag;
                set
                {
                    _othersCollisionFlag = value;
                    Save();
                }
            }

            public ColliderOption OwnColliderOption
            {
                get => _ownColliderOption;
                set
                {
                    _ownColliderOption = value;
                    Save();
                }
            }

            public ColliderOption WhitelistColliderOption
            {
                get => _whitelistColliderOption;
                set
                {
                    _whitelistColliderOption = value;
                    Save();
                }
            }

            public ColliderOption FriendsColliderOption
            {
                get => _friendsColliderOption;
                set
                {
                    _friendsColliderOption = value;
                    Save();
                }
            }

            public ColliderOption OthersColliderOption
            {
                get => _othersColliderOption;
                set
                {
                    _othersColliderOption = value;
                    Save();
                }
            }

            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    Save();
                }
            }

            public bool AutoReloadAvatars
            {
                get => _autoReloadAvatars;
                set
                {
                    _autoReloadAvatars = value;
                    Save();
                }
            }
            
            public float MaxRadius
            {
                get => _maxRadius;
                set
                {
                    _maxRadius = value;
                    Save();
                }
            }

            public List<string> WhitelistedUsers => _whitelistedUsers;

            public bool AlwaysIncludeOwnHead
            {
                get => _alwaysIncludeOwnHead;
                set
                {
                    _alwaysIncludeOwnHead = value;
                    Save();
                }
            }

            public bool AlwaysIncludeWhitelistedHead
            {
                get => _alwaysIncludeWhitelistedHead;
                set
                {
                    _alwaysIncludeWhitelistedHead = value;
                    Save();
                }
            }

            public bool AlwaysIncludeFriendsHead
            {
                get => _alwaysIncludeFriendsHead;
                set
                {
                    _alwaysIncludeFriendsHead = value;
                    Save();
                }
            }

            public bool AlwaysIncludeOthersHead
            {
                get => _alwaysIncludeOthersHead;
                set
                {
                    _alwaysIncludeOthersHead = value;
                    Save();
                }
            }

            public void WhitelistUser(string userId)
            {
                if (!IsWhitelisted(userId))
                {
                    _whitelistedUsers.Add(userId);
                    Save();
                }
            }

            public void RemoveWhitelist(string userId)
            {
                if (_whitelistedUsers.Remove(userId))
                {
                    Save();
                }
            }

            public bool IsWhitelisted(string userId)
            {
                return _whitelistedUsers.Contains(userId);
            }


            private bool _enabled = false;
            private float _maxRadius = 1f;
            private bool _autoReloadAvatars = true;

            private List<string> _whitelistedUsers = new List<string>
            {
                "usr_00000000-0000-0000-0000-000000000000"
            };

            private CollisionFlag _ownCollisionFlag = CollisionFlag.Self | CollisionFlag.Friends | CollisionFlag.Whitelist;
            private CollisionFlag _whitelistCollisionFlag = CollisionFlag.Self;
            private CollisionFlag _friendsCollisionFlag = CollisionFlag.Self | CollisionFlag.Friends;
            private CollisionFlag _othersCollisionFlag = CollisionFlag.Self;

            private ColliderOption _ownColliderOption = ColliderOption.HandsAndLowerBody;
            private ColliderOption _whitelistColliderOption = ColliderOption.HandsAndLowerBody;
            private ColliderOption _friendsColliderOption = ColliderOption.HandsAndLowerBody;
            private ColliderOption _othersColliderOption = ColliderOption.Hands;

            private bool _alwaysIncludeOwnHead = false;
            private bool _alwaysIncludeWhitelistedHead = false;
            private bool _alwaysIncludeFriendsHead = false;
            private bool _alwaysIncludeOthersHead = false;
        }

        private readonly Settings _settings;

        private ReMenuButton _maxRadiusButton;

        private ReMenuToggle _whitelistToggle;
        private ReMenuButton _ownColliderOptionButton;
        private ReMenuButton _whitelistedColliderOptionButton;
        private ReMenuButton _friendsColliderOptionButton;
        private ReMenuButton _othersColliderOptionButton;

        private readonly List<DynamicBone> _ownDynamicBones = new List<DynamicBone>();
        private readonly List<DynamicBoneCollider> _ownDynamicBoneColliders = new List<DynamicBoneCollider>();

        private readonly List<DynamicBone> _whitelistedDynamicBones = new List<DynamicBone>();
        private readonly List<DynamicBoneCollider> _whitelistedBoneColliders = new List<DynamicBoneCollider>();

        private readonly List<DynamicBone> _friendsDynamicBones = new List<DynamicBone>();
        private readonly List<DynamicBoneCollider> _friendsDynamicBoneColliders = new List<DynamicBoneCollider>();

        private readonly List<DynamicBone> _othersDynamicBones = new List<DynamicBone>();
        private readonly List<DynamicBoneCollider> _othersDynamicBoneColliders = new List<DynamicBoneCollider>();

        public DynamicBonesComponent()
        {
            _settings = Settings.FromFile();
        }

        public override void OnSelectUser(IUser user, bool isRemote)
        {
            if (isRemote)
                return;

            _whitelistToggle.Toggle(_settings.IsWhitelisted(user.prop_String_0), false, true);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _whitelistToggle = uiManager.TargetMenu.AddToggle("Whitelist GDB",
                "Enables global dynamic bones for this person.",
                _ =>
                {
                    var selectedUser = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                    if (selectedUser == null)
                        return;

                    WhitelistUser(selectedUser.prop_String_0);
                });

            uiManager.TargetMenu.AddButton("Reload Avatar", "Reload this users avatar", () =>
            {
                var selectedUser = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                if (selectedUser == null)
                    return;

                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(selectedUser.prop_String_0);
                player?.GetVRCPlayer()?.ReloadAvatar();
            }, ResourceManager.GetSprite("remodce.reload"));

            var menu = uiManager.MainMenu.GetMenuPage("DynamicBones");
            menu.AddToggle("Enabled", "Enable/Disable global dynamic bones", ToggleDynamicBones,
                _settings.Enabled);
            _maxRadiusButton = menu.AddButton($"Max Collider Radius: {_settings.MaxRadius}", "Ignore any colliders that are bigger than this", PromptMaxRadiusInput, ResourceManager.GetSprite("remodce.radius"));
            menu.AddToggle("Auto Reload Avatars", "Automatically reload all avatars when changing settings",
                b =>
                {
                    _settings.AutoReloadAvatars = b;
                    if (_settings.AutoReloadAvatars)
                    {
                        VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                    }
                }, _settings.AutoReloadAvatars);
            menu.AddButton("Reload All Avatars", "Reload every users avatar. Necessary to apply changed dynamic bones settings.",
            () => VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars(), ResourceManager.GetSprite("remodce.reload"));

            var ownMenu = menu.AddMenuPage("Self Options", "Adjust how your colliders affect others", ResourceManager.GetSprite("remodce.cogwheel"));
            _ownColliderOptionButton = ownMenu.AddButton($"Colliders: {_settings.OwnColliderOption}", "Choose which colliders are applied", () =>
            {
                var o = _settings.OwnColliderOption;
                CycleColliderOption(_ownColliderOptionButton, ref o);
                _settings.OwnColliderOption = o;

                _ownDynamicBoneColliders.Clear(); // clear this because we might have colliders we don't want in here
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            });
            ownMenu.AddToggle("Always Include Head", "Always include head collider",
                b =>
                {
                    _settings.AlwaysIncludeOwnHead = b;
                    if (_settings.AutoReloadAvatars)
                    {
                        VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                    }
                }, _settings.AlwaysIncludeOwnHead);
            ownMenu.AddToggle("To Self", "Add colliders to self", toggled =>
            {
                if (toggled)
                {
                    _settings.OwnCollisionFlag |= CollisionFlag.Self;
                }
                else
                {
                    _settings.OwnCollisionFlag &= ~CollisionFlag.Self;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OwnCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self);
            ownMenu.AddToggle("To Whitelisted", "Add colliders to whitelisted users", toggled =>
            {
                if (toggled)
                {
                    _settings.OwnCollisionFlag |= CollisionFlag.Whitelist;
                }
                else
                {
                    _settings.OwnCollisionFlag &= ~CollisionFlag.Whitelist;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OwnCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist);
            ownMenu.AddToggle("To Friends", "Add colliders to friends", toggled =>
            {
                if (toggled)
                {
                    _settings.OwnCollisionFlag |= CollisionFlag.Friends;
                }
                else
                {
                    _settings.OwnCollisionFlag &= ~CollisionFlag.Friends;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OwnCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends);
            ownMenu.AddToggle("To Others", "Add colliders to others", toggled =>
            {
                if (toggled)
                {
                    _settings.OwnCollisionFlag |= CollisionFlag.Others;
                }
                else
                {
                    _settings.OwnCollisionFlag &= ~CollisionFlag.Others;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OwnCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others);


            var whitelistedMenu = menu.AddMenuPage("Whitelisted Options", "Adjust how whitelisted users colliders affect others and you", ResourceManager.GetSprite("remodce.cogwheel"));
            _whitelistedColliderOptionButton = whitelistedMenu.AddButton($"Colliders: {_settings.WhitelistColliderOption}", "Choose which colliders are applied", () =>
            {
                var o = _settings.WhitelistColliderOption;
                CycleColliderOption(_whitelistedColliderOptionButton, ref o);
                _settings.WhitelistColliderOption = o;

                _whitelistedBoneColliders.Clear(); // clear this because we might have colliders we don't want in here
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            });
            whitelistedMenu.AddToggle("Always Include Head", "Always include head collider",
                b =>
                {
                    _settings.AlwaysIncludeWhitelistedHead = b;
                    if (_settings.AutoReloadAvatars)
                    {
                        VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                    }
                }, _settings.AlwaysIncludeWhitelistedHead);
            whitelistedMenu.AddToggle("To Self", "Add colliders to self", toggled =>
            {
                if (toggled)
                {
                    _settings.WhitelistCollisionFlag |= CollisionFlag.Self;
                }
                else
                {
                    _settings.WhitelistCollisionFlag &= ~CollisionFlag.Self;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.WhitelistCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self);
            whitelistedMenu.AddToggle("To Whitelisted", "Add colliders to whitelisted users", toggled =>
            {
                if (toggled)
                {
                    _settings.WhitelistCollisionFlag |= CollisionFlag.Whitelist;
                }
                else
                {
                    _settings.WhitelistCollisionFlag &= ~CollisionFlag.Whitelist;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.WhitelistCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist);
            whitelistedMenu.AddToggle("To Friends", "Add colliders to friends", toggled =>
            {
                if (toggled)
                {
                    _settings.WhitelistCollisionFlag |= CollisionFlag.Friends;
                }
                else
                {
                    _settings.WhitelistCollisionFlag &= ~CollisionFlag.Friends;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.WhitelistCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends);
            whitelistedMenu.AddToggle("To Others", "Add colliders to others", toggled =>
            {
                if (toggled)
                {
                    _settings.WhitelistCollisionFlag |= CollisionFlag.Others;
                }
                else
                {
                    _settings.WhitelistCollisionFlag &= ~CollisionFlag.Others;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.WhitelistCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others);

            var friendsMenu = menu.AddMenuPage("Friends Options", "Adjust how friends colliders affect others and you", ResourceManager.GetSprite("remodce.cogwheel"));
            _friendsColliderOptionButton = friendsMenu.AddButton($"Colliders: {_settings.FriendsColliderOption}", "Choose which colliders are applied", () =>
            {
                var o = _settings.FriendsColliderOption;
                CycleColliderOption(_friendsColliderOptionButton, ref o);
                _settings.FriendsColliderOption = o;

                _friendsDynamicBoneColliders.Clear(); // clear this because we might have colliders we don't want in here
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            });
            friendsMenu.AddToggle("Always Include Head", "Always include head collider",
                b =>
                {
                    _settings.AlwaysIncludeFriendsHead = b;
                    if (_settings.AutoReloadAvatars)
                    {
                        VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                    }
                }, _settings.AlwaysIncludeFriendsHead);
            friendsMenu.AddToggle("To Self", "Add colliders to self", toggled =>
            {
                if (toggled)
                {
                    _settings.FriendsCollisionFlag |= CollisionFlag.Self;
                }
                else
                {
                    _settings.FriendsCollisionFlag &= ~CollisionFlag.Self;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.FriendsCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self);
            friendsMenu.AddToggle("To Whitelisted", "Add colliders to whitelisted users", toggled =>
            {
                if (toggled)
                {
                    _settings.FriendsCollisionFlag |= CollisionFlag.Whitelist;
                }
                else
                {
                    _settings.FriendsCollisionFlag &= ~CollisionFlag.Whitelist;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.FriendsCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist);
            friendsMenu.AddToggle("To Friends", "Add colliders to friends", toggled =>
            {
                if (toggled)
                {
                    _settings.FriendsCollisionFlag |= CollisionFlag.Friends;
                }
                else
                {
                    _settings.FriendsCollisionFlag &= ~CollisionFlag.Friends;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.FriendsCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends);
            friendsMenu.AddToggle("To Others", "Add colliders to others", toggled =>
            {
                if (toggled)
                {
                    _settings.FriendsCollisionFlag |= CollisionFlag.Others;
                }
                else
                {
                    _settings.FriendsCollisionFlag &= ~CollisionFlag.Others;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.FriendsCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others);


            var othersMenu = menu.AddMenuPage("Others Options", "Adjust how others colliders affect others and you", ResourceManager.GetSprite("remodce.cogwheel"));
            _othersColliderOptionButton = othersMenu.AddButton($"Colliders: {_settings.OthersColliderOption}", "Choose which colliders are applied", () =>
            {
                var o = _settings.OthersColliderOption;
                CycleColliderOption(_othersColliderOptionButton, ref o);
                _settings.OthersColliderOption = o;

                _ownDynamicBoneColliders.Clear(); // clear this because we might have colliders we don't want in here
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            });
            othersMenu.AddToggle("Always Include Head", "Always include head collider",
                b =>
                {
                    _settings.AlwaysIncludeOthersHead = b;
                    if (_settings.AutoReloadAvatars)
                    {
                        VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                    }
                }, _settings.AlwaysIncludeOthersHead);
            othersMenu.AddToggle("To Self", "Add colliders to self", toggled =>
            {
                if (toggled)
                {
                    _settings.OthersCollisionFlag |= CollisionFlag.Self;
                }
                else
                {
                    _settings.OthersCollisionFlag &= ~CollisionFlag.Self;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OthersCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self);
            othersMenu.AddToggle("To Whitelisted", "Add colliders to whitelisted users", toggled =>
            {
                if (toggled)
                {
                    _settings.OthersCollisionFlag |= CollisionFlag.Whitelist;
                }
                else
                {
                    _settings.OthersCollisionFlag &= ~CollisionFlag.Whitelist;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OthersCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist);
            othersMenu.AddToggle("To Friends", "Add colliders to friends", toggled =>
            {
                if (toggled)
                {
                    _settings.OthersCollisionFlag |= CollisionFlag.Friends;
                }
                else
                {
                    _settings.OthersCollisionFlag &= ~CollisionFlag.Friends;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OthersCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends);
            othersMenu.AddToggle("To Others", "Add colliders to others", toggled =>
            {
                if (toggled)
                {
                    _settings.OthersCollisionFlag |= CollisionFlag.Others;
                }
                else
                {
                    _settings.OthersCollisionFlag &= ~CollisionFlag.Others;
                }
                if (_settings.AutoReloadAvatars)
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
                }
            }, (_settings.OthersCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others);
        }

        private void PromptMaxRadiusInput()
        {
            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Max Collider Radius",
                _settings.MaxRadius.ToString(CultureInfo.InvariantCulture), InputField.InputType.Standard, false, "Submit",
                (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return;

                    if (!float.TryParse(s, out var maxRadius))
                        return;

                    _settings.MaxRadius = maxRadius;
                    _maxRadiusButton.Text = $"Max Collider Radius: {_settings.MaxRadius}";
                }, null);
        }

        private void WhitelistUser(string userId)
        {
            if (_settings.IsWhitelisted(userId))
            {
                _settings.RemoveWhitelist(userId);
            }
            else
            {
                _settings.WhitelistUser(userId);
            }
        }

        private void CycleColliderOption(ReMenuButton button, ref ColliderOption option)
        {
            if (++option > ColliderOption.All)
            {
                option = ColliderOption.None;
            }

            button.Text = $"Colliders: {option}";
        }

        private void ToggleDynamicBones(bool value)
        {
            _settings.Enabled = value;
            if (_settings.AutoReloadAvatars)
            {
                VRCPlayer.field_Internal_Static_VRCPlayer_0.ReloadAllAvatars();
            }
        }

        private void AddBoneCollider(List<DynamicBoneCollider> list, Animator animator, List<HumanBodyBones> bones)
        {
            if (animator == null)
                return;

            if (!animator.isHuman)
                return;

            foreach (var bone in bones)
            {
                var boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform == null)
                    continue;

                foreach (var collider in boneTransform.GetComponentsInChildren<DynamicBoneCollider>(true))
                {
                    if (collider.m_Bound == DynamicBoneCollider.DynamicBoneColliderBound.Inside)
                        continue;

                    var radius = collider.m_Radius * Math.Abs(collider.transform.lossyScale.y);
                    if (radius > _settings.MaxRadius)
                        continue;

                    list.Add(collider);
                }
            }
        }

        private void HandleColliderOption(List<DynamicBoneCollider> list, Animator animator, GameObject avatarObject, ColliderOption option, bool alwaysIncludeHead)
        {
            switch (option)
            {
                case ColliderOption.None:
                    break;
                case ColliderOption.All:
                    {
                        foreach (var collider in avatarObject.GetComponentsInChildren<DynamicBoneCollider>(true))
                        {
                            if (collider.m_Bound == DynamicBoneCollider.DynamicBoneColliderBound.Inside)
                                continue;

                            var radius = collider.m_Radius * Math.Abs(collider.transform.lossyScale.y);
                            if (radius > _settings.MaxRadius)
                                continue;

                            list.Add(collider);
                        }

                        return;
                    }
                case ColliderOption.Hands:
                    AddBoneCollider(list, animator, new List<HumanBodyBones> {HumanBodyBones.LeftHand, HumanBodyBones.RightHand});
                    break;
                case ColliderOption.HandsAndLowerBody:
                    AddBoneCollider(list, animator, new List<HumanBodyBones>
                    {
                        HumanBodyBones.LeftHand,
                        HumanBodyBones.RightHand,
                        HumanBodyBones.LeftFoot,
                        HumanBodyBones.RightFoot,
                        HumanBodyBones.LeftUpperLeg,
                        HumanBodyBones.RightUpperLeg,
                        HumanBodyBones.LeftLowerLeg,
                        HumanBodyBones.RightLowerLeg
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (alwaysIncludeHead)
            {
                AddBoneCollider(list, animator, new List<HumanBodyBones> { HumanBodyBones.Head });
            }
        }

        private void AddCollidersToDynamicBones(List<DynamicBoneCollider> colliders, List<DynamicBone> bones)
        {
            foreach (var bone in bones.ToList())
            {
                if (bone == null)
                {
                    bones.Remove(bone);
                    continue;
                }

                foreach (var collider in colliders.ToList())
                {
                    if (collider == null)
                    {
                        colliders.Remove(collider);
                        continue;
                    }

                    if (bone.m_Colliders.IndexOf(collider) == -1)
                    {
                        bone.m_Colliders.Add(collider);
                    }
                }
            }
        }

        public override void OnAvatarIsReady(VRCPlayer vrcPlayer)
        {
            if (!_settings.Enabled)
                return;

            var apiUser = vrcPlayer.GetPlayer()?.GetAPIUser();
            if (apiUser == null)
                return;
            var avatarObject = vrcPlayer.GetAvatarObject();
            if (avatarObject == null)
                return;

            var animator = avatarObject.GetComponentInChildren<Animator>();
            if (animator == null)
                return;

            var isSelf = vrcPlayer.gameObject == VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject;
            var isWhitelisted = !isSelf && _settings.IsWhitelisted(apiUser.id);
            var isFriend = !isSelf && APIUser.IsFriendsWith(apiUser.id);
            var isOther = !isSelf && !isFriend && !isWhitelisted;

            ColliderOption colliderOption;
            CollisionFlag collisionFlag;
            List<DynamicBone> bonesList;
            List<DynamicBoneCollider> collidersList;
            bool alwaysIncludeHead;
            if (isSelf)
            {
                colliderOption = _settings.OwnColliderOption;
                collisionFlag = _settings.OwnCollisionFlag;
                bonesList = _ownDynamicBones;
                collidersList = _ownDynamicBoneColliders;
                alwaysIncludeHead = _settings.AlwaysIncludeOwnHead;
            }
            else if (isWhitelisted)
            {
                colliderOption = _settings.WhitelistColliderOption;
                collisionFlag = _settings.WhitelistCollisionFlag;
                bonesList = _whitelistedDynamicBones;
                collidersList = _whitelistedBoneColliders;
                alwaysIncludeHead = _settings.AlwaysIncludeWhitelistedHead;
            }
            else if (isFriend)
            {
                colliderOption = _settings.FriendsColliderOption;
                collisionFlag = _settings.FriendsCollisionFlag;
                bonesList = _friendsDynamicBones;
                collidersList = _friendsDynamicBoneColliders;
                alwaysIncludeHead = _settings.AlwaysIncludeFriendsHead;
            }
            else
            {
                colliderOption = _settings.OthersColliderOption;
                collisionFlag = _settings.OthersCollisionFlag;
                bonesList = _othersDynamicBones;
                collidersList = _othersDynamicBoneColliders;
                alwaysIncludeHead = _settings.AlwaysIncludeOthersHead;
            }

            HandleColliderOption(collidersList, animator, avatarObject, colliderOption, alwaysIncludeHead);

            bonesList.AddRange(avatarObject.GetComponentsInChildren<DynamicBone>(true));

            if ((collisionFlag & CollisionFlag.Self) == CollisionFlag.Self)
            {
                AddCollidersToDynamicBones(collidersList, _ownDynamicBones);
            }
            if ((collisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist)
            {
                AddCollidersToDynamicBones(collidersList, _whitelistedDynamicBones);
            }
            if ((collisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends)
            {
                AddCollidersToDynamicBones(collidersList, _friendsDynamicBones);
            }
            if ((collisionFlag & CollisionFlag.Others) == CollisionFlag.Others)
            {
                AddCollidersToDynamicBones(collidersList, _othersDynamicBones);
            }

            if (isSelf)
            {
                if ((_settings.WhitelistCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self)
                {
                    AddCollidersToDynamicBones(_whitelistedBoneColliders, bonesList);
                }
                if ((_settings.FriendsCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self)
                {
                    AddCollidersToDynamicBones(_friendsDynamicBoneColliders, bonesList);
                }
                if ((_settings.OthersCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self)
                {
                    AddCollidersToDynamicBones(_othersDynamicBoneColliders, bonesList);
                }
            }
            else if (isWhitelisted)
            {
                if ((_settings.OwnCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist)
                {
                    AddCollidersToDynamicBones(_ownDynamicBoneColliders, bonesList);
                }
                if ((_settings.FriendsCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist)
                {
                    AddCollidersToDynamicBones(_friendsDynamicBoneColliders, bonesList);
                }
                if ((_settings.OthersCollisionFlag & CollisionFlag.Whitelist) == CollisionFlag.Whitelist)
                {
                    AddCollidersToDynamicBones(_othersDynamicBoneColliders, bonesList);
                }
            }
            else if (isFriend)
            {
                if ((_settings.OwnCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends)
                {
                    AddCollidersToDynamicBones(_ownDynamicBoneColliders, bonesList);
                }
                if ((_settings.WhitelistCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends)
                {
                    AddCollidersToDynamicBones(_whitelistedBoneColliders, bonesList);
                }
                if ((_settings.OthersCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends)
                {
                    AddCollidersToDynamicBones(_othersDynamicBoneColliders, bonesList);
                }
            }
            else
            {
                if ((_settings.OwnCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others)
                {
                    AddCollidersToDynamicBones(_ownDynamicBoneColliders, bonesList);
                }
                if ((_settings.WhitelistCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others)
                {
                    AddCollidersToDynamicBones(_whitelistedBoneColliders, bonesList);
                }
                if ((_settings.FriendsCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others)
                {
                    AddCollidersToDynamicBones(_friendsDynamicBoneColliders, bonesList);
                }
            }
        }
    }
}
