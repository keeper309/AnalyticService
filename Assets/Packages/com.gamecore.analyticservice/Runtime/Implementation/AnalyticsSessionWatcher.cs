using System;
using System.Globalization;
using GameCore.GeneralExtensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    public class AnalyticsSessionWatcher : IInitializable, IDisposable
    {
        public event Action<string> OnSessionStart;
        private readonly ILogger _logger;
        private readonly IAppLifecycleProvider _appLifecycleProvider;

        private readonly TimeSpan _allowedGap;
        private readonly TimeSpan _updateInterval;
        private readonly string _dataStorageKey;

        private bool _isInitialized;
        private bool _isInFocus;

        public DateTime SessionStartTime { get; private set; }
        public TimeSpan SessionDuration => GetSessionDuration();

        public string SessionId { get; private set; }

        public AnalyticsSessionWatcher(ILogger logger, IAppLifecycleProvider lifecycleProvider, SessionWatcherSettings settings = null)
        {
            _logger = logger;
            _appLifecycleProvider = lifecycleProvider;

            settings ??= new SessionWatcherSettings();

            _allowedGap = TimeSpan.FromMinutes(settings.AllowedGapMinutes);
            _updateInterval = TimeSpan.FromSeconds(settings.UpdateIntervalSeconds);
            _dataStorageKey = settings.DataStorageKey;

            _appLifecycleProvider.OnAppPause += OnAppPause;
            _appLifecycleProvider.OnAppFocus += OnAppFocus;
            _appLifecycleProvider.OnAppQuit += OnAppQuit;
        }

        public void Dispose()
        {
            _isInitialized = false;
        }

        public UniTask Initialize(IProgressReceiver progressReceiver)
        {
            if (_isInitialized)
                return UniTask.CompletedTask;

            HandleFocus();
            _isInitialized = true;
            _isInFocus = true;

            Run();

            return UniTask.CompletedTask;
        }

        private TimeSpan GetSessionDuration()
        {
            TimeSpan duration = DateTime.Now - SessionStartTime;

            return duration;
        }

        private async void Run()
        {
            while (_isInitialized && _isInFocus)
            {
                Tick();
                await UniTask.Delay(_updateInterval);
            }
        }

        private void Tick()
        {
            WriteData();
        }

        private void OnAppQuit()
        {
            HandleFocusLost();
        }

        private void OnAppFocus(bool isFocus)
        {
            if (isFocus)
                HandleFocus();
            else
                HandleFocusLost();
        }

        private void OnAppPause(bool isPause)
        {
            if (isPause)
                HandleFocusLost();
            else
                HandleFocus();
        }

        private void HandleFocus()
        {
            string data = PlayerPrefs.GetString(_dataStorageKey);
            _isInFocus = true;
            if (string.IsNullOrEmpty(data))
            {
                SessionId = Guid.NewGuid().ToString();
                SessionStartTime = DateTime.Now;
                try
                {
                    OnSessionStart?.Invoke(SessionId);
                }
                catch (Exception e)
                {
                    _logger.Warning($"Exception thrown while calling OnSessionStart.\n{e}");
                }
            }
            else
            {
                SessionData sessionData = ParseSessionData(data);
                bool sessionExpired = IsSessionExpired(sessionData);
                if (sessionExpired)
                {
                    SessionId = Guid.NewGuid().ToString();
                    SessionStartTime = DateTime.Now;

                    try
                    {
                        OnSessionStart?.Invoke(SessionId);
                    }
                    catch (Exception e)
                    {
                        _logger.Warning($"Exception thrown while calling OnSessionStart.\n{e}");
                    }
                }
                else
                {
                    SessionId = sessionData.sessionId;
                    SessionStartTime = sessionData.SessionStartTime;
                }
            }
        }

        private SessionData ParseSessionData(string json)
        {
            try
            {
                SessionData sessionData = JsonUtility.FromJson<SessionData>(json);

                return sessionData;
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to parse session data.\n{e}");

                return null;
            }
        }

        private bool IsSessionExpired(SessionData sessionData)
        {
            if (sessionData == null)
                return true;

            TimeSpan delta = DateTime.Now - sessionData.ReportTime;

            return delta > _allowedGap;
        }

        private void HandleFocusLost()
        {
            _isInFocus = false;
            WriteData();
            PlayerPrefs.Save();
        }

        private void WriteData()
        {
            SessionData sessionData = new()
            {
                sessionId = SessionId,
                reportTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                sessionStartTime = SessionStartTime.ToString(CultureInfo.InvariantCulture)
            };
            string json = JsonUtility.ToJson(sessionData);
            PlayerPrefs.SetString(_dataStorageKey, json);
        }

        [Serializable]
        private class SessionData
        {
            public string sessionId;
            public string reportTime;
            public string sessionStartTime;

            public DateTime ReportTime => string.IsNullOrEmpty(reportTime)
                ? DateTime.Now
                : DateTime.Parse(reportTime, CultureInfo.InvariantCulture);

            public DateTime SessionStartTime => string.IsNullOrEmpty(sessionStartTime)
                ? DateTime.Now
                : DateTime.Parse(sessionStartTime, CultureInfo.InvariantCulture);

            public override string ToString()
            {
                return $"SessionData: sessionId: {sessionId}, reportTime: {ReportTime}, sessionStartTime: {SessionStartTime}";
            }
        }

        public class SessionWatcherSettings
        {
            public float AllowedGapMinutes { get; set; } = 30;
            public float UpdateIntervalSeconds { get; set; } = 60;
            public string DataStorageKey { get; set; } = "session-watcher-data";
        }
    }
}
