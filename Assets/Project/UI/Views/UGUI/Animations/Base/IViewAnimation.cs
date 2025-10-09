using Cysharp.Threading.Tasks;
using System.Threading;

public interface IViewAnimation
{
    MonoCanvasView View { get; }
    float Duration { get; set; }
    UniTask AnimateAsync(CancellationToken token = default);
}
