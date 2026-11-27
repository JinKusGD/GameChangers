using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour
{
    private const float FillSpeed = 0.5f;

    [SerializeField] private Slider _progressBar;

    private float _targetProgress;

    public bool IsFilled => _progressBar.value >= 1f;

    public void SetTarget(float progress)
    {
        float clamped = Mathf.Clamp01(progress);
        if (clamped <= _targetProgress) { return; }

        _targetProgress = clamped;

        _progressBar.DOKill();
        _progressBar.DOValue(_targetProgress, FillSpeed).SetSpeedBased().SetEase(Ease.Linear).SetUpdate(true).SetLink(gameObject);
    }

    public void ResetProgress()
    {
        _progressBar.DOKill();
        _targetProgress = 0f;
        _progressBar.value = 0f;
    }
}
