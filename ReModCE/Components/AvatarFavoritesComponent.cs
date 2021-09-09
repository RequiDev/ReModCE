using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

namespace ReModCE.Components
{
    internal class AvatarFavoritesComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _avatarList;
        private ReUiButton _favoriteButton;

        private ReAvatarList _searchedAvatarList;

        private Button.ButtonClickedEvent _changeButtonEvent;

        private const string ApiUrl = "https://requi.dev/remod";
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;

        private const string PinPath = "UserData/ReModCE/pin";
        private int _pinCode;
        private ReQuickButton _enterPinButton;

        private ConfigValue<bool> AvatarFavoritesEnabled;
        private ReQuickToggle _enabledToggle;
        private ConfigValue<int> MaxAvatarsPerPage;
        private ReQuickButton _maxAvatarsPerPageButton;

        private List<ReAvatar> _savedAvatars;
        private AvatarList _searchedAvatars;
        private readonly List<ReAvatar> _localAvatars;

        private GameObject _avatarScreen;
        private UiInputField _searchBox;
        private UnityAction<string> _searchAvatarsAction;
        private UnityAction<string> _overrideSearchAvatarsAction;
        private UnityAction<string> _emmVRCsearchAvatarsAction;

        public AvatarFavoritesComponent()
        {
            _httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };
            _httpClient = new HttpClient(_httpClientHandler);

            AvatarFavoritesEnabled = new ConfigValue<bool>(nameof(AvatarFavoritesEnabled), true);
            AvatarFavoritesEnabled.OnValueChanged += () =>
            {
                _enabledToggle.Toggle(AvatarFavoritesEnabled);
                _avatarList.GameObject.SetActive(AvatarFavoritesEnabled);
            };
            MaxAvatarsPerPage = new ConfigValue<int>(nameof(MaxAvatarsPerPage), 100);
            MaxAvatarsPerPage.OnValueChanged += () =>
            {
                _avatarList.SetMaxAvatarsPerPage(MaxAvatarsPerPage);
            };
            
            _savedAvatars = new List<ReAvatar>();
            _searchedAvatars = new AvatarList();

            if (File.Exists(PinPath))
            {
                if (!int.TryParse(File.ReadAllText(PinPath), out _pinCode))
                {
                    ReLogger.Warning($"Couldn't read pin file from \"{PinPath}\". File might be corrupted.");
                }
            }

            if (File.Exists("UserData/ReModCE/avatars.bin"))
            {
                _localAvatars = BinaryGZipSerializer.Deserialize("UserData/ReModCE/avatars.bin") as List<ReAvatar>;
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetSubMenu("Avatars");
            _enabledToggle = menu.AddToggle("Avatar Favorites", "Enable/Disable avatar favorites (requires VRC+)",
                AvatarFavoritesEnabled.SetValue, AvatarFavoritesEnabled);
            _maxAvatarsPerPageButton = menu.AddButton($"Max Avatars Per Page: {MaxAvatarsPerPage}",
                "Set the maximum amount of avatars shown per page",
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Max Avatars Per Page",
                        MaxAvatarsPerPage.ToString(), InputField.InputType.Standard, true, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s))
                                return;

                            if (!int.TryParse(s, out var maxAvatarsPerPage))
                                return;

