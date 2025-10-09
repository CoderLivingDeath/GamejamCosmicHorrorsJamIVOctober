using UnityEngine;

[CreateAssetMenu(fileName = "AnimationData", menuName = "Scriptable Objects/AnimationData")]
public abstract class ViewAnimationSO<T> : ScriptableObject, IViewAnimationProvider<T> where T : MonoCanvasView
{
    public abstract ViewAnimation<T> ProvideAnimation(T view);
}
