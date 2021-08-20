using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class ButtonAdjustmentsComponent : ModComponent
    {
        private Button.ButtonClickedEvent _originalButtonClickedEvent;
        private bool _movingButton;

        private bool _canMoveButtons = true;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.AddSubMenu("Button Adjustments",
                "Disable or move buttons around as you please");

            var disablerMenu = menu.AddSubMenu("Disabler", "Disable VRChat buttons in your Quick Menu");
            var moverMenu = menu.AddSubMenu("Mover", "Move buttons around in your Qick Menu");
            var sizeMenu = menu.AddSubMenu("Sizer", "Make any button half size if needed");
            var shortcutMenu = ExtendedQuickMenu.ShortcutMenu;
            var childrenButtons = shortcutMenu.gameObject.GetComponentsInDirectChildren<Button>();
            foreach (var button in childrenButtons)
            {
                var name = button.name;
                if (name == "DevToolsButton") continue;

                ReQuickToggle buttonToggle = null;
                var buttonEnabled = new ConfigValue<bool>($"{name}Enabled", button.gameObject.activeSelf);
                var buttonPosX = new ConfigValue<float>($"{name}PosX", button.transform.localPosition.x, isHidden:true);
                var buttonPosY = new ConfigValue<float>($"{name}PosY", button.transform.localPosition.y, isHidden: true);
                var buttonHalfSize = new ConfigValue<bool>($"{name}HalfSize", false, isHidden: true);

                buttonEnabled.OnValueChanged += () =>
                {
                    buttonToggle.Toggle(buttonEnabled);
                    button.gameObject.SetActive(buttonEnabled);
                };

                var text = button.gameObject.GetComponentsInDirectChildren<Text>();
                if (text == null || text.Length == 0)
                {
                    continue;
                }
                var buttonName = text[0].text;

                buttonToggle = disablerMenu.AddToggle($"{buttonName}", $"Enable/Disable \"{buttonName}\" button.",
                    buttonEnabled.SetValue, buttonEnabled);

                moverMenu.AddButton($"{buttonName}", $"Move \"{buttonName}\" button", () =>
                {
                    if (!_canMoveButtons) return;
                    if (_movingButton) return;
                    _originalButtonClickedEvent = button.onClick;
                    button.onClick = new Button.ButtonClickedEvent();
                    
                    _movingButton = true;
                    _canMoveButtons = false;

                    ExtendedQuickMenu.Instance.SetCurrentPage(shortcutMenu.name);
                    MelonCoroutines.Start(MoveButtonCoroutine(button.gameObject, (openPrevMenu) =>
                    {
                        button.onClick = _originalButtonClickedEvent;
                        buttonPosX.SetValue(button.transform.localPosition.x);
                        buttonPosY.SetValue(button.transform.localPosition.y);
                        button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y, 0f);
                        MelonCoroutines.Start(EnableCanMoveButtonsDelayed());
                        if (openPrevMenu)
                        {
                            moverMenu.Open();
                        }
                    }));
                });

                sizeMenu.AddToggle($"{buttonName}", $"Half \"{buttonName}\" button", b =>
                {
                    buttonHalfSize.SetValue(b);
                    if (b)
                    {
                        button.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 0.5f);
                    }
                    else
                    {
                        button.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 2f);
                    }
                }, buttonHalfSize);

                var buttonPos = new Vector3(buttonPosX, buttonPosY);
                if (buttonPos != Vector3.zero)
                {
                    button.transform.localPosition = buttonPos;
                }
                if (buttonEnabled != button.gameObject.activeSelf)
                {
                    button.gameObject.SetActive(buttonEnabled);
                }
                if (buttonHalfSize)
                {
                    button.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 0.5f);
                }
            }
        }

        private IEnumerator EnableCanMoveButtonsDelayed()
        {
            yield return new WaitForSeconds(1);
            _canMoveButtons = true;
        }

        private IEnumerator MoveButtonCoroutine(GameObject gameObjectToMove, Action<bool> onComplete = null)
        {
            var movingGameObjectRect = gameObjectToMove.GetComponent<RectTransform>();
            var oldPosition = movingGameObjectRect.anchoredPosition3D;
            
            while (ExtendedCursor.IsUseInputPressed && _movingButton)
            {
                movingGameObjectRect.transform.position = ExtendedCursor.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, 25f);
                movingGameObjectRect.anchoredPosition = movingGameObjectRect.anchoredPosition.RoundAmount(UiManager.ButtonSize / 4f);
                
                yield return null;
            }
            
            while (!ExtendedCursor.IsUseInputPressed && _movingButton)
            {
                if (!ExtendedQuickMenu.ShortcutMenu.gameObject.activeSelf)
                {
                    movingGameObjectRect.anchoredPosition3D = oldPosition;
                    _movingButton = false;
                    onComplete?.Invoke(false);
                    yield break;
                }

                movingGameObjectRect.transform.position = ExtendedCursor.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, 25f);
                movingGameObjectRect.anchoredPosition = movingGameObjectRect.anchoredPosition.RoundAmount(UiManager.ButtonSize / 4f);
                
                yield return null;
            }
            
            if (_movingButton)
                onComplete?.Invoke(true);
            else
                movingGameObjectRect.anchoredPosition3D = oldPosition;
            _movingButton = false;
        }
    }
}