                            MaxAvatarsPerPage.SetValue(maxAvatarsPerPage);
                            _maxAvatarsPerPageButton.Text = $"Max Avatars Per Page: {MaxAvatarsPerPage}";
                        }, null);
                });

            if (_pinCode == 0)
            {
                _enterPinButton = menu.AddButton("Set/Enter Pin", "Set or enter your pin for the ReMod CE API", () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Enter pin",
                        "", InputField.InputType.Standard, true, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s))
                                return;

                            if (!int.TryParse(s, out var pinCode))
                                return;

                            _pinCode = pinCode;
                            File.WriteAllText(PinPath, _pinCode.ToString());

                            _httpClientHandler = new HttpClientHandler
                            {
                                UseCookies = true,
                                CookieContainer = new CookieContainer()
                            };
                            _httpClient = new HttpClient(_httpClientHandler);

                            LoginToAPI(APIUser.CurrentUser);
                        }, null);
                });
            }
            
            _avatarList = new ReAvatarList("ReModCE Favorites", this);
            _avatarList.AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);
            _avatarList.OnEnable += () =>
            {
                // make sure it stays off if it should be off.
                _avatarList.GameObject.SetActive(AvatarFavoritesEnabled);
            };

            _searchedAvatarList = new ReAvatarList("ReMod CE Search", this);
            _searchedAvatarList.AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);


            var parent = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button").transform.parent;
            _favoriteButton = new ReUiButton("Favorite", new Vector2(-600f, 375f), new Vector2(0.5f, 1f), () => FavoriteAvatar(_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0),
                parent);

            var changeButton = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button");
            if (changeButton != null)
            {
                var button = changeButton.GetComponent<Button>();
                _changeButtonEvent = button.onClick;

                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(new Action(ChangeAvatarChecked));
            }

            if (uiManager.IsRemodLoaded || uiManager.IsRubyLoaded)
            {
                _favoriteButton.Position += new Vector3(UiManager.ButtonSize, 0f);
            }
            
            _searchAvatarsAction = DelegateSupport.ConvertDelegate<UnityAction<string>>(
                (Action<string>)SearchAvatars);
            _overrideSearchAvatarsAction = DelegateSupport.ConvertDelegate<UnityAction<string>>(
                (Action<string>)PromptChooseSearch);

            _avatarScreen = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
            _searchBox = GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs/ViewPort/Content/Search/InputField").GetComponent<UiInputField>();

            if (_localAvatars != null && _localAvatars.Count > 0)
            {
                var button = new ReUiButton($"Transfer {_localAvatars.Count}", new Vector2(165f, 375f), new Vector2(0.5f, 1f), () =>
                {
                    foreach (var avi in _localAvatars)
                    {
                        FavoriteAvatar(avi.AsApiAvatar());
                    }

                    File.Move("UserData/ReModCE/avatars.bin", "UserData/ReModCE/avatars_old.bin");

                    FetchAvatars();
                }, parent);
            }

            MelonCoroutines.Start(LoginToAPICoroutine());
        }

        public override void OnUpdate()
        {
            if (_searchBox == null)
                return;

            if (!_avatarScreen.active)
            {
                return;
            }

            if (!_searchBox.field_Public_Button_0.interactable)
            {
                if (!UiManager.IsEmmVRCLoaded)
                {
                    _searchBox.field_Public_Button_0.interactable = true;
                    _searchBox.field_Public_UnityAction_1_String_0 = _searchAvatarsAction;
                }
                // emmVRC will set it to be interactable. We want to grab their search function
            }
            else
            {
                if (UiManager.IsEmmVRCLoaded)
                {
                    if (_searchBox.field_Public_UnityAction_1_String_0 == null)
                        return;
                    
                    if (_searchBox.field_Public_UnityAction_1_String_0.method != _overrideSearchAvatarsAction.method)
                    {
                        if (_emmVRCsearchAvatarsAction == null)
                        {
                            _emmVRCsearchAvatarsAction = _searchBox.field_Public_UnityAction_1_String_0;
                        }
                        _searchBox.field_Public_UnityAction_1_String_0 = _overrideSearchAvatarsAction;
                    }
                }
            }
        }

        private void PromptChooseSearch(string searchTerm)
        {
            MelonCoroutines.Start(PrompSearchDelayed(searchTerm));
        }

        private IEnumerator PrompSearchDelayed(string searchTerm)
        {
            yield return new WaitForSeconds(1f);
            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowStandardPopupV2("Choose Search",
                "Choose whether you want to search with ReMod CE or emmVRC", "ReModCE",
                () =>
                {
                    SearchAvatars(searchTerm);
                    VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
                }, "emmVRC", () =>
                {
                    _emmVRCsearchAvatarsAction?.Invoke(searchTerm);
                    VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
                }, null);
        }

        private void SearchAvatars(string searchTerm)
        {
            ReLogger.Msg($"Searching for avatar {searchTerm}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/search.php?searchTerm={searchTerm}");

            _httpClient.SendAsync(request).ContinueWith(rsp =>
            {
                var searchResponse = rsp.Result;
                if (!searchResponse.IsSuccessStatusCode)
                {
                    searchResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(t.Result).Error;
                        ReLogger.Error($"Could not search for avatars: \"{errorMessage}\"");
                    });

                    return;
                }

                searchResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                {
                    var avatars = JsonConvert.DeserializeObject<List<ReAvatar>>(t.Result);
                    MelonCoroutines.Start(RefreshSearchedAvatars(avatars));
                });
            });
        }

        private IEnumerator RefreshSearchedAvatars(List<ReAvatar> results)
        {
            yield return new WaitForEndOfFrame();

            _searchedAvatars.Clear();
            foreach (var avi in results.Select(x => x.AsApiAvatar()).ToList())
            {
                _searchedAvatars.Add(avi);
            }

            ReLogger.Msg($"Found {_searchedAvatars.Count} avatars");
            _searchedAvatarList.RefreshAvatars();
        }

        private void ChangeAvatarChecked()
        {
            var currentAvatar = _avatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
            if (!HasAvatarFavorited(currentAvatar.id)) // this isn't in our list. we don't care about it
            {
                _changeButtonEvent.Invoke();
                return;
            }

            new ApiAvatar { id = currentAvatar.id }.Fetch(new Action<ApiContainer>(ac =>
            {
                var updatedAvatar = ac.Model.Cast<ApiAvatar>();
                switch (updatedAvatar.releaseStatus)
                {
                    case "private" when updatedAvatar.authorId != APIUser.CurrentUser.id:
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar is private and you don't own it. You can't switch into it.");
                        break;
                    case "unavailable":
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar has been deleted. You can't switch into it.");
                        break;
                    default:
                        _changeButtonEvent.Invoke();
                        break;
                }
            }), new Action<ApiContainer>(ac =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "This avatar has been deleted. You can't switch into it.");
            }));
        }

        private IEnumerator LoginToAPICoroutine()
        {
            while (APIUser.CurrentUser == null) yield return new WaitForEndOfFrame();

            var user = APIUser.CurrentUser;
            LoginToAPI(user);
        }

        private async void LoginToAPI(APIUser user)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrl}/login.php")
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new("user_id", user.id),
                    new("pin", _pinCode.ToString())
                })
            };

            var loginResponse = await _httpClient.SendAsync(request);
            if (loginResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                var errorData = await loginResponse.Content.ReadAsStringAsync();
                var errorMessage = JsonConvert.DeserializeObject<ApiError>(errorData).Error;

                ReLogger.Error($"Could not login to ReMod CE API: \"{errorMessage}\"");
                MelonCoroutines.Start(ShowAlertDelayed($"Could not login to ReMod CE API\nReason: \"{errorMessage}\""));
                File.Delete(PinPath);
                return;
            }

            if (_pinCode != 0 && _enterPinButton != null)
            {
                _enterPinButton.Interactable = false;
            }

            FetchAvatars();
        }

        private void FetchAvatars()
        {
            SendAvatarRequest(HttpMethod.Get, avatarResponse =>
            {
                if (!avatarResponse.IsSuccessStatusCode)
                {
                    avatarResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(t.Result).Error;
                        ReLogger.Error($"Could not fetch avatars: \"{errorMessage}\"");
                    });

                    return;
                }

                avatarResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                {
                    _savedAvatars = JsonConvert.DeserializeObject<List<ReAvatar>>(t.Result);
                });
            });
        }

        private static IEnumerator ShowAlertDelayed(string message, float seconds = 0.5f)
        {
            if (VRCUiPopupManager.prop_VRCUiPopupManager_0 == null) yield break;

            yield return new WaitForSeconds(seconds);

            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", message);
        }

        private void OnAvatarInstantiated(string url, GameObject avatar, AvatarPerformanceStats avatarPerformanceStats)
        {
            _favoriteButton.Text = HasAvatarFavorited(_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id) ? "Unfavorite" : "Favorite";
        }

        private void FavoriteAvatar(ApiAvatar apiAvatar)
        {
            var isSupporter = APIUser.CurrentUser.isSupporter;
            if (!isSupporter)
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", "You need VRC+ to use this feature.\nWe're not trying to destroy VRChat's monetization.");
                return;
            }

            var hasFavorited = HasAvatarFavorited(apiAvatar.id);
            
            SendAvatarRequest(hasFavorited ? HttpMethod.Delete : HttpMethod.Put, favResponse =>
            {
                if (!favResponse.IsSuccessStatusCode)
                {
                    favResponse.Content.ReadAsStringAsync().ContinueWith(errorData =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(errorData.Result).Error;

                        ReLogger.Error($"Could not (un)favorite avatar: \"{errorMessage}\"");
                        if (favResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            MelonCoroutines.Start(ShowAlertDelayed($"Could not (un)favorite avatar\nReason: \"{errorMessage}\""));
                        }
                    });
                }
            }, new ReAvatar(apiAvatar));

            if (_avatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id == apiAvatar.id)
            {
                if (!HasAvatarFavorited(apiAvatar.id))
                {
                    _savedAvatars.Insert(0, new ReAvatar(apiAvatar));
                    _favoriteButton.Text = "Unfavorite";
                }
                else
                {
                    _savedAvatars.RemoveAll(a => a.Id == apiAvatar.id);
                    _favoriteButton.Text = "Favorite";
                }
            }
            _avatarList.RefreshAvatars();
        }

        private void SendAvatarRequest(HttpMethod method, Action<HttpResponseMessage> onResponse, ReAvatar avater = null)
        {
            var request = new HttpRequestMessage(method, $"{ApiUrl}/avatar.php");
            if (avater != null)
            {
                request.Content = new StringContent(avater.ToJson(), Encoding.UTF8, "application/json");
            }

            _httpClient.SendAsync(request).ContinueWith(t => onResponse(t.Result));
        }

        private bool HasAvatarFavorited(string id)
        {
            return _savedAvatars.FirstOrDefault(a => a.Id == id) != null;
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            if (avatarList == _avatarList)
            {
                var list = new AvatarList();
                foreach (var avi in _savedAvatars.Select(x => x.AsApiAvatar()).ToList())
                {
                    list.Add(avi);
                }

                return list;
            }
            else if (avatarList == _searchedAvatarList)
            {
                return _searchedAvatars;
            }

            return null;
        }
    }
}
