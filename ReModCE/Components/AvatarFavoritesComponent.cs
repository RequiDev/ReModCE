﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using MelonLoader;
using Newtonsoft.Json;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Core;
using ReModCE.Loader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;
using BuildInfo = ReModCE.Loader.BuildInfo;

namespace ReModCE.Components
{
    internal class AvatarFavoritesComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _favoriteAvatarList;
        private ReUiButton _favoriteButton;

        private ReAvatarList _searchedAvatarList;

        private Button.ButtonClickedEvent _changeButtonEvent;

        private const bool EnableApi = true;
        private const string ApiUrl = "https://remod-ce.requi.dev/api";
        private string _userAgent = "";
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;

        private const string PinPath = "UserData/ReModCE/pin";
        private int _pinCode;
        private ReMenuButton _enterPinButton;

        private ConfigValue<bool> AvatarFavoritesEnabled;
        private ReMenuToggle _enabledToggle;
        private ConfigValue<int> MaxAvatarsPerPage;
        private ReMenuButton _maxAvatarsPerPageButton;

        private List<ReAvatar> _savedAvatars;
        private readonly AvatarList _searchedAvatars;

        private GameObject _avatarScreen;
        private UiInputField _searchBox;
        private UnityAction<string> _searchAvatarsAction;
        private UnityAction<string> _overrideSearchAvatarsAction;
        private UnityAction<string> _emmVRCsearchAvatarsAction;
        private int _updatesWithoutSearch;

        private int _loginRetries;

        public AvatarFavoritesComponent()
        {
            AvatarFavoritesEnabled = new ConfigValue<bool>(nameof(AvatarFavoritesEnabled), true);
            AvatarFavoritesEnabled.OnValueChanged += () =>
            {
                _enabledToggle.Toggle(AvatarFavoritesEnabled);
                _favoriteAvatarList.GameObject.SetActive(AvatarFavoritesEnabled);
                _favoriteButton.GameObject.SetActive(AvatarFavoritesEnabled);
            };
            MaxAvatarsPerPage = new ConfigValue<int>(nameof(MaxAvatarsPerPage), 100);
            MaxAvatarsPerPage.OnValueChanged += () =>
            {
                _favoriteAvatarList.SetMaxAvatarsPerPage(MaxAvatarsPerPage);
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
        }

        private void InitializeNetworkClient()
        {
            if (!EnableApi)
                return;

            _httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };
            _httpClient = new HttpClient(_httpClientHandler);

            var vrHeadset = XRDevice.isPresent ? XRDevice.model : "Desktop";
            vrHeadset = vrHeadset.Replace(' ', '_');

            _userAgent = $"ReModCE/{vrHeadset}.{BuildInfo.Version} (Windows NT 10.0; Win64; x64)";
            
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
        }

        public override void OnUiManagerInitEarly()
        {
            InitializeNetworkClient();

            _searchedAvatarList = new ReAvatarList("ReModCE Search", this);

            _favoriteAvatarList = new ReAvatarList("ReModCE Favorites", this, false);
            _favoriteAvatarList.AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);
            _favoriteAvatarList.OnEnable += () =>
            {
                // make sure it stays off if it should be off.
                _favoriteAvatarList.GameObject.SetActive(AvatarFavoritesEnabled);
            };

            var parent = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button").transform.parent;
            _favoriteButton = new ReUiButton("Favorite", new Vector2(-600f, 375f), new Vector2(0.5f, 1f),
                () => FavoriteAvatar(_favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0),
                parent);
            _favoriteButton.GameObject.SetActive(AvatarFavoritesEnabled);

            var changeButton = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button");
            if (changeButton != null)
            {
                var button = changeButton.GetComponent<Button>();
                _changeButtonEvent = button.onClick;

                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(new Action(ChangeAvatarChecked));
            }


            _searchAvatarsAction = DelegateSupport.ConvertDelegate<UnityAction<string>>(
                (Action<string>)SearchAvatars);
            _overrideSearchAvatarsAction = DelegateSupport.ConvertDelegate<UnityAction<string>>(
                (Action<string>)PromptChooseSearch);

            _avatarScreen = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
            _searchBox = GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs/ViewPort/Content/Search/InputField").GetComponent<UiInputField>();

            MelonCoroutines.Start(LoginToAPICoroutine());
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            if (ReModCE.IsRubyLoaded)
            {
                _favoriteButton.Position += new Vector3(420f, 0f);
            }

