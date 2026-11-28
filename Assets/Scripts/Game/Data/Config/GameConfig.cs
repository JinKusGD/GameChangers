using System.Collections.Generic;
using System.Runtime.Serialization;

public class GameConfig : IValidatable
{
    public OfflineConfig Offline { get; set; } = new OfflineConfig();
    public StageConfig Stage { get; set; } = new StageConfig();
    public BlockConfig Block { get; set; } = new BlockConfig();
    public SkillUnlockConfig SkillUnlock { get; set; } = new SkillUnlockConfig();

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        Offline ??= new OfflineConfig();
        Stage ??= new StageConfig();
        Block ??= new BlockConfig();
        SkillUnlock ??= new SkillUnlockConfig();
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        AddSectionErrors(errors, nameof(Offline), Offline.Validate());
        AddSectionErrors(errors, nameof(Stage), Stage.Validate());
        AddSectionErrors(errors, nameof(Block), Block.Validate());
        AddSectionErrors(errors, nameof(SkillUnlock), SkillUnlock.Validate());

        return errors;
    }

    private static void AddSectionErrors(List<string> errors, string section, List<string> sectionErrors)
    {
        foreach (string error in sectionErrors)
        {
            errors.Add($"{section}.{error}");
        }
    }
}
