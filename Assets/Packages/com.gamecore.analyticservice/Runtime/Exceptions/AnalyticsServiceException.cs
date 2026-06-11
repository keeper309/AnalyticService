using System;

namespace GameCore.AnalyticService
{
    public class AnalyticsServiceException : Exception
    {
        public AnalyticsServiceException()
        {

        }

        public AnalyticsServiceException(string message)
            : base(message)
        {

        }

        public AnalyticsServiceException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}