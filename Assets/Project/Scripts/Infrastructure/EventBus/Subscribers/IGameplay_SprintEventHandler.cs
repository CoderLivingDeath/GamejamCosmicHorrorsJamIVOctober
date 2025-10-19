
namespace GameJam.Project.Infrastructure.EventBus.Subscribers
{
    public interface IGameplay_SprintEventHandler : IGlobalSubscriber
    {
        void HandleSprint(bool button);
    }
}