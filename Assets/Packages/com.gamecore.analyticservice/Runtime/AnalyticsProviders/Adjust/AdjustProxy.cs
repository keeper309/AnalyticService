#if ANALYTICS_ADJUST_EXISTS

using System;
using AdjustSdk;
using GameCore.GeneralExtensions;

namespace GameCore.AnalyticService
{
    public static class AdjustProxy
    {
        public static async Cysharp.Threading.Tasks.UniTask<int> RequestAppTrackingAuthorization()
        {
            return ApplicationHelper.IsIos(true)
                ? await UniTaskExtensions.AsUniTaskAsync<int>(true, Adjust.RequestAppTrackingAuthorization)
                : -1;
        }

        public static async Cysharp.Threading.Tasks.UniTask<string> GetAdid()
        {
            return ApplicationHelper.IsAndroidOrIos(true)
                ? await UniTaskExtensions.AsUniTaskAsync<string>(true, Adjust.GetAdid) ?? string.Empty
                : String.Empty;
        }

        public static async Cysharp.Threading.Tasks.UniTask<string> GetGoogleAdId()
        {
            return ApplicationHelper.IsAndroid(true)
                ? await UniTaskExtensions.AsUniTaskAsync<string>(true, Adjust.GetGoogleAdId) ?? string.Empty
                : String.Empty;
        }

        public static async Cysharp.Threading.Tasks.UniTask<string> GetIdfa()
        {
            return ApplicationHelper.IsIos(true)
                ? await UniTaskExtensions.AsUniTaskAsync<string>(true, Adjust.GetIdfa) ?? string.Empty
                : String.Empty;
        }

        public static async Cysharp.Threading.Tasks.UniTask<AdjustAttribution> GetAttribution()
        {
            return ApplicationHelper.IsAndroidOrIos(true)
                ? await UniTaskExtensions.AsUniTaskAsync<AdjustAttribution>(true, Adjust.GetAttribution) ?? new AdjustAttribution()
                : new AdjustAttribution();
        }

        public static async Cysharp.Threading.Tasks.UniTask<bool> IsEnabled()
        {
            return ApplicationHelper.IsAndroidOrIos(true) &&
                await UniTaskExtensions.AsUniTaskAsync<bool>(true, Adjust.IsEnabled);
        }

        public static async Cysharp.Threading.Tasks.UniTask<string> GetSdkVersion()
        {
            return ApplicationHelper.IsAndroidOrIos(true)
                ? await UniTaskExtensions.AsUniTaskAsync<string>(true, Adjust.GetSdkVersion) ?? string.Empty
                : string.Empty;
        }

        public static void TrackEvent(AdjustEvent adjustEvent)
        {
            if (ApplicationHelper.IsAndroidOrIos(true))
                Adjust.TrackEvent(adjustEvent);
        }

        public static void SwitchToOfflineMode()
        {
            if (ApplicationHelper.IsAndroidOrIos(true))
                Adjust.SwitchToOfflineMode();
        }

        public static void SwitchBackToOnlineMode()
        {
            if (ApplicationHelper.IsAndroidOrIos(true))
                Adjust.SwitchBackToOnlineMode();
        }

        public static void AddGlobalCallbackParameter(string key, string value)
        {
            if (ApplicationHelper.IsAndroidOrIos(true))
                Adjust.AddGlobalCallbackParameter(key, value);
        }

        public static void InitSdk(AdjustConfig adjustConfig)
        {
            if (ApplicationHelper.IsAndroidOrIos(true))
                Adjust.InitSdk(adjustConfig);
        }
    }
}

#endif