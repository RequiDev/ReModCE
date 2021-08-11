using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;

namespace ReModCE.Components
{
    internal class AvatarFavoritesComponent : ModComponent
    {
        private ReAvatarList _avatarList;
        public override void OnUiManagerInit(UiManager uiManager)
        {
            _avatarList = new ReAvatarList("ReModCE Favorites");
        }
    }
}
