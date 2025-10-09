namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_DownEventHandler : IGlobalSubscriber
    {
        void HandleDown(bool button);
    }
}