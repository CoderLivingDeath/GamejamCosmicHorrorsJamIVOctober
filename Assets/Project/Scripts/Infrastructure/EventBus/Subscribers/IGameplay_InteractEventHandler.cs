
namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IGameplay_InteractEventHandler : IGlobalSubscriber
    {
        void HandleInteract(bool button);
    }
}