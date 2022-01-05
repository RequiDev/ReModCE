using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class VRCNewsAdjustment : ModComponent
    {
        private ConfigValue<bool> EnableNews;
        private ReMenuToggle _enableToggle;

        private Transform _carousel;
        private ReMenuHeaderCollapsible _newsHeader;

        public VRCNewsAdjustment()
        {
            EnableNews = new ConfigValue<bool>(nameof(EnableNews), false);
            EnableNews.OnValueChanged += () =>
            {
                if (_newsHeader != null)
                {
                    _newsHeader.Active = EnableNews;
                }
                _carousel?.gameObject.SetActive(EnableNews);
                _enableToggle?.Toggle(EnableNews, false, true);
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var dashboard = QuickMenuEx.Instance.field_Public_Transform_0.Find("Window/QMParent/Menu_Dashboard").GetComponentInChildren<ScrollRect>().content;
            
            _carousel = dashboard.Find("Carousel_Banners");
            if (_carousel==null) return; // some mod removed the carousel.
            
            _newsHeader = new ReMenuHeaderCollapsible("VRChat News", dashboard);
            _newsHeader.OnToggle += b => _carousel.gameObject.SetActive(b);
            
            _newsHeader.RectTransform.SetSiblingIndex(_carousel.GetSiblingIndex());
            
            _newsHeader.Active = EnableNews;
            _carousel.gameObject.SetActive(EnableNews);

            var menu = uiManager.MainMenu.GetCategoryPage("Utility").GetCategory("VRChat News");
            _enableToggle = menu.AddToggle("Enable", "Enable/Disable VRChat News on the dashboard/launchpad", EnableNews);
        }
    }
}
