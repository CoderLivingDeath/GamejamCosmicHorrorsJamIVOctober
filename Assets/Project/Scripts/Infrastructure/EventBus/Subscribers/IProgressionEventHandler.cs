namespace GameJamLvl5.Project.Infrastructure.EventBus
{
    public interface IProgressionEventHandler : IGlobalSubscriber
    {
        void HandleProgressionEvent(string key);
    }
}