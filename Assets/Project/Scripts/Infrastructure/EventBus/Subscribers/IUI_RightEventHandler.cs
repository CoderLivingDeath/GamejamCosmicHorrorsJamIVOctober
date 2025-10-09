namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_RightEventHandler : IGlobalSubscriber
    {
        void HandleRight(bool button);
    }
}