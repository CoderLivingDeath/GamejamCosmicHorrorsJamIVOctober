namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_MouseLeftClickEventHandler : IGlobalSubscriber
    {
        void HandleMouseLeftClick(bool button);
    }
}