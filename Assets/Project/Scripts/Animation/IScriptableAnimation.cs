using System.Threading;
using Cysharp.Threading.Tasks;

public interface IScriptableAnimation
{
    float Duration { get; }
    UniTask Run(CancellationToken token = default);
}
