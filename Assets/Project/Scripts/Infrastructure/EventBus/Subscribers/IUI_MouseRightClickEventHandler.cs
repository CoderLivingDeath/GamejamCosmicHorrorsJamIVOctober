namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_MouseRightClickEventHandler : IGlobalSubscriber
    {
        void HandleMouseRightClick(bool button);
    }
}