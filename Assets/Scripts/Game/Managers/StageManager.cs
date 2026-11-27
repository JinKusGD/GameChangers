using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private const string Tag = "StageManager";
    private const float NextStageDelay = 1f;

    [SerializeField] private BlockField _blockField;

    public static StageManager Instance { get; private set; }

    public int AliveCount => _blockField.AliveCount;
    public int GridSize => _blockField.GridSize;
    public int ChangeVersion => _blockField.ChangeVersion;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DebugUtility.LogError($"[{Tag}] 씬에 StageManager가 두 개 이상 있습니다.", this);
            Destroy(this);
            return;
        }

        if (!UnityUtility.CheckReference(_blockField, nameof(_blockField), this))
        {
            enabled = false;
            return;
        }

        Instance = this;
        _blockField.AllBlocksBroken += OnAllBlocksBroken;
        _blockField.BlocksChanged += OnBlocksChanged;
        OnBlocksChanged();
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
    }

    private void OnAllBlocksBroken()
    {
        GoToNextStageAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid GoToNextStageAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(NextStageDelay), cancellationToken: token);

        GoToNextStage();
    }

    private void GoToNextStage()
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null) { return; }

        StageModel stageModel = ModelManager.Instance.StageModel;
        (int chapter, int stage) = GetNextStage(stageModel.Chapter, stageModel.Stage);

        gameManager.ChangeStage(chapter, stage);
    }

    private static (int Chapter, int Stage) GetNextStage(int chapter, int stage)
    {
        DataManager data = DataManager.Instance;

        if (data.Stages.TryGet(stage + 1, out _)) { return (chapter, stage + 1); }

        return (chapter + 1, 1);
    }
}
