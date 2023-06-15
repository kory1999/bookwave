using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.UserGuide;
using BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList;
using BeWild.AIBook.Runtime.Scene.Popup;
using BeWild.Framework.Runtime.Utils;
using BW.Framework.Utils;
using DP.Base.Utilities.NativeAudioHelper;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene
{
    public class MainScene : MonoBehaviour
    {
        public static MainSceneEvent Event = new MainSceneEvent();

        [SerializeField] private MainSceneUI _ui;
        [SerializeField] private PopupHelper _popupHelper;
        [SerializeField] private FPSDisplay _fpsDisplay;

        private static bool _initialized;
        private bool _gameStoreLaunched;
        private bool _gameStoreReady;
        private bool _showingTutorial;
        private int _quitPopupId;

        void Start()
        {
            InitializeGame(StartSceneLogic);
        }

        private void InitializeGame(Action callback)
        {
            MobileKeyboardManager.Instance.AddBackListener(HandleOnBackButton,
                BookwavesConstants.BackButtonPriority_MainScene);

            if (GameManager.Initialized)
            {
                return;
            }

            GameManager.OnGameInitialized += HandleOnGameInitialized;
            GameManager.OnGameLaunched += HandleGameLaunched;
            GameManager.OnGameStoreReady += HandleOnGameStoreReady;
            GameManager.OnGameTestModeChanged += HandleOnGameTestModeChanged;
            GameManager.Initialize();
            
            
            void HandleOnGameInitialized()
            {
                GameManager.OnGameInitialized -= HandleOnGameInitialized;
                InitializeSceneLogic();
                callback?.Invoke();
            }
        }

        private void InitializeSceneLogic()
        {
            _ui.Initialize();
            
            _popupHelper.Initialize();
            
            InitializePopPlayList();

            Event.GetEvent<OpenBookEvent>().Subscribe(HandleOnRequiresToOpenBook);
            Event.GetEvent<OpenBookContentEvent>().Subscribe(HandleOnRequiresToOpenBookContent);
            Event.GetEvent<OpenHomePageEvent>().Subscribe(HandleOnRequiresToOpenHomePage);
        }

        private void HandleOnBackButton()
        {
            string[] texts = new[]
            {
                "Quit Bookwaves", "Are you sure to quit Bookwaves?", "Quit"
            };
            
            GlobalEvent.GetEvent<GetLocalizationArrayEvent>().Publish(texts, results =>
            {
                GlobalEvent.GetEvent<ShowPopupEvent>().Publish(new PopupConfigurations()
                {
                    Title = results[0],
                    Text = results[1],
                    ButtonText = results[2],
                    ShowCloseButton = true,
                    ShowGift = false,
                    ButtonCallback = HandleOnQuitPopupCallback
                }, id => _quitPopupId = id);
            });
        }

        private void HandleOnQuitPopupCallback(bool confirm)
        {
            if (confirm)
            {
                Application.Quit();
            }
            else
            {
                GlobalEvent.GetEvent<ClosePopupEvent>().Publish(_quitPopupId);
            }
        }

        private void StartSceneLogic()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_MainScene);

            _ui.DoStart();

            TryToShowTutorial();
            
            _fpsDisplay.ToggleEnable(GameManager.IsTestModeEnabled);
        }
        
        private void HandleOnGameTestModeChanged(bool enabled)
        {
            _fpsDisplay.ToggleEnable(enabled);
        }

        private void HandleGameLaunched()
        {
            GameManager.OnGameLaunched -= HandleGameLaunched;
                
            BaseLogger.Log(nameof(MainScene), $"on game launched.");
            
            DPBaseNativeAudioHelper.SetAudioCategoryPlayback();

            _gameStoreLaunched = true;
            
            TryToShowStore();
        }

        private void HandleOnGameStoreReady()
        {
            GameManager.OnGameStoreReady -= HandleOnGameStoreReady;
                
            BaseLogger.Log(nameof(MainScene), $"on game store ready.");

            _gameStoreReady = true;
            
            TryToShowStore();
        }

        private void HandleOnRequiresToOpenBook(int id)
        {
            GlobalEvent.GetEvent<GetBookEvent>().Publish(id, bookBrief =>
            {
                if (bookBrief != null)
                {
                    OpenBookBrief(bookBrief);
                }
                else
                {
                    BaseLogger.Log(nameof(MainScene), $"can't find book brief of {id}, check backend data.",
                        LogType.Error);
                }
            });
        }

        private void OpenBookBrief(BookBriefData data)
        {
            GameManager.RuntimeDataManager.BookBriefData = data;
            Event.GetEvent<OpenBookBriefEvent>().Publish(data);
            _ui.OpenPage(MainScenePage.BookBrief);
        }

        private void HandleOnRequiresToOpenBookContent(BookBriefData data, int chapterId, bool showTextPage,double jumpToCurrentSeconds)
        {
            if (data.isFree || GameManager.IsGameUnlocked || GameManager.IsFreeChapter(chapterId))
            {
                GameManager.RuntimeDataManager.BookBriefData = data;
                GameManager.RuntimeDataManager.CurrentSelectChapter = chapterId;
                GameManager.RuntimeDataManager.IsTextPageShown = showTextPage;
                GameManager.RuntimeDataManager.JumpToCurrentSeconds = jumpToCurrentSeconds;
                
                _ui.OpenPage(MainScenePage.BookContent);
            }
            else
            {
                GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_Book);
            }
        }

        private void HandleOnRequiresToOpenHomePage()
        {
            _ui.OpenPage(MainScenePage.Home);
        }

        #region user guide

        private void TryToShowTutorial()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsHelper.Key_UserGuide))
            {
                UserGuidePage.Data data = new UserGuidePage.Data()
                {
                    FinishCallback = HandleOnTutorialFinish
                };
                
                if (GameManager.IsPadDevice)
                {
                    OverlayPage.Instance.Show<UserGuidePagePad>(data);
                }
                else
                {
                    OverlayPage.Instance.Show<UserGuidePage>(data);
                }

                PlayerPrefs.SetString(PlayerPrefsHelper.Key_UserGuide, String.Empty);

                _showingTutorial = true;
            }
        }

        private void HandleOnTutorialFinish()
        {
            _showingTutorial = false;

            TryToShowStore();
        }

        private void TryToShowStore()
        {
            if (!_showingTutorial && _gameStoreLaunched && _gameStoreReady && !GameManager.IsGameUnlocked)
            {
                int openCount = PlayerPrefs.GetInt(PlayerPrefsHelper.Key_OpenAppConversionPage, 0);
                
                BaseLogger.Log(nameof(MainScene), $"open game store, open count: {openCount}.");
                
                if (openCount < 2)
                {
                    GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_FirstDay);
                    PlayerPrefs.SetInt(PlayerPrefsHelper.Key_OpenAppConversionPage, openCount + 1);
                }
            }
        }

        private void InitializePopPlayList()
        {
            PlayListCenter.Instance.Initialize();
        }

        #endregion
    }
}