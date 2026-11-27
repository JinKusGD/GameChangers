using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour, IProgress<float>
{
    private const float SceneLoadedProgress = 0.9f;
    private const float DataWeight = 0.4f;
    private const float SaveWeight = 0.1f;
    private const float UIWeight = 0.1f;
    private const float SceneWeight = 0.4f;

    [SerializeField]private LoadingView _loadingView;

    private float _totalWeight;
    private float _completedWeight;

    private void Start()
    {
        BootAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask BootAsync(CancellationToken token)
    {
        if (GameManager.Instance == null)
        {
            DebugUtility.LogError("[BootLoader] GameManager가 없습니다.", this);
            return;
        }

        _totalWeight = DataWeight + SaveWeight + UIWeight + SceneWeight;
        _completedWeight = 0f;

        _loadingView.ResetProgress();

        await DataManager.Instance.LoadAsync(this, token);
        CompleteStage(DataWeight);

        GameManager.Instance.LoadSaveData();
        CompleteStage(SaveWeight);

        await PreloadUIAsync();
        CompleteStage(UIWeight);

        await LoadGameSceneAsync(token);
    }

    private async UniTask LoadGameSceneAsync(CancellationToken token)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(SceneNames.Game);
        loadOperation.allowSceneActivation = false;

        float loadRatio = 0f;
        while (loadRatio < 1f || !_loadingView.IsFilled)
        {
            loadRatio = Mathf.Clamp01(loadOperation.progress / SceneLoadedProgress);
            ReportStage(SceneWeight, loadRatio);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        loadOperation.allowSceneActivation = true;
    }

    private async UniTask PreloadUIAsync()
    {
        UIManager uiManager = UIManager.Instance;

        if (uiManager == null)
        {
            DebugUtility.LogError("[BootLoader] UIManager가 없어 UI를 미리 생성하지 않습니다.", this);
            return;
        }

        await uiManager.PreloadAsync<HudView>();
    }

    public void Report(float dataRatio)
    {
        ReportStage(DataWeight, dataRatio);
    }

    private void CompleteStage(float stageWeight)
    {
        _completedWeight += stageWeight;
        ReportStage(0f, 0f);
    }

    private void ReportStage(float stageWeight, float stageRatio)
    {
        _loadingView.SetTarget((_completedWeight + stageWeight * stageRatio) / _totalWeight);
    }
}
