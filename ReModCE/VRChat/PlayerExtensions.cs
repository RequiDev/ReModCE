using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE.VRChat
{
    internal static class PlayerExtensions
    {
        public static Player[] GetPlayers(this PlayerManager playerManager)
        {
            return playerManager.prop_ArrayOf_Player_0;
        }

        public static Player GetPlayer(this PlayerManager playerManager, string userId)
        {
            foreach (var player in playerManager.GetPlayers())
            {
                if (player == null) continue;
                
                var apiUser = player.GetAPIUser();
                if (apiUser == null) continue;
                
                if (apiUser.id == userId) return player;
            }

            return null;
        }

        public static VRCPlayer GetVRCPlayer(this Player player)
        {
            return player._vrcplayer;
        }

        public static APIUser GetAPIUser(this Player player)
        {
            return player.field_Private_APIUser_0;
        }

        public static ApiAvatar GetApiAvatar(this Player player)
        {
            return player.prop_ApiAvatar_0;
        }

        public static Player GetPlayer(this VRCPlayer vrcPlayer)
        {
            return vrcPlayer._player;
        }

        public static PlayerNet GetPlayerNet(this VRCPlayer vrcPlayer)
        {
            return vrcPlayer._playerNet;
        }

        public static GameObject GetAvatarObject(this VRCPlayer vrcPlayer)
        {
            return vrcPlayer.field_Internal_GameObject_0;
        }

        public static VRCPlayerApi GetPlayerApi(this VRCPlayer vrcPlayer)
        {
            return vrcPlayer.field_Private_VRCPlayerApi_0;
        }

        private static MethodInfo _reloadAvatarMethod;
        private static MethodInfo LoadAvatarMethod
        {
            get
            {
                if (_reloadAvatarMethod == null)
                {
                    _reloadAvatarMethod = typeof(VRCPlayer).GetMethods().First(mi =>
                        mi.Name.StartsWith("Method_Private_Void_Boolean_") && mi.Name.Length < 31 &&
                        mi.GetParameters().Any(pi => pi.IsOptional));
                }

                return _reloadAvatarMethod;
            }
        }
        
        private static MethodInfo _reloadAllAvatarsMethod;
        private static MethodInfo ReloadAllAvatarsMethod
        {
            get
            {
                if (_reloadAllAvatarsMethod == null)
                {
                    _reloadAllAvatarsMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Void_Boolean_") && mi.Name.Length < 30 && mi.GetParameters().Any(pi => pi.IsOptional));
                }

                return _reloadAllAvatarsMethod;
            }
        }
        public static void ReloadAvatar(this VRCPlayer instance)
        {
            LoadAvatarMethod.Invoke(instance, new object[] { true }); // parameter is forceLoad and has to be true
        }
        public static void ReloadAllAvatars(this VRCPlayer instance, bool ignoreSelf = false)
        {
            ReloadAllAvatarsMethod.Invoke(instance, new object[] { ignoreSelf });
        }
    }
}
