using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using Tomlet;
using Tomlet.Exceptions;
using Tomlet.Models;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class ButtonAdjustmentsComponent : ModComponent
    {
        internal class AdjustedButton
        {
            public bool Active { get; set; }
            public Vector2 Position { get; set; }
            public bool HalfSize { get; set; }
        }

        private Button.ButtonClickedEvent _originalButtonClickedEvent;
        private bool _movingButton;

        private bool _canMoveButtons = true;

        private ReQuickMenu _disablerMenu;
        private ReQuickMenu _moverMenu;
        private ReQuickMenu _sizerMenu;

        private readonly Dictionary<string, AdjustedButton> _adjustButtonConfig;

        private readonly TomlTable _remodTomlTable;

        public ButtonAdjustmentsComponent()
        {
            if (File.Exists("UserData/ReModCE/adjusted_buttons.json"))
            {
                _adjustButtonConfig =
                    JsonConvert.DeserializeObject<Dictionary<string, AdjustedButton>>(File.ReadAllText("UserData/ReModCE/adjusted_buttons.json"));
            }
            else
            {
                _adjustButtonConfig = new Dictionary<string, AdjustedButton>();
            }

            var melonPrefs = TomlParser.ParseFile(Path.Combine(MelonUtils.UserDataDirectory, "MelonPreferences.cfg"));
            try
            {
                _remodTomlTable = melonPrefs.GetSubTable("ReModCE");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.AddSubMenu("Button Adjustments",
                "Disable or move buttons around as you please");

            _disablerMenu = menu.AddSubMenu("Disabler", "Disable VRChat buttons in your Quick Menu");
            _moverMenu = menu.AddSubMenu("Mover", "Move buttons around in your Qick Menu");
            _sizerMenu = menu.AddSubMenu("Sizer", "Make any button half size if needed");

            MelonCoroutines.Start(RunDelayed());
        }

        private IEnumerator RunDelayed()
        {
            // wait 1 frame so other mods are initialized and won't copy a resized/moved button
            yield return new WaitForEndOfFrame();

            var shortcutMenu = ExtendedQuickMenu.ShortcutMenu;
            var childrenButtons = shortcutMenu.gameObject.GetComponentsInDirectChildren<Button>();
            foreach (var button in childrenButtons)
            {
                var name = button.name;
                if (name == "DevToolsButton") continue;
                if (name.StartsWith("SingleButton")) continue;

                var texts = button.gameObject.GetComponentsInDirectChildren<Text>();
                if (texts == null || texts.Length == 0)
                {
                    continue;
                }

                var text = texts[0];
                if (text.text.Length == 0)
                    continue;

                CreateUiForButton(button.gameObject, text.text);
            }

            CreateUiForButton(ExtendedQuickMenu.UserIconCameraButton.gameObject, "Camera Icon Button", allowDisable: false, allowSize: false); // 
            CreateUiForButton(ExtendedQuickMenu.VRCPlusPet.gameObject, "VRC+ Pet", false, allowDisable: false, allowSize: false);

            if (!File.Exists("UserData/ReModCE/adjusted_buttons.json"))
            {
                SaveButtonAdjustments();
            }
        }

        private void SaveButtonAdjustments()
        {
            File.WriteAllText("UserData/ReModCE/adjusted_buttons.json", JsonConvert.SerializeObject(_adjustButtonConfig, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DynamicContractResolver(new List<string>
                {
                    "normalized",
                    "magnitude",
                    "sqrMagnitude"
                })
            }));
        }

        private T GetValueFromPrefs<T>(string prefName, T dflt)
        {
            try
            {
                return TomletMain.To<T>(_remodTomlTable.Entries[prefName]);
            }
            catch (Exception ex)
            {
                return dflt;
            }
        }

        private void CreateUiForButton(GameObject gameObject, string name, bool hasButton = true, bool allowDisable = true, bool allowMove = true, bool allowSize = true)
        {
            AdjustedButton adjustedButton;
            if (_adjustButtonConfig.ContainsKey(gameObject.name))
            {
                adjustedButton = _adjustButtonConfig[gameObject.name];
            }
            else
            {
                var baseEntryName = string.Concat(gameObject.name.Where(c => char.IsLetter(c) || char.IsNumber(c)));
                
                adjustedButton = new AdjustedButton
                {
                    Active = GetValueFromPrefs($"{baseEntryName}Enabled", gameObject.activeSelf),
                    HalfSize = GetValueFromPrefs($"{baseEntryName}HalfSize", false),
                    Position = new Vector2(
                        GetValueFromPrefs($"{baseEntryName}PosX", gameObject.transform.localPosition.x),
                        GetValueFromPrefs($"{baseEntryName}PosY", gameObject.transform.localPosition.y))
                };

                _adjustButtonConfig.Add(gameObject.name, adjustedButton);
            }

            if (allowDisable)
            {
                _disablerMenu.AddToggle($"{name}", $"Enable/Disable \"{name}\" button.",
                    b =>
                    {
                        adjustedButton.Active = b;
                        gameObject.SetActive(b);
                        SaveButtonAdjustments();
                    }, adjustedButton.Active);

                if (adjustedButton.Active != gameObject.gameObject.activeSelf)
                {
                    gameObject.SetActive(adjustedButton.Active);
                }
            }

            if (allowMove)
            {
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

                        adjustedButton.Position = gameObject.transform.localPosition;
                        SaveButtonAdjustments();
                        gameObject.transform.localPosition = adjustedButton.Position;
                        MelonCoroutines.Start(EnableCanMoveButtonsDelayed());
                        if (openPrevMenu)
                        {
                            _moverMenu.Open();
                        }
                    }));
                });

                gameObject.transform.localPosition = adjustedButton.Position;
            }

            if (allowSize)
            {
                _sizerMenu.AddToggle($"{name}", $"Half \"{name}\" button", b =>
                {
                    adjustedButton.HalfSize = b;
                    SaveButtonAdjustments();
                    if (b)
                    {
                        gameObject.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 0.5f);
                    }
                    else
                    {
                        gameObject.GetComponent<RectTransform>().sizeDelta *= new Vector2(1f, 2f);
                    }
                }, adjustedButton.HalfSize);

                if (adjustedButton.HalfSize)
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
