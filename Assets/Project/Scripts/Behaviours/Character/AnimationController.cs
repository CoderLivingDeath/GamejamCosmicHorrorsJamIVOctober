using System.Collections.Generic;
using UnityEngine;
// -----------------------------------------------
#region Animation Controller
public class AnimationController
{
    private ScriptableAnimationScope _currentScope;

    private readonly Dictionary<string, IScriptableAnimation> _animationLibrary;

    public bool IsAnimating => _currentScope != null;

    public Animator Animator
    {
        get { return animator; }
        set { animator = value; }
    }

    private Animator animator;

    public AnimationController(Dictionary<string, IScriptableAnimation> animationLibrary, Animator animator)
    {
        _animationLibrary = animationLibrary;
        this.animator = animator;

        ValidateAnimator(animator);
    }

    public void ValidateAnimator(Animator animator)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null.");
            return;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;

        string[] requiredTriggers = { "Idle", "Walk", "Run" };
        foreach (string triggerName in requiredTriggers)
        {
            bool triggerFound = false;
            foreach (var param in parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                {
                    triggerFound = true;
                    break;
                }
            }
            if (!triggerFound)
            {
                Debug.LogWarning($"Animator is missing trigger parameter: '{triggerName}'");
            }
        }
    }

    public ScriptableAnimationScope PlayAnimation(string key, bool @override = false)
    {
        if (_animationLibrary.TryGetValue(key, out var animation))
        {
            var scope = PlayAnimation(animation, @override);
            return scope;
        }
        else
        {
            Debug.LogWarning($"Animation with key '{key}' not found.");
            return null;
        }
    }

    public ScriptableAnimationScope PlayAnimation(IScriptableAnimation animation, bool @override = false)
    {
        if (@override)
        {
            // При override отменяем текущий скоуп и создаём новый
            ClearAnimationScope();

            _currentScope = new ScriptableAnimationScope(animation);
            _currentScope.Completed += ClearAnimationScope;
        }
        else
        {
            if (IsAnimating)
            {
                Debug.LogWarning("Animation is already running");
                return null;
            }
            _currentScope = new ScriptableAnimationScope(animation);
            _currentScope.Completed += ClearAnimationScope;
        }

        return _currentScope;
    }

    private void ClearAnimationScope()
    {
        _currentScope?.Dispose();
        _currentScope = null;
    }

    public void FireAnimationTrigger(string trigger)
    {
        if (IsAnimating) return;

        if (animator == null)
        {
            Debug.LogWarning("Animator is null. Cannot set trigger.");
            return;
        }

        if (string.IsNullOrEmpty(trigger))
        {
            Debug.LogWarning("Trigger string is null or empty.");
            return;
        }

        animator.SetTrigger(trigger);
    }


    public void CancelCurrentAnimation()
    {
        _currentScope?.Cancel();
    }
}

#endregion
#region Movement Controller

#endregion
#region  Interaction Controller
#endregion
