using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BW.Framework.Utils;
using BW.FunLearning.Manager;
using UnityEngine;
using UnityEngine.UI;
using GameManager = BeWild.AIBook.Runtime.Manager.GameManager;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryHistoryBook : BookListBook
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private GameObject finishMark;
        [SerializeField] private Button popupButton;
        [SerializeField] private Button downloadButton;
        [SerializeField] private Image downloadStateImage;
        [SerializeField] private Sprite[] downloadStateSprites;
        [SerializeField] private RectTransform historyPopupAnchorLeft;
        [SerializeField] private RectTransform historyPopupAnchorRight;

        [SerializeField] private LibraryHistoryBookPopup historyPopup;
        private float progressParentWidth;

        public override void Initialize(Action<int> callback)
        {
            base.Initialize(callback);
        }

        public void SetCallback(Action isShowCallback, Action<int> finishCallback, Action<int> shareCallback,
            Action<int> deleteCallback)
        {
            popupButton.onClick.AddListener((() =>
            {
                GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickHistorySubMenu);
                historyPopup.transform.position = (transform.GetSiblingIndex() == 0 || transform.GetSiblingIndex()%2!=0) ? historyPopupAnchorRight.position : historyPopupAnchorLeft.position;
                historyPopup.ToggleVisual(true);
                historyPopup.transform.SetParent(transform.parent.parent);
                isShowCallback.Invoke();
            }));
            historyPopup.SetUp((() => { finishCallback.Invoke(Id); }), (() => { shareCallback.Invoke(Id); }),
                (() =>
                {
                    GlobalEvent.GetEvent<DeleteBookSoundCacheEvent>().Publish(Id);
                    deleteCallback.Invoke(Id);
                }), () =>
                {
                    HandleOnDownload();
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickPopupDownload);
                });
            downloadButton.onClick.AddListener(() =>
            {
                HandleOnDownload();
                GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickBookDownload);
            });
        }

        public override void SetData(int id, string bookName, string author, string imageUrl,bool isFree)
        {
            base.SetData(id, bookName, author, imageUrl,isFree);
            GlobalEvent.GetEvent<GetBookSoundCacheEvent>().Publish(Id, list => { SetDownloadState(list != null); });
        }

        public void CloseHistoryPopup()
        {
            historyPopup.transform.SetParent(transform);
            historyPopup.ToggleVisual(false);
        }


        public void SetReadData(BookReadHistoryData historyData)
        {
            if (historyData != null)
            {
                historyPopup.ToggleFinishedButton(!historyData.isFinished);
                finishMark.SetActive(historyData.isFinished);

                SetProgress(historyData.progress);
                if (historyData.isFinished)
                {
                    progressBar.transform.parent.GetComponent<CanvasGroup>().ToggleEnable(false);
                }
                else
                {
                    progressBar.transform.parent.GetComponent<CanvasGroup>().ToggleEnable(true);
                }
            }
        }

        private void SetProgress(float progress)
        {
            Vector2 tmp = progressBar.GetComponent<RectTransform>().sizeDelta;
            tmp.x = Mathf.Lerp(20, 365.46f, progress);
            progressBar.GetComponent<RectTransform>().sizeDelta = tmp;
        }

        private void HandleOnDownload()
        {
            if (GameManager.IsGameUnlocked||_isFree)
            {
                downloadButton.GetComponent<Button>().interactable = false;
                historyPopup.ToggleDownloadButton(false);
                
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Start download",
                    text => { GlobalEvent.GetEvent<ShowToastEvent>().Publish(text, 0.2f); });
                GlobalEvent.GetEvent<DownloadBookSoundEvent>().Publish(Id, SetDownloadStateAndToast);
            }
            else
            {
                GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_DownloadButton);
            }
        }
        
        private void SetDownloadStateAndToast(bool on)
        {
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(on ? "Download successful" : "Download failed",
                text => { GlobalEvent.GetEvent<ShowToastEvent>().Publish(text, 0.2f); });
            SetDownloadState(on);
        }

        private void SetDownloadState(bool on)
        {
            downloadButton.GetComponent<Button>().interactable = !on;
            historyPopup.ToggleDownloadButton(!on);
            downloadStateImage.sprite = downloadStateSprites[on ? 1 : 0];
        }
    }
}