            var menu = uiManager.MainMenu.GetMenuPage("Avatars");
            _enabledToggle = menu.AddToggle("Avatar Favorites", "Enable/Disable avatar favorites (requires VRC+)", AvatarFavoritesEnabled);
            _maxAvatarsPerPageButton = menu.AddButton($"Avatars Per Page: {MaxAvatarsPerPage}",
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
                }, ResourceManager.GetSprite("remodce.max"));

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

                            InitializeNetworkClient();

                            LoginToAPI(APIUser.CurrentUser, FetchAvatars);
                        }, null);
                }, ResourceManager.GetSprite("remodce.padlock"));
            }
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
                if (!ReModCE.IsEmmVRCLoaded || _updatesWithoutSearch >= 10)
                {
                    _searchBox.field_Public_Button_0.interactable = true;
                    _searchBox.field_Public_UnityAction_1_String_0 = _searchAvatarsAction;
                }
                else if (ReModCE.IsEmmVRCLoaded)
                {
                    ++_updatesWithoutSearch;
                }
                // emmVRC will set it to be interactable. We want to grab their search function
            }
            else
            {
                if (ReModCE.IsEmmVRCLoaded && _updatesWithoutSearch < 10)
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
            var popupManager = VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0;
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 3)
            {
                popupManager.ShowStandardPopupV2("ReModCE Search", "That search term is too short. The search term has to be at least 3 characters.", "I'm sorry!",
                    () =>
                    {
                        popupManager.HideCurrentPopup();
                    });
                return;
            }

            if (!EnableApi)
            {
                popupManager.ShowStandardPopupV2("ReModCE API", "ReModCE API is currently down for maintenance. This will take about 12-24 hours. During this time, your avatar favorites in ReModCE are unavailable and search will be disabled.\nThank you for your patience!", "OK!",
                    () =>
                    {
                        popupManager.HideCurrentPopup();
                    });
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/search.php?searchTerm={searchTerm}");
            
            _httpClient.SendAsync(request).ContinueWith(rsp =>
            {
                var searchResponse = rsp.Result;
                if (!searchResponse.IsSuccessStatusCode)
                {
                    if (searchResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ReLogger.Msg($"Not logged into ReMod CE API anymore. Trying to login again and resuming request.");
                        LoginToAPI(APIUser.CurrentUser, () => SearchAvatars(searchTerm));
                        return;
                    }

                    searchResponse.Content.ReadAsStringAsync().ContinueWith(errorData =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(errorData.Result).Error;

                        ReLogger.Error($"Could not search for avatars: \"{errorMessage}\"");
                        if (searchResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            MelonCoroutines.Start(ShowAlertDelayed($"Could not search for avatars\nReason: \"{errorMessage}\""));
                        }
                    });
                }
                else
                {
                    searchResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                    {
                        var avatars = JsonConvert.DeserializeObject<List<ReAvatar>>(t.Result) ?? new List<ReAvatar>();
                        MelonCoroutines.Start(RefreshSearchedAvatars(avatars));
                    });
                }
            });
        }

        private IEnumerator RefreshSearchedAvatars(List<ReAvatar> results)
        {
            yield return new WaitForEndOfFrame();

            _searchedAvatars.Clear();
            foreach (var avi in results.Select(x => x.AsApiAvatar()))
            {
                _searchedAvatars.Add(avi);
            }

            ReLogger.Msg($"Found {_searchedAvatars.Count} avatars");
            _searchedAvatarList.RefreshAvatars();
        }

        private void ChangeAvatarChecked()
        {
            if (!AvatarFavoritesEnabled)
            {
                _changeButtonEvent.Invoke();
                return;
            }

            var currentAvatar = _favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
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
            LoginToAPI(user, FetchAvatars);
        }

        private void LoginToAPI(APIUser user, Action onLogin)
        {
            if (!EnableApi)
            {
                return;
            }
            if (_loginRetries >= 3)
            {
                ReLogger.Error($"Could not login to ReModCE API: Exceeded retries. Please restart your game and make sure your pin is correct!");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrl}/login.php")
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new("user_id", user.id),
                    new("pin", _pinCode.ToString())
                })
            };

            ++_loginRetries;
            _httpClient.SendAsync(request).ContinueWith(t =>
            {
                var loginResponse = t.Result;
                if (!loginResponse.IsSuccessStatusCode)
                {
                    loginResponse.Content.ReadAsStringAsync().ContinueWith(tsk =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(tsk.Result).Error;

                        ReLogger.Error($"Could not login to ReMod CE API: \"{errorMessage}\"");
                        MelonCoroutines.Start(ShowAlertDelayed($"Could not login to ReMod CE API\nReason: \"{errorMessage}\""));

                        switch (loginResponse.StatusCode)
                        {
                            case HttpStatusCode.Forbidden:
                                File.Delete(PinPath);
                                _pinCode = 0;
                                break;
                            default:
                                break;
                        }
                    });
                }
                else
                {
                    if (_pinCode != 0 && _enterPinButton != null)
                    {
                        _enterPinButton.Interactable = false;
                    }

                    _loginRetries = 0;

                    onLogin();
                }
            });
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
            _favoriteButton.Text = HasAvatarFavorited(_favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id) ? "Unfavorite" : "Favorite";
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
                    if (favResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ReLogger.Msg($"Not logged into ReMod CE API anymore. Trying to login again and resuming request.");
                        LoginToAPI(APIUser.CurrentUser, () => FavoriteAvatar(apiAvatar));
                        return;
                    }

                    favResponse.Content.ReadAsStringAsync().ContinueWith(errorData =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(errorData.Result).Error;
                        ReLogger.Error($"Could not (un)favorite avatar: \"{errorMessage}\"");
                        if (favResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            MelonCoroutines.Start(ShowAlertDelayed($"Could not (un)favorite avatar\nReason: \"{errorMessage}\""));
                        }
                    });
                }
            }, new ReAvatar(apiAvatar));

            if (_favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id == apiAvatar.id)
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

            _favoriteAvatarList.RefreshAvatars();
        }

        private void SendAvatarRequest(HttpMethod method, Action<HttpResponseMessage> onResponse, ReAvatar avater = null)
        {
            if (!EnableApi)
                return;

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
            if (avatarList == _favoriteAvatarList)
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

        public void Clear(ReAvatarList avatarList)
        {
            if (avatarList == _searchedAvatarList)
            {
                _searchedAvatars.Clear();
                avatarList.RefreshAvatars();
            }
        }
    }
}
