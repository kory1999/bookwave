using System;
using BW.Framework.Utils;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class AVProAudioPlayer : MonoBehaviour
    {
        private const string LogHeader = nameof(AVProAudioPlayer);
        
        public enum AudioPlayerStatus
        {
            Playing,
            Pause,
            Closed
        }

        public event Action<double, double> OnMediaPlayerUpdateFrameEvent;
        public bool IsPaused => _mediaPlayer.Control.IsPaused();
        public MediaPlayer MediaPlayer => _mediaPlayer;
        public event Action<MediaPlayerEvent.EventType> OnMediaPlayerEventTriggerEvent;
        public event Action<AudioPlayerStatus,AudioPlayerStatus> OnAVProPlayerStatusChangedEvent;
        public event Action<bool> OnMediaPauseStateChangedEvent;

        private AudioPlayerStatus _currentStatus = AudioPlayerStatus.Closed;

        public AudioPlayerStatus CurrentStatus
        {
            private set
            {
                Log($"set _currentStatus to {value}");
                
                _currentStatus = value;
            }
            get => _currentStatus;
        }
        
        
        private MediaPlayerEvent.EventType _lastTriggeredEventType = MediaPlayerEvent.EventType.Closing;

        private const string _audioPlayerLoadPath = "AVProAudioPlayer";
        private bool _isLastFramePause = false;
        private Func<AudioPlayerStatus> _playFinishCheckStateLogic;

        public static AVProAudioPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Instantiate(Resources.Load<GameObject>(_audioPlayerLoadPath))
                        .GetComponent<AVProAudioPlayer>();

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        private static AVProAudioPlayer _instance;

        [SerializeField] private MediaPlayer _mediaPlayer;

        public void Play(string url)
        {
            _mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, url, true);
        }

        public void Resume()
        {
            _mediaPlayer.Play();
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.CloseMedia();
        }

        public void SeekBySeconds(double position)
        {
            position = Mathf.Clamp((int) position, 0, (int) GetFullVideoLengthInSeconds());
            if (position >= 0)
            {
                _mediaPlayer.Control.Seek(position);
            }

            _mediaPlayer.Control.Play();
        }

        public void SeekByProgress(float progress)
        {
            if (progress >= 0 && progress <= 1)
            {
                _mediaPlayer.Control.Seek(progress * _mediaPlayer.Info.GetDuration());
            }

            _mediaPlayer.Control.Play();
        }

        public void SetSpeed(float factor)
        {
            _mediaPlayer.Control.SetPlaybackRate(factor);
        }

        public void SetPlayFinishStateLogic(Func<AudioPlayerStatus> logic)
        {
            _playFinishCheckStateLogic = logic;
        }

        public double GetFullVideoLengthInSeconds()
        {
            return _mediaPlayer.Info.GetDuration();
        }

        public double GetCurrentVideoTimeInSeconds()
        {
            if (_mediaPlayer == null || _mediaPlayer.Control == null)
            {
                return 0;
            }
            return _mediaPlayer.Control.GetCurrentTime();
        }

        private void FixedUpdate()
        {
            if (_mediaPlayer != null && _mediaPlayer.Info != null && _mediaPlayer.Info.GetDuration() > 0f)
            {
                OnMediaPlayerUpdateFrameEvent?.Invoke(GetCurrentVideoTimeInSeconds(), GetFullVideoLengthInSeconds());
            }
        }

        private void Update()
        {
            if (CurrentStatus == AudioPlayerStatus.Closed)
            {
                return;
            }
            
            if (IsPaused && _lastTriggeredEventType != MediaPlayerEvent.EventType.Closing && CurrentStatus == AudioPlayerStatus.Playing)
            {
                Log($"{DateTime.Now.ToString()} media player change status : {CurrentStatus.ToString()} to {AudioPlayerStatus.Pause.ToString()}");
                OnAVProPlayerStatusChangedEvent?.Invoke(CurrentStatus,AudioPlayerStatus.Pause);
                CurrentStatus = AudioPlayerStatus.Pause;
                
            }
            
            if(_isLastFramePause!= IsPaused)
            {
                OnMediaPauseStateChangedEvent?.Invoke(IsPaused);
                if (!IsPaused)
                {
                    Log($"{DateTime.Now.ToString()} media player change status : {CurrentStatus.ToString()} to {AudioPlayerStatus.Playing.ToString()}");
                    OnAVProPlayerStatusChangedEvent?.Invoke(CurrentStatus,AudioPlayerStatus.Playing);
                    CurrentStatus = AudioPlayerStatus.Playing;
                }
            }
            
            _isLastFramePause = IsPaused;
        }

        private void Awake()
        {
            Application.runInBackground = true;
            _mediaPlayer.Events.AddListener(HandleOnMediaPlayerEvents);
        }

        private void HandleOnMediaPlayerEvents(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType,
            ErrorCode errorCode)
        {
            Log($"{DateTime.Now.ToString()} media player trigger event : {eventType.ToString()}");
            AudioPlayerStatus newStatus = CurrentStatus;

            if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
            {
                newStatus = _playFinishCheckStateLogic?.Invoke() ?? AudioPlayerStatus.Closed;
            }
            // else if (eventType == MediaPlayerEvent.EventType.Closing)
            // {
            //     newStatus = AudioPlayerStatus.Closed;
            // }
            else if (eventType == MediaPlayerEvent.EventType.Started ||
                     eventType == MediaPlayerEvent.EventType.FinishedSeeking ||
                     eventType == MediaPlayerEvent.EventType.FinishedBuffering ||
                     eventType == MediaPlayerEvent.EventType.Unstalled)
            {
                newStatus = AudioPlayerStatus.Playing;
            }
            else if (eventType == MediaPlayerEvent.EventType.Stalled ||
                     eventType == MediaPlayerEvent.EventType.StartedBuffering ||
                     eventType == MediaPlayerEvent.EventType.StartedSeeking)
            {
                newStatus = AudioPlayerStatus.Pause;
            }

            if (newStatus != CurrentStatus)
            {
                Log($"{DateTime.Now.ToString()} media player change status : {CurrentStatus.ToString()} to {newStatus.ToString()}");
                OnAVProPlayerStatusChangedEvent?.Invoke(CurrentStatus,newStatus);
                
                CurrentStatus = newStatus;
            }

            _lastTriggeredEventType = eventType;
            OnMediaPlayerEventTriggerEvent?.Invoke(eventType);
        }

        private void Log(string info)
        {
            BaseLogger.Log(LogHeader, info);
        }
    }
}