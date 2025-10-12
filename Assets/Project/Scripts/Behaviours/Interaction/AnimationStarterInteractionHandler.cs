using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class AnimationStarterInteractionHandler : MonoInteractableHandlerBase
    {
        public MonoScriptableAnimation Animation;

        public UnityEvent OnStart;
        public UnityEvent OnEnd;
        public UnityEvent OnCanceled;

        private ScriptableAnimationWithEvents _internalAnimation;
        private CancellationTokenSource _cts;
        private bool _isRunning = false;

        public override void HandleInteract(InteractionContext context)
        {
            if (_isRunning) return;

            if (Animation == null)
            {
                Debug.LogWarning("Animation not assigned");
                return;
            }

            _cts = new CancellationTokenSource();
            _internalAnimation = new ScriptableAnimationWithEvents(Animation);

            _internalAnimation.OnStart += () =>
            {
                _isRunning = true;
                OnStart?.Invoke();
            };

            _internalAnimation.OnEnd += () =>
            {
                _isRunning = false;
                OnEnd?.Invoke();
                Cleanup();
            };

            _internalAnimation.OnCanceled += () =>
            {
                _isRunning = false;
                OnCanceled?.Invoke();
                Cleanup();
            };

            RunAnimationAsync(_cts.Token).Forget();
        }

        private async UniTask RunAnimationAsync(CancellationToken token)
        {
            try
            {
                await _internalAnimation.Run(token);
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void Cleanup()
        {
            _cts?.Dispose();
            _cts = null;
            _internalAnimation = null;
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
