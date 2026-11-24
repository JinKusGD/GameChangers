using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class StageTemplateRepository
{
    private const string StageDataPath = "Stages/StageTemplates";

    private static Dictionary<string, StageTemplate> templates;

    public static StageTemplate GetById(string id)
    {
        EnsureLoaded();

        if (id == null || !templates.TryGetValue(id, out StageTemplate template))
        {
            Debug.LogError($"[StageTemplate] Id '{id}' 템플릿이 없습니다.");
            return null;
        }

        return template;
    }

    public static void Reload()
    {
        templates = null;

        EnsureLoaded();
    }

    static void EnsureLoaded()
    {
        if (templates != null) { return; }

        templates = new Dictionary<string, StageTemplate>();

        TextAsset asset = Resources.Load<TextAsset>(StageDataPath);

        if (asset == null)
        {
            Debug.LogError($"[StageTemplate] Resources/{StageDataPath}.json 을 찾을 수 없습니다.");
            return;
        }

        List<StageTemplate> stageTemplateList;

        try
        {
            stageTemplateList = JsonConvert.DeserializeObject<List<StageTemplate>>(asset.text);
        }
        catch (JsonException e)
        {
            Debug.LogError($"[StageTemplate] JSON 읽기 실패: {e.Message}");
            return;
        }

        if (stageTemplateList == null)
        {
            Debug.LogError("[StageTemplate] JSON이 비어 있습니다.");
            return;
        }

        foreach (StageTemplate stageTemplate in stageTemplateList)
        {
            if (stageTemplate == null) { continue; }

            List<string> errors = stageTemplate.Validate();

            if (errors.Count > 0)
            {
                Debug.LogError($"[StageTemplate] '{stageTemplate.Id}' 제외됨: {string.Join(", ", errors)}");
                continue;
            }

            if (!templates.TryAdd(stageTemplate.Id, stageTemplate))
            {
                Debug.LogError($"[StageTemplate] '{stageTemplate.Id}' 제외됨: Id 중복");
            }
        }
    }
}
