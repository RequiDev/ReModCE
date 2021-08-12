using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;

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

            public bool AlwaysIncludeHead
            {
                get => _alwaysIncludeHead;
                set
                {
                    _alwaysIncludeHead = value;
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
            private bool _alwaysIncludeHead = false;

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
        }

        private readonly Settings _settings;

        private ReQuickToggle _whitelistToggle;
        private ReQuickButton _ownColliderOptionButton;


        private Dictionary<int, List<DynamicBone>> _ownDynamicBones = new Dictionary<int, List<DynamicBone>>();
        private Dictionary<int, List<DynamicBoneCollider>> _ownDynamicBoneColliders = new Dictionary<int, List<DynamicBoneCollider>>();

        private Dictionary<int, List<DynamicBone>> _whitelistedDynamicBones = new Dictionary<int, List<DynamicBone>>();
        private Dictionary<int, List<DynamicBoneCollider>> _whitelistedBoneColliders = new Dictionary<int, List<DynamicBoneCollider>>();

        private Dictionary<int, List<DynamicBone>> _friendsDynamicBones = new Dictionary<int, List<DynamicBone>>();
        private Dictionary<int, List<DynamicBoneCollider>> _friendsDynamicBoneColliders = new Dictionary<int, List<DynamicBoneCollider>>();

        private Dictionary<int, List<DynamicBone>> _othersDynamicBones = new Dictionary<int, List<DynamicBone>>();
        private Dictionary<int, List<DynamicBoneCollider>> _othersDynamicBoneColliders = new Dictionary<int, List<DynamicBoneCollider>>();

        public DynamicBonesComponent()
        {
            _settings = Settings.FromFile();
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _whitelistToggle = uiManager.TargetMenu.AddToggle("Whitelist Global Dynamic Bones",
                "Enables dynamic bones for this person.",
                _ =>
                {
                    var interactMenu = ExtendedQuickMenu.UserInteractMenu;
                    var activeUser = interactMenu.field_Public_MenuController_0.activeUser;
                    if (activeUser == null)
                        return;

                    WhitelistUser(activeUser.id);
                });

            uiManager.TargetMenu.OnOpen += () =>
            {
                var interactMenu = ExtendedQuickMenu.UserInteractMenu;
                var activeUser = interactMenu.field_Public_MenuController_0.activeUser;
                if (activeUser == null)
                    return;

                _whitelistToggle.Toggle(_settings.IsWhitelisted(activeUser.id));
            };

            var menu = uiManager.MainMenu.AddSubMenu("Dynamic Bones", "Access your global dynamic bone settings");
            menu.AddToggle("Enabled", "Enable/Disable global dynamic bones", ToggleDynamicBones,
                _settings.Enabled);

            var ownMenu = menu.AddSubMenu("Self Options", "Adjust how your colliders affect others");
            _ownColliderOptionButton = ownMenu.AddButton($"Colliders: {_settings.OwnColliderOption}", "Choose which of your own colliders to apply to others", () =>
            {
                var o = _settings.OwnColliderOption;
                CycleColliderOption(_ownColliderOptionButton, ref o);
                _settings.OwnColliderOption = o;
            });
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
            }, (_settings.OwnCollisionFlag & CollisionFlag.Self) == CollisionFlag.Self);
            ownMenu.AddToggle("To Whitelist", "Add colliders to whitelisted users", toggled =>
            {
                if (toggled)
                {
                    _settings.OwnCollisionFlag |= CollisionFlag.Whitelist;
                }
                else
                {
                    _settings.OwnCollisionFlag &= ~CollisionFlag.Whitelist;
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
            }, (_settings.OwnCollisionFlag & CollisionFlag.Friends) == CollisionFlag.Friends);
            ownMenu.AddToggle("To Friends", "Add colliders to others", toggled =>
            {
                if (toggled)
                {
                    _settings.OwnCollisionFlag |= CollisionFlag.Others;
                }
                else
                {
                    _settings.OwnCollisionFlag &= ~CollisionFlag.Others;
                }
            }, (_settings.OwnCollisionFlag & CollisionFlag.Others) == CollisionFlag.Others);
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

        private void CycleColliderOption(ReQuickButton button, ref ColliderOption option)
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

        }
    }
}
