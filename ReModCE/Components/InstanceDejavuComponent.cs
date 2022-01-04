using ReMod.Core;
using ReMod.Core.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.UI;
using Object = UnityEngine.Object;

namespace ReModCE.Components
{
    internal sealed class InstanceDejavuComponent : ModComponent
    {
        private static HashSet<string> _visitedWorldHashSet;
        private static GameObject _worldVisitedIcon;
        private static UiWorldInstanceList _uiWorldInstanceListInstance;
        private static GameObject _visitedButton;

        public InstanceDejavuComponent()
        {
            _visitedWorldHashSet = new HashSet<string>();

            foreach (var m in typeof(UiWorldInstanceList).GetMethods().Where(m =>
                m.Name.StartsWith("Method_Protected_Virtual_Void_VRCUiContentButton_Object_")))
            {
                ReModCE.Harmony.Patch(m, postfix: GetLocalPatch(nameof(OnInstanceContentButtonGenerationPostfix)));
            }

            var selectWorldInstance = typeof(PageWorldInfo).GetMethods().Single(m => XrefUtils.CheckMethod(m, "Make Home"));
            ReModCE.Harmony.Patch(selectWorldInstance, postfix: GetLocalPatch(nameof(UpdateWorldMainPicker)));
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            InitializeVisitedIcon();
        }

        public override void OnEnterWorld(ApiWorld world, ApiWorldInstance instance)
        {
            _visitedWorldHashSet.Add($"{world.name}-{instance.instanceId}");
        }

        private static void UpdateWorldMainPicker()
        {
            var pageWorldInfo = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo")
                .GetComponent<PageWorldInfo>();

            if (pageWorldInfo.field_Private_ApiWorld_0 == null ||
                pageWorldInfo.field_Public_ApiWorldInstance_0 == null) return;

            UpdateSelectedWorldVisited(pageWorldInfo.field_Private_ApiWorld_0,
                pageWorldInfo.field_Public_ApiWorldInstance_0);
        }
        
        private void InitializeVisitedIcon()
        {
            var worldInfoScreen = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo/").transform;
            var worldIconBase = worldInfoScreen.FindChild("WorldImage/OverlayIcons/FavoriteIcon/").gameObject;

            _uiWorldInstanceListInstance = worldInfoScreen.FindChild("OtherInstances").GetComponent<UiWorldInstanceList>();

            _worldVisitedIcon = Object.Instantiate(worldIconBase, worldIconBase.transform.parent.transform);
            _worldVisitedIcon.name = "iconVisitedBefore";
            _worldVisitedIcon.transform.FindChild("IconImage").GetComponent<Image>().sprite =
                ResourceManager.GetSprite("remodce.history");
            _worldVisitedIcon.transform.localPosition = new Vector3(-110, 188, 0);
            _worldVisitedIcon.SetActive(false);
        }

        private static Transform GenerateIcon(VRCUiContentButton button)
        {
            if (_visitedButton == null)
            {
                var original = button.transform.FindChild("RoomImageShape/RoomImage/OverlayIcons/iconFavoriteStar").gameObject;
                var newIcon = Object.Instantiate(original, original.transform.parent.transform);
                newIcon.name = "iconVisitedBefore";
                var img = newIcon.GetComponent<Image>();
                img.sprite = ResourceManager.GetSprite("remodce.history");
                img.color = new Color(0.4157f,0.8902f,0.9765f,1f);
                newIcon.SetActive(false);
                _visitedButton = newIcon;
                return newIcon.transform;
            }

            var visitedIcon = Object.Instantiate(_visitedButton,
                button.transform.FindChild("RoomImageShape/RoomImage/OverlayIcons/"));

            visitedIcon.name = "iconVisitedBefore";
            return visitedIcon.transform;
        }

        private static void UpdateSelectedWorldVisited(ApiWorld world, ApiWorldInstance instance)
        {
            var a = _visitedWorldHashSet.Contains($"{world.name}-{instance.instanceId}");
            _worldVisitedIcon.SetActive(a);
        }

        private static void OnInstanceContentButtonGenerationPostfix(VRCUiContentButton __0, ApiWorldInstance __1)
        {
            var icon = __0.transform.FindChild("RoomImageShape/RoomImage/OverlayIcons/iconVisitedBefore")?.gameObject;
            if (icon == null)
            {
                icon = GenerateIcon(__0).gameObject;
            }
            
            var instanceId = __1.instanceId;
            
            if (_visitedWorldHashSet.Contains(
                $"{_uiWorldInstanceListInstance.field_Public_ApiWorld_0.name}-{instanceId}"))
            {
                icon.transform.parent.gameObject.SetActive(true);
                icon.SetActive(true);
            }
            else
            {
                icon.SetActive(false);
            }
        }
    }
}
