using System;
using GameCore.Analytics;
using GameCore.LoggerService;

namespace GameCore.AnalyticService
{
    public static class DiskUtilsSafe
    {
        public static int CheckAvailableSpace(ILogger logger)
        {
            try
            {
                int space = DiskUtils.CheckAvailableSpace();

                return space;
            }
            catch (Exception e)
            {
                logger.Message(e.Message);

                return 0;
            }
        }

        public static int CheckTotalSpace(ILogger logger)
        {
            try
            {
                int space = DiskUtils.CheckTotalSpace();

                return space;
            }
            catch (Exception e)
            {
                logger.Message(e.Message);

                return 0;
            }
        }

        public static int CheckBusySpace(ILogger logger)
        {
            try
            {
                int space = DiskUtils.CheckBusySpace();

                return space;
            }
            catch (Exception e)
            {
                logger.Message(e.Message);

                return 0;
            }
        }
    }
}
