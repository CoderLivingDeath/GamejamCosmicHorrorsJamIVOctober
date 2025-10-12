using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class TransitionAnimation : IScriptableAnimation
{
    public IScriptableAnimation DoorOpenAnimation;
    public IScriptableAnimation DoorCloseAnimation;
    public IScriptableAnimation CharacterMoveAnimation;

    public TransitionAnimation(IScriptableAnimation doorOpenAnimation, IScriptableAnimation doorCloseAnimation, IScriptableAnimation characterMoveAnimation)
    {
        DoorOpenAnimation = doorOpenAnimation;
        DoorCloseAnimation = doorCloseAnimation;
        CharacterMoveAnimation = characterMoveAnimation;
    }

    public float Duration => CharacterMoveAnimation.Duration + DoorOpenAnimation.Duration + DoorCloseAnimation.Duration;

    public async UniTask Run(CancellationToken token = default)
    {
        await DoorOpenAnimation.Run(token);
        await CharacterMoveAnimation.Run(token);
        await DoorCloseAnimation.Run(token);
    }
}
