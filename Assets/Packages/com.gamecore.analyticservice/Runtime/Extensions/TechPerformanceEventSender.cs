using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    public partial class TechPerformanceEventSender
    {
        private const int OriginCapacity = 1024;

        private readonly ILogger _logger;
        private readonly IAnalyticsService _analyticsService;
        private readonly TechPerformanceEventOptions _options;

        private readonly List<float> _fpsBuffer;
        private readonly List<float> _ramUsedBuffer;
        private readonly List<float> _ramAvailableBuffer;
        private readonly List<float> _storageAvailableBuffer;

        private bool _isSendingEvents;

        public float MedianFps => Median(_fpsBuffer, IndexRange.FromList(_fpsBuffer));
        public float MinFps => Average(_fpsBuffer, IndexRange.FromListAndFactors(_logger, _fpsBuffer, 0, _options.MinFactor));
        public float MaxFps => Average(_fpsBuffer, IndexRange.FromListAndFactors(_logger, _fpsBuffer, _options.MaxFactor, 1));

        public float LastRamUsed { get; private set; }
        public float MedianRamUsed => Median(_ramUsedBuffer, IndexRange.FromList(_ramUsedBuffer));
        public float MinRamUsed => Average(_ramUsedBuffer, IndexRange.FromListAndFactors(_logger, _ramUsedBuffer, 0, _options.MinFactor));
        public float MaxRamUsed => Average(_ramUsedBuffer, IndexRange.FromListAndFactors(_logger, _ramUsedBuffer, _options.MaxFactor, 1));

        public float LastRamAvailable => SystemMemorySize - LastRamUsed;
        public float MedianRamAvailable => Median(_ramAvailableBuffer, IndexRange.FromList(_ramAvailableBuffer));

        public float MedianStorageAvailable => Median(_storageAvailableBuffer, IndexRange.FromList(_storageAvailableBuffer));

        private static int SystemMemorySize => SystemInfo.systemMemorySize;

        public TechPerformanceEventSender(
            ILogger logger,
            IAnalyticsService analyticsService,
            TechPerformanceEventOptions options = null
        )
        {
            _logger = logger;
            _analyticsService = analyticsService;

            _options = options ?? new TechPerformanceEventOptions();
            IndexRange.ValidateRangeFactors(_options.MinFactor, _options.MaxFactor);

            _fpsBuffer = new List<float>(OriginCapacity);
            _ramUsedBuffer = new List<float>(OriginCapacity);
            _ramAvailableBuffer = new List<float>(OriginCapacity);
            _storageAvailableBuffer = new List<float>(OriginCapacity);
        }

        public bool AreAllBuffersFilledWithAny()
        {
            return
                _fpsBuffer.Count > 0 &&
                _ramUsedBuffer.Count > 0 &&
                _ramAvailableBuffer.Count > 0 &&
                _storageAvailableBuffer.Count > 0;
        }

        public async UniTask SendEvents(CancellationToken token = default)
        {
            if (_isSendingEvents)
            {
                _logger.Error($"attempted to {nameof(SendEvents)} while already is in progress");

                return;
            }

            try
            {
                _isSendingEvents = true;
                ClearBuffers();
                RunCapturing().Forget();

                while (!AreAllBuffersFilledWithAny())
                {
                    await UniTask.Yield(token);
                }

                float previousMoment = 0;
                foreach (float moment in _options.SendingMomentsSeconds)
                {
                    float delay = moment - previousMoment;
                    if (delay != 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                    }

                    previousMoment = moment;
                    SendEvent(moment);
                }
            }
            finally
            {
                ClearBuffers();
                _isSendingEvents = false;

                await UniTask.Yield();
            }
        }

        private void SendEvent(float timing)
        {
            Dictionary<string, object> parameters = new()
            {
                { "timing_id", Mathf.FloorToInt( timing ) },
                { "median_fps", MedianFps },
                { "min_fps", MinFps },
                { "max_fps", MaxFps },
                { "median_ram_used", MedianRamUsed },
                { "min_ram_used", MinRamUsed },
                { "max_ram_used", MaxRamUsed },
                { "ram_available", LastRamAvailable },
                { "median_ram_available", MedianRamAvailable },
                { "median_storage_available", MedianStorageAvailable }
            };

            _analyticsService.SendEvent(_options.EventId, parameters);
        }

        private void ClearBuffers()
        {
            _fpsBuffer.Clear();
            _ramUsedBuffer.Clear();
            _ramAvailableBuffer.Clear();
            _storageAvailableBuffer.Clear();

            LastRamUsed = 0;
        }

        private void CaptureSample()
        {
            LastRamUsed = GetRamUsed();

            _fpsBuffer.Add(GetCurrentFps());
            _ramUsedBuffer.Add(LastRamUsed);
            _ramAvailableBuffer.Add(SystemMemorySize - LastRamUsed);
            _storageAvailableBuffer.Add(DiskUtilsSafe.CheckAvailableSpace(_logger));
        }

        private float GetCurrentFps()
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            if (_options.MinSupportedFps > fps || fps > _options.MaxSupportedFps)
            {
                if (Debug.isDebugBuild)
                {
                    _logger.Warning(
                        $"Detected Incorrect FPS: {fps}   UDT: {Time.unscaledDeltaTime}   DT: {Time.deltaTime}   Scale: {Time.timeScale}"
                    );
                }
            }

            return Mathf.Clamp(fps, _options.MinSupportedFps, _options.MaxSupportedFps);
        }

        private float GetRamUsed()
        {
            return (float)GC.GetTotalMemory(false) / (1024 * 1024);
        }

        private async UniTask RunCapturing()
        {
            while (_isSendingEvents)
            {
                CaptureSample();
                await UniTask.DelayFrame(_options.SampleCaptureFramesInterval);
            }
        }

        private float Average(List<float> list, IndexRange range)
        {
            range.Validate(_logger, list);
            list.Sort();

            double sum = 0;
            for (int i = range.Min; i <= range.Max; i++)
            {
                sum += list[i];
            }

            return (float)(sum / range.IdsCount);
        }

        private float Median(List<float> list, IndexRange range)
        {
            range.Validate(_logger, list);
            list.Sort();

            return list[range.MiddleId];
        }
    }
}
