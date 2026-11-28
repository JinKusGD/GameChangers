using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : UIView
{
    private const float MaxFrameDelta = 1f / 30f;

    [SerializeField, Min(0f)] private float _fadeInSeconds = 0.3f;
    [SerializeField, Min(0f)] private float _fadeOutSeconds = 0.3f;

    private CanvasGroup _canvasGroup;

    public UniTask FadeInAsync(CancellationToken token)
    {
        Show();

        return FadeAsync(1f, _fadeInSeconds, token);
    }

    public void SetBlack()
    {
        Show();
        _canvasGroup.alpha = 1f;
    }

    public async UniTask FadeOutAsync(CancellationToken token)
    {
        await FadeAsync(0f, _fadeOutSeconds, token);

        Hide();
    }

    protected override bool OnCreate()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;

        return true;
    }

    private async UniTask FadeAsync(float targetAlpha, float duration, CancellationToken token)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFrameDelta);    
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
        }

        _canvasGroup.alpha = targetAlpha;
    }
}
