using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.VRChat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ReMod.Core.Notification;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;

namespace ReModCE.Components
{
    internal sealed class MediaControlComponent : ModComponent
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        private static readonly int KEYEVENTF_KEYUP = 0x0002;

        private const byte MediaPlayPause = (byte)Keys.MediaPlayPause;
        private const byte MediaNextTrack = (byte)Keys.MediaNextTrack;
        private const byte MediaPreviousTrack = (byte)Keys.MediaPreviousTrack;
        private const byte MediaStop = (byte)Keys.MediaStop;
        private const byte VolUp = (byte)Keys.VolumeUp;
        private const byte VolDown = (byte)Keys.VolumeDown;
        private const byte VolMute = (byte)Keys.VolumeMute;

        private static void PlayPause()
        {
            keybd_event(MediaPlayPause, MediaPlayPause, 0, 0);
            keybd_event(MediaPlayPause, MediaPlayPause, KEYEVENTF_KEYUP, 0);
        }
        private static void PrevTrack()
        {
            keybd_event(MediaPreviousTrack, MediaPreviousTrack, 0, 0);
            keybd_event(MediaPreviousTrack, MediaPreviousTrack, KEYEVENTF_KEYUP, 0);
        }
        private static void NextTrack()
        {
            keybd_event(MediaNextTrack, MediaNextTrack, 0, 0);
            keybd_event(MediaNextTrack, MediaNextTrack, KEYEVENTF_KEYUP, 0);
        }
        private static void Stop()
        {
            keybd_event(MediaStop, MediaStop, 0, 0);
            keybd_event(MediaStop, MediaStop, KEYEVENTF_KEYUP, 0);
        }
        private static void VolumeUp()
        {
            keybd_event(VolUp, VolUp, 0, 0);
            keybd_event(VolUp, VolUp, KEYEVENTF_KEYUP, 0);
        }
        private static void VolumeDown()
        {
            keybd_event(VolDown, VolDown, 0, 0);
            keybd_event(VolDown, VolDown, KEYEVENTF_KEYUP, 0);
        }
        private static void VolumeMute()
        {
            keybd_event(VolMute, VolMute, 0, 0);
            keybd_event(VolMute, VolMute, KEYEVENTF_KEYUP, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        private IntPtr _spotifyWindow;
        private string _currentSong;

        private ReMenuCategory _mediaControlCategory;

        private ConfigValue<bool> MediaControlsEnabled;
        private ConfigValue<bool> SongPreviewEnabled;

        private float _timer;

        public MediaControlComponent()
        {
            MediaControlsEnabled = new ConfigValue<bool>(nameof(MediaControlsEnabled), true);
            MediaControlsEnabled.OnValueChanged += () =>
            {
                _mediaControlCategory.Active = MediaControlsEnabled;
            };
            SongPreviewEnabled = new ConfigValue<bool>(nameof(SongPreviewEnabled), true);

            MediaControlsEnabled.OnValueChanged += ToggleMediaControls;
        }

        public override void OnUiManagerInitEarly()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    if (_spotifyWindow != IntPtr.Zero && IsWindow(_spotifyWindow)) continue;

                    var spotifyProcess = Process.GetProcessesByName("Spotify").FirstOrDefault();
                    if (spotifyProcess == null)
                        continue;

                    _spotifyWindow = new MainWindowFinder().FindMainWindow(spotifyProcess.Id);
                    if (_spotifyWindow == IntPtr.Zero || !IsWindow(_spotifyWindow))
                        continue;
                }
            }).Start();
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var dashboard = QuickMenuEx.Instance.field_Public_Transform_0.Find("Window/QMParent/Menu_Dashboard").GetComponent<UIPage>();
            var scrollRect = dashboard.GetComponentInChildren<ScrollRect>();

            _mediaControlCategory = new ReMenuCategory(string.IsNullOrEmpty(_currentSong) ? "Media Controls" : _currentSong, scrollRect.content, false);
            var tmpro = _mediaControlCategory.Header.RectTransform.GetComponentInChildren<TextMeshProUGUI>();

            tmpro.enableAutoSizing = true;
            tmpro.enableWordWrapping = false;

            _mediaControlCategory.RectTransform.SetAsFirstSibling();
            _mediaControlCategory.Header.RectTransform.SetAsFirstSibling();

            _mediaControlCategory.AddButton("Stop", "Stop song.", Stop, ResourceManager.GetSprite("remodce.stop"));
            _mediaControlCategory.AddButton("Previous", "Previous song.", PrevTrack, ResourceManager.GetSprite("remodce.previous"));
            _mediaControlCategory.AddButton("Play", "Play/Pause song.", PlayPause, ResourceManager.GetSprite("remodce.play"));
            _mediaControlCategory.AddButton("Next", "Next song.", NextTrack, ResourceManager.GetSprite("remodce.next"));

            _mediaControlCategory.Active = MediaControlsEnabled;
            var subMenu = uiManager.MainMenu.GetCategoryPage("Utility").AddCategory("Media Controls");

            subMenu.AddToggle("Enable Controls", "Enable/Disable media controls in your quick menu", MediaControlsEnabled);
            subMenu.AddToggle("Enable Song Preview", "Enable/Disable preview of song that's currently playing", SongPreviewEnabled);
        }

        public void ToggleMediaControls()
        {
            _mediaControlCategory.Active = MediaControlsEnabled;
        }

        public override void OnUpdate()
        {
            _timer += Time.deltaTime;
            if (_timer < 1f)
                return;
            _timer = 0f;

            if (_spotifyWindow == IntPtr.Zero || !IsWindow(_spotifyWindow))
                return;

            var len = GetWindowTextLength(_spotifyWindow) * 2;
            var sb = new StringBuilder(len + 1);
            GetWindowText(_spotifyWindow, sb, sb.Capacity);

            var song = sb.ToString();
            var isPlayingSong = !song.StartsWith("Spotify");
            if (isPlayingSong && _currentSong != song)
            {
                if (_mediaControlCategory != null)
                {
                    _mediaControlCategory.Title = song;
                }
                if (SongPreviewEnabled)
                {
                    NotificationSystem.EnqueueNotification("ReModCE", $"<color=#{Color.green.ToHex()}>Now Playing on Spotify:\n{song}</color>", icon: ResourceManager.GetSprite("remodce.remod"));
                    // VRCUiManagerEx.Instance.QueueHudMessage($"Now Playing on Spotify:\n{song}", Color.green);
                }
            }
            _currentSong = isPlayingSong ? song : string.Empty;
        }
    }

    internal class MainWindowFinder
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);
        private delegate bool CallBackPtr(IntPtr hwnd, int lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(CallBackPtr lpEnumFunc, int lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private enum GetWindowType : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        private IntPtr _bestHandle;
        private int _processId;

        public IntPtr FindMainWindow(int processId)
        {
            _bestHandle = IntPtr.Zero;
            _processId = processId;

            EnumWindows(EnumWindowsThunk, _processId);

            return _bestHandle;
        }

        private bool EnumWindowsThunk(IntPtr hWnd, int processId)
        {
            GetWindowThreadProcessId(hWnd, out var pid);
            if (pid != processId || !IsMainWindow(hWnd))
                return true;
            _bestHandle = hWnd;
            return false;
        }

        private static bool IsMainWindow(IntPtr hWnd)
        {
            if (GetWindow(hWnd, GetWindowType.GW_OWNER) == IntPtr.Zero && IsWindowVisible(hWnd))
                return true;
            return false;
        }
    }
}
