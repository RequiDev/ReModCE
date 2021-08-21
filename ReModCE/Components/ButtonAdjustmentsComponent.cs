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

        private ReQuickMenu _disablerMenu;
        private ReQuickMenu _moverMenu;
        private ReQuickMenu _sizerMenu;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.AddSubMenu("Button Adjustments",
                "Disable or move buttons around as you please");

            _disablerMenu = menu.AddSubMenu("Disabler", "Disable VRChat buttons in your Quick Menu");
            _moverMenu = menu.AddSubMenu("Mover", "Move buttons around in your Qick Menu");
            _sizerMenu = menu.AddSubMenu("Sizer", "Make any button half size if needed");
            var shortcutMenu = ExtendedQuickMenu.ShortcutMenu;
            var childrenButtons = shortcutMenu.gameObject.GetComponentsInDirectChildren<Button>();
            foreach (var button in childrenButtons)
            {
                var name = button.name;
                if (name == "DevToolsButton") continue;

                var texts = button.gameObject.GetComponentsInDirectChildren<Text>();
                if (texts == null || texts.Length == 0)
                {
                    continue;
                }

                var text = texts[0];
                if (text.text.Length == 0)
                    return;
                CreateUiForButton(button.gameObject, text.text);
            }

            CreateUiForButton(ExtendedQuickMenu.UserIconCameraButton.gameObject, "Camera Icon Button", disable:false, size: false); // 
            CreateUiForButton(ExtendedQuickMenu.VRCPlusPet.gameObject, "VRC+ Pet", false, disable: false, size: false);
        }

        private void CreateUiForButton(GameObject gameObject, string name, bool hasButton = true, bool disable = true, bool move = true, bool size = true)
        {
            ReQuickToggle buttonToggle = null;
            var buttonHalfSize = new ConfigValue<bool>($"{gameObject.name}HalfSize", false, isHidden: true);

            if (disable)
            {
                var buttonEnabled = new ConfigValue<bool>($"{gameObject.name}Enabled", gameObject.gameObject.activeSelf);
                buttonEnabled.OnValueChanged += () =>
                {
                    buttonToggle.Toggle(buttonEnabled);
                    gameObject.gameObject.SetActive(buttonEnabled);
                };

                buttonToggle = _disablerMenu.AddToggle($"{name}", $"Enable/Disable \"{name}\" button.",
                    buttonEnabled.SetValue, buttonEnabled);

                if (buttonEnabled != gameObject.gameObject.activeSelf)
                {
                    gameObject.gameObject.SetActive(buttonEnabled);
                }
            }

            if (move)
            {
                var buttonPosX = new ConfigValue<float>($"{gameObject.name}PosX", gameObject.transform.localPosition.x,
                    isHidden: true);
                var buttonPosY = new ConfigValue<float>($"{gameObject.name}PosY", gameObject.transform.localPosition.y,
                    isHidden: true);

                _moverMenu.AddButton($"{name}", $"Move \"{name}\" button", () =>
                {
                    if (!_canMoveButtons) return;
                    if (_movingButton) return;
                    if (hasButton)
                    {
                        _originalButtonClickedEvent = gameObject.GetComponent<Button>().onClick;
                        gameObject.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    }

                    _movingButton = true;
                    _canMoveButtons = false;

                    ExtendedQuickMenu.Instance.SetCurrentPage(ExtendedQuickMenu.ShortcutMenu.name);
                    MelonCoroutines.Start(MoveButtonCoroutine(gameObject.gameObject, (openPrevMenu) =>
                    {
                        if (hasButton)
                        {
                            gameObject.GetComponent<Button>().onClick = _originalButtonClickedEvent;
                        }

                        buttonPosX.SetValue(gameObject.transform.localPosition.x);
                        buttonPosY.SetValue(gameObject.transform.localPosition.y);
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                            gameObject.transform.localPosition.y, 0f);
                        MelonCoroutines.Start(EnableCanMoveButtonsDelayed());
                        if (openPrevMenu)
                        {
                            _moverMenu.Open();
                        }
                    }));
                });
                
                gameObject.transform.localPosition = new Vector3(buttonPosX, buttonPosY);
            }

            if (size)
            {
                _sizerMenu.AddToggle($"{name}", $"Half \"{name}\" button", b =>
                {
                    buttonHalfSize.SetValue(b);
                    if (b)
                    {
                        gameObject.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 0.5f);
                    }
                    else
                    {
                        gameObject.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 2f);
                    }
                }, buttonHalfSize);

                if (buttonHalfSize)
                {
                    gameObject.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 0.5f);
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
