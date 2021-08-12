using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }
}
