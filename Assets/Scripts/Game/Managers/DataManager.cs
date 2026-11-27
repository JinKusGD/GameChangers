using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class DataManager
{
    private static DataManager _instance;

    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataManager();
            }

            return _instance;
        }
    }

    public BlockTable Blocks { get; } = new BlockTable();
    public BlockLayoutTable BlockLayouts { get; } = new BlockLayoutTable();
    public ChapterTable Chapters { get; } = new ChapterTable();
    public StageTable Stages { get; } = new StageTable();

    private const string Tag = "DataManager";

    private DataManager() { }

    public async UniTask LoadAsync(IProgress<float> progress, CancellationToken token)
    {
        (string Name, UniTask Task)[] loads =
        {
            (nameof(Blocks), Blocks.LoadAsync(token)),
            (nameof(BlockLayouts), BlockLayouts.LoadAsync(token)),
            (nameof(Chapters), Chapters.LoadAsync(token)),
            (nameof(Stages), Stages.LoadAsync(token)),
        };

        int loadedCount = 0;

        async UniTask TrackAsync((string Name, UniTask Task) load)
        {
            await LoadSafelyAsync(load.Name, load.Task);
            loadedCount++;
            progress?.Report((float)loadedCount / loads.Length);
        }

        UniTask[] trackedLoads = new UniTask[loads.Length];

        for (int i = 0; i < loads.Length; i++)
        {
            trackedLoads[i] = TrackAsync(loads[i]);
        }

        await UniTask.WhenAll(trackedLoads);

        CheckReferences();
    }

    public StageInfo GetStageInfo(int chapter, int stage)
    {
        StageData stageData = Stages.Get(stage);

        if (stageData == null) { return null; }

        int lastChapter = Chapters.LastChapter;
        int sourceChapter = lastChapter > 0 ? Math.Min(chapter, lastChapter) : chapter;
        ChapterData chapterData = Chapters.Get(sourceChapter);

        if (chapterData == null) { return null; }

        int overflowChapters = Math.Max(0, chapter - sourceChapter);

        return StageInfo.Create(stageData, chapterData, overflowChapters);
    }

    private static async UniTask LoadSafelyAsync(string tableName, UniTask loadTask)
    {
        try
        {
            await loadTask;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[{Tag}] {tableName} 로드 중 예외: {e}");
        }
    }

    private void CheckReferences()
    {
        Chapters.CheckBlockIds(Blocks);
        Stages.CheckLayoutIds(BlockLayouts);
        BlockLayouts.CheckBlockKeys(Blocks);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }
}
