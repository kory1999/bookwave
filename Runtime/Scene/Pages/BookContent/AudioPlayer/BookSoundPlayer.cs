using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.Framework.Runtime.Utils.AudioPlayer;
using BW.Framework.Utils;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class BookSoundPlayer
    {
        public Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus CurrentStatus =>
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.CurrentStatus;
        
        public float CurrentSeconds => _currentSeconds;
        public event Action<float, float> OnFrameUpdateEvent;
        public event Action<Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType> OnAudioPlayerEvent;

        public event
            Action<Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus,
                Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus> OnStatusChangedEvent;

        private List<AudioPlayerClip> _soundList;
        private List<AudioPlayerClip> _soundListForPlugin;
        private Coroutine _delayCoroutine;
        private int _currentPageIndex = -1;
        private float _currentSeconds;
        private float _totalSeconds;

        public void Initialize()
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Initialize();
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.OnMediaPlayerEvent += HandleOnAudioPlayerEvents;
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.OnMediaPlayerUpdateFrameEvent +=
                HandleOnProgressUpdatePerSecond;
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.OnMediaPlayerStatusChanged +=
                HandleOnAudioPlayerStatusChanged;
        }

        public void Prepare(List<AudioPlayerClip> soundList, int bookID, Action callback)
        {
            if (_soundList != null)
            {
                if (_soundList.Count != soundList.Count)
                {
                    _soundList = soundList;
                    Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Stop();
                }
                else
                {
                    for (int i = 0; i < _soundList.Count; i++)
                    {
                        if (_soundList[i].audioUrl != soundList[i].audioUrl)
                        {
                            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Stop();
                            break;
                        }
                    }
                }
            }

            _soundList = soundList;

            FilterSoundList(bookID, _soundList, pluginList =>
            {
                _soundListForPlugin = pluginList;
                Framework.Runtime.Utils.AudioPlayer.AudioPlayer.SetPlayList(_soundListForPlugin);
                callback?.Invoke();
            });
        }

        public bool IsPlaying()
        {
            return Framework.Runtime.Utils.AudioPlayer.AudioPlayer.IsPlaying();
        }

        public bool Play(int startPageIndex = 0)
        {
            if (startPageIndex >= _soundListForPlugin.Count)
            {
                _soundListForPlugin = _soundList;
                Framework.Runtime.Utils.AudioPlayer.AudioPlayer.SetPlayList(_soundListForPlugin);
                BaseLogger.Log(nameof(BookSoundPlayer),
                    "Play: _currentPageIndex >= _soundListForPlugin.Count. So recover to _soundList");
            }

            _currentPageIndex = startPageIndex;
            return Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Play(_currentPageIndex);
        }

        public void SetTime(float seconds)
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.SetCurrentTime(seconds);
        }

        public void SetProgress(float progress)
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.SetCurrentTime(
                Framework.Runtime.Utils.AudioPlayer.AudioPlayer.GetDuration() * progress);
        }

        public void Resume()
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Resume();
        }

        public void Stop()
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Stop();
        }

        public void Pause()
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Pause();
        }

        public void StartDelayToPause(float seconds)
        {
            StopDelayCoroutine();
            _delayCoroutine = DelayInvoker.Instance.StartCoroutine(DelayToPause(seconds));
        }

        public void SetSpeed(float speed)
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.SetSpeed(speed);
        }

        private void StopDelayCoroutine()
        {
            if (_delayCoroutine != null)
            {
                DelayInvoker.Instance.StopCoroutine(_delayCoroutine);
                _delayCoroutine = null;
            }
        }

        private void HandleOnAudioPlayerEvents(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType eventType)
        {
            OnAudioPlayerEvent?.Invoke(eventType);
        }

        private void HandleOnAudioPlayerStatusChanged(
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus oldStatus,
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus newStatus)
        {
            OnStatusChangedEvent?.Invoke(oldStatus, newStatus);
        }

        public void PlayNextClip()
        {
            _currentPageIndex++;
            if (_currentPageIndex < _soundList.Count)
            {
                Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Play(_currentPageIndex);
            }
        }

        public bool IsLastSoundClip()
        {
            return _currentPageIndex >= _soundList.Count - 1;
        }

        private void PlayPreviousClip()
        {
            _currentPageIndex--;
            if (_currentPageIndex >= 0)
            {
                Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Play(_currentPageIndex);
            }
        }

        private void HandleOnProgressUpdatePerSecond(float currentSeconds, float totalSeconds)
        {
            _currentSeconds = currentSeconds;
            _totalSeconds = totalSeconds;
            OnFrameUpdateEvent?.Invoke(currentSeconds, totalSeconds);
        }

        private IEnumerator DelayToPause(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.Pause();
        }

        private void FilterSoundList(int bookID, List<AudioPlayerClip> soundList,
            Action<List<AudioPlayerClip>> callback)
        {
            List<AudioPlayerClip> result = new List<AudioPlayerClip>();
            GameManager.BookUnlocker.CheckBookUnlock(bookID, (unlock) =>
            {
                if (unlock)
                {
                    callback?.Invoke(soundList);
                }
                else
                {
                    result.Add(soundList[0]);
                    callback?.Invoke(result);
                }
            });
        }
    }
}