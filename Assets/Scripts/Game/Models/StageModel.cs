using System;
using System.ComponentModel;

public class StageModel : ObservableModel
{
    private static readonly PropertyChangedEventArgs ChapterChangedArgs = new PropertyChangedEventArgs(nameof(Chapter));
    private static readonly PropertyChangedEventArgs StageChangedArgs = new PropertyChangedEventArgs(nameof(Stage));
    private static readonly PropertyChangedEventArgs AliveBlockCountChangedArgs = new PropertyChangedEventArgs(nameof(AliveBlockCount));

    private int _chapter;
    private int _stage;
    private int _aliveBlockCount;

    public int Chapter
    {
        get => _chapter;
        private set => SetProperty(ref _chapter, value, ChapterChangedArgs);
    }

    public int Stage
    {
        get => _stage;
        private set => SetProperty(ref _stage, value, StageChangedArgs);
    }

    public int AliveBlockCount
    {
        get => _aliveBlockCount;
        set => SetProperty(ref _aliveBlockCount, value, AliveBlockCountChangedArgs);
    }

    public void Load(int chapter, int stage)
    {
        Chapter = chapter;
        Stage = stage;
    }

    public void ChangeStage(int chapter, int stage)
    {
        Chapter = Math.Max(1, chapter);
        Stage = Math.Max(1, stage);
    }
}
