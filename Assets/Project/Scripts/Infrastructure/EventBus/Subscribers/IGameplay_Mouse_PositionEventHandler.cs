using UnityEngine;

namespace GameJam.Project.Infrastructure.EventBus.Subscribers
{
    public interface IGameplay_Mouse_PositionEventHandler : IGlobalSubscriber
    {
        void HandleMousePosition(Vector2 position);
    }
}