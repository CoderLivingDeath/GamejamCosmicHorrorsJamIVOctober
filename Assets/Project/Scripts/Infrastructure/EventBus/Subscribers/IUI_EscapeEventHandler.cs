namespace GameJamLvl5.Project.Infrastructure.EventBus
{
    public interface IUI_EscapeEventHandler : IGlobalSubscriber
    {
        void HandleEscape(bool button);
    }
}