using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private const string Tag = "StageManager";

    [SerializeField] private BlockField _blockField;
    [SerializeField] private InfiniteBackground _background;

    public static StageManager Instance { get; private set; }

    public int AliveCount => _blockField.AliveCount;
    public int GridSize => _blockField.GridSize;

    public event Action BlocksChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DebugUtility.LogError($"[{Tag}] 씬에 StageManager가 두 개 이상 있습니다.", this);
            Destroy(this);
            return;
        }

        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_blockField, nameof(_blockField), this);
        hasAllReferences &= UnityUtility.CheckReference(_background, nameof(_background), this);

        if (!hasAllReferences)
        {
            enabled = false;
            return;
        }

        Instance = this;
        _blockField.AllBlocksBroken += OnAllBlocksBroken;
        _blockField.BlocksChanged += OnBlocksChanged;
        OnBlocksChanged();
    }

    private void Start()
    {
        StartStageAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void OnDestroy()
    {
        if (Instance != this) { return; }

        _blockField.AllBlocksBroken -= OnAllBlocksBroken;
        _blockField.BlocksChanged -= OnBlocksChanged;
        Instance = null;
    }

    public Block FindNearest(Vector2 position, float maxDistance = float.PositiveInfinity)
    {
        return _blockField.FindNearest(position, maxDistance);
    }

    public void Damage(Block target, double damage)
    {
        _blockField.Damage(target, damage);
    }

    public bool TryGetCellColor(int column, int row, out Color color)
    {
        return _blockField.TryGetCellColor(column, row, out color);
    }

    public Vector2 GetNormalizedPosition(Vector2 worldPosition)
    {
        return _blockField.GetNormalizedPosition(worldPosition);
    }

    private void OnBlocksChanged()
    {
        ModelManager.Instance.StageModel.AliveBlockCount = _blockField.AliveCount;
        BlocksChanged?.Invoke();
    }

    private void OnAllBlocksBroken()
    {
        GoToNextStageAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid StartStageAsync(CancellationToken token)
    {
        UIManager uiManager = UIManager.Instance;
        ScreenFader fader = uiManager != null ? await uiManager.ShowAsync<ScreenFader>() : null;

        if (fader != null)
        {
            fader.SetBlack();
        }

        await _background.ApplyStageAsync(ModelManager.Instance.StageModel.Stage, token);

        if (fader != null)
        {
            await fader.FadeOutAsync(token);
        }
    }

    private async UniTaskVoid GoToNextStageAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(DataManager.Instance.Config.Stage.NextStageDelay), cancellationToken: token);

        GameManager gameManager = GameManager.Instance;

        if (gameManager == null) { return; }

        StageModel stageModel = ModelManager.Instance.StageModel;
        int clearedChapter = stageModel.Chapter;
        (int chapter, int stage) = GetNextStage(clearedChapter, stageModel.Stage);

        UIManager uiManager = UIManager.Instance;
        ScreenFader fader = uiManager != null ? await uiManager.ShowAsync<ScreenFader>() : null;

        if (fader != null)
        {
            await fader.FadeInAsync(token);
        }

        gameManager.ChangeStage(chapter, stage);
        await _background.ApplyStageAsync(stage, token);

        ChapterClearPopup popup = null;

        if (chapter != clearedChapter && uiManager != null)
        {
            popup = await uiManager.ShowAsync<ChapterClearPopup>();

            if (popup != null)
            {
                popup.Setup(clearedChapter, chapter, DataManager.Instance.Chapters.LastChapter);
            }
        }

        if (fader != null)
        {
            await fader.FadeOutAsync(token);
        }

        if (popup == null) { return; }

        await popup.WaitForCloseAsync(token);

        uiManager.Hide<ChapterClearPopup>();
    }

    private static (int Chapter, int Stage) GetNextStage(int chapter, int stage)
    {
        DataManager data = DataManager.Instance;

        if (data.Stages.TryGet(stage + 1, out _)) { return (chapter, stage + 1); }

        return (chapter + 1, 1);
    }
}
