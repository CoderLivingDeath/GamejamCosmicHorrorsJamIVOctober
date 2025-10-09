namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_LeftEventHandler : IGlobalSubscriber
    {
        void HandleLeft(bool button);
    }
}