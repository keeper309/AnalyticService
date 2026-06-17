using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    [Parameters(
        new[]
        {
            "gpu_name_at_device",
            "cpu_name_at_device",
            "total_ram_at_device",
            "total_gpu_ram_at_device",
            "device_name", "device_model", "device_os_name", "device_os_version", "device_gpu", "device_cpu",
            "device_total_ram", "device_total_gpu_ram", "device_storage_available", "game_quality_settings"
        }
    )]
    public class DeviceInfoProvider : IAnalyticsParametersProvider
    {
        private readonly ILogger _logger;

        private static string _deviceOsName;
        private static string _graphicsDeviceName;
        private static string _processorType;
        private static int _systemMemorySize;
        private static int _graphicsMemorySize;
        private static string _deviceName;
        private static string _deviceModel;
        private static string _operatingSystem;
        private static string _qualitySettings;

        private readonly Dictionary<string, Func<object>> _parameters;
        private SynchronizationContext _synchronizationContext;

        public IReadOnlyCollection<string> ParametersId => _parameters.Keys;

        public DeviceInfoProvider(ILogger logger)
        {
            _logger = logger;

            _parameters = new()
            {
                { "gpu_name_at_device", () => _graphicsDeviceName },
                { "cpu_name_at_device", () => _processorType },
                { "total_ram_at_device", () => _systemMemorySize },
                { "total_gpu_ram_at_device", () => _graphicsMemorySize },
                { "device_name", () => _deviceName },
                { "device_model", () => _deviceModel },
                { "device_os_name", () => _deviceOsName },
                { "device_os_version", () => _operatingSystem },
                { "device_gpu", () => _graphicsDeviceName },
                { "device_cpu", () => _processorType },
                { "device_total_ram", () => _systemMemorySize },
                { "device_total_gpu_ram", () => _graphicsMemorySize },
                { "device_storage_available", () => DiskUtilsSafe.CheckAvailableSpace( _logger ) },
                { "game_quality_settings", () => _qualitySettings }
            };
        }

        public object GetValue(string key)
        {
            if (SynchronizationContext.Current == _synchronizationContext) { }

            if (string.IsNullOrEmpty(key))
                throw new AnalyticsServiceException($"Parameters provider {GetType().Name}, key is null or empty");

            if (_parameters.TryGetValue(key, out Func<object> getter))
                return getter.Invoke();

            throw new AnalyticsServiceException($"Parameters provider {GetType().Name}, not contains value for key: {key}");
        }

        public void SetValue(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new AnalyticsServiceException($"Parameters provider {GetType().Name}, key is null or empty");

            if (!_parameters.ContainsKey(key))
                throw new AnalyticsServiceException($"Parameters provider {GetType().Name}, not contains value for key: {key}");

            _parameters[key] = () => value;
        }

        public void Initialize()
        {
            _synchronizationContext ??= SynchronizationContext.Current;
            _deviceOsName = SystemInfo.operatingSystemFamily.ToString();
            _graphicsDeviceName = SystemInfo.graphicsDeviceName;
            _processorType = SystemInfo.processorType;
            _deviceName = SystemInfo.deviceName;
            _deviceModel = SystemInfo.deviceModel;
            _operatingSystem = SystemInfo.operatingSystem;
            _graphicsDeviceName = SystemInfo.graphicsDeviceName;
            _processorType = SystemInfo.processorType;
            _systemMemorySize = SystemInfo.systemMemorySize;
            _qualitySettings = QualitySettings.names[QualitySettings.GetQualityLevel()];
        }
    }
}
