namespace GameCore.AnalyticService
{
    internal interface IRuntimeEventProfile : IEventProfile
    {
        bool CanSend(IEvent @event);
    }
}
