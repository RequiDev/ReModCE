using System;
using MelonLoader;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;

namespace ReModCE.Components
{
    internal class IngameLog : ModComponent
    {
        private ReScrollView _log;
        private string _preUiLog;

        private ConfigValue<bool> IngameLogEnabled;
        private ReQuickToggle _logToggle;
        public IngameLog()
        {
            IngameLogEnabled = new ConfigValue<bool>(nameof(IngameLogEnabled), true);
            IngameLogEnabled.OnValueChanged += () =>
            {
                _logToggle.Toggle(IngameLogEnabled);
                ToggleIngameLog(IngameLogEnabled);
            };

            MelonLogger.MsgCallbackHandler += (color, consoleColor, nameSection, msg) =>
            {
                if (nameSection != nameof(ReModCE))
                    return;

                var nameSectionPretty = string.IsNullOrEmpty(nameSection)
                    ? string.Empty
                    : $"<color={ConsoleColorToHexCode(color)}>[{nameSection}]</color> ";
                if (_log == null)
                {
                    _preUiLog += $"{nameSectionPretty}<color={ConsoleColorToHexCode(consoleColor)}>{msg}</color>\n";
                    return;
                }

                _log.AddText($"{nameSectionPretty}<color={ConsoleColorToHexCode(consoleColor)}>{msg}</color>\n");
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var logMenu = uiManager.MainMenu.AddSubMenu("Ingame Log", "Access ingame log related settings.");
            _logToggle = logMenu.AddToggle("Enabled", "Enable/Disable the ingame log UI", IngameLogEnabled.SetValue, IngameLogEnabled);

            var logPos = new Vector2(-1695, 1470f);
            if (uiManager.IsRubyLoaded)
            {
                logPos.x -= 245f;
            }

            _log = new ReScrollView("ReModCELog", logPos, ExtendedQuickMenu.ShortcutMenu);
            _log.AddText(_preUiLog);
        }

        private void ToggleIngameLog(bool toggled)
        {
            _log.Active = toggled;
        }

        private static string ConsoleColorToHexCode(ConsoleColor c)
        {
            string[] cColors = {
                "#000000", //Black = 0
                "#000080", //DarkBlue = 1
                "#008000", //DarkGreen = 2
                "#008080", //DarkCyan = 3
                "#800000", //DarkRed = 4
                "#800080", //DarkMagenta = 5
                "#808000", //DarkYellow = 6
                "#C0C0C0", //Gray = 7
                "#808080", //DarkGray = 8
                "#0000FF", //Blue = 9
                "#00FF00", //Green = 10
                "#00FFFF", //Cyan = 11
                "#FF0000", //Red = 12
                "#FF00FF", //Magenta = 13
                "#FFFF00", //Yellow = 14
                "#FFFFFF"  //White = 15
            };
            return cColors[(int)c];
        }

    }
}
