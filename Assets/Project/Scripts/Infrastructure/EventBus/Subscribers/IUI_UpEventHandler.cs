namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_UpEventHandler : IGlobalSubscriber
    {
        void HandleUp(bool button);
    }
}