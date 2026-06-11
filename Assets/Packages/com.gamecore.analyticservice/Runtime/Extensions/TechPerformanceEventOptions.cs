using System.Collections.Generic;

namespace GameCore.AnalyticService
{
    public class TechPerformanceEventOptions
    {
#if UNITY_EDITOR
        private const int DefaultMaxFps = 10000;
#else
        private const int DefaultMaxFps = 240;
#endif

        public readonly float MinFactor;
        public readonly float MaxFactor;
        public readonly float MinSupportedFps;
        public readonly float MaxSupportedFps;

        public readonly List<float> SendingMomentsSeconds;
        public readonly int SampleCaptureFramesInterval;
        public readonly string EventId;

        public TechPerformanceEventOptions(
            List<float> sendingMoments = null,
            int sampleCaptureFramesInterval = 5,
            string eventId = "LevelTechPerformance",
            float minFactor = 0.02f,
            float maxFactor = 0.98f,
            float minSupportedFps = 1,
            float maxSupportedFps = DefaultMaxFps
        )
        {
            MinFactor = minFactor;
            MaxFactor = maxFactor;
            MinSupportedFps = minSupportedFps;
            MaxSupportedFps = maxSupportedFps;

            SendingMomentsSeconds = sendingMoments ?? new List<float> { 0, 1, 3, 5, 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            SampleCaptureFramesInterval = sampleCaptureFramesInterval;
            EventId = eventId;
        }
    }
}
