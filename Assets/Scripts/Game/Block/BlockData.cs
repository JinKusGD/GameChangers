using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BlockData : IValidatable
{
    public string Id { get; set; } = string.Empty;
    public int Key { get; set; }
    public double MaxHp { get; set; }
    public double Gold { get; set; }
    public string SpriteName { get; set; } = string.Empty;
    public List<BlockTraitData> Traits { get; set; } = new List<BlockTraitData>();

    private string _colorHex = "#FFFFFF";
    private bool _isColorValid = true;

    public string ColorHex
    {
        get { return _colorHex; }
        set
        {
            _colorHex = value;
            _isColorValid = ColorUtility.TryParseHtmlString(value, out Color color);

            if (_isColorValid)
            {
                Color = color;
            }
            else
            {
                Color = Color.white;
            }
        }
    }

    public Sprite Sprite { get; private set; }
    public Color Color { get; private set; } = Color.white;

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (Traits == null)
        {
            Traits = new List<BlockTraitData>();
        }
    }

    public async UniTask LoadSpriteAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(SpriteName)) { return; }

        string address = AddressKeys.BlockSpritePrefix + SpriteName;

        try
        {
            Sprite = await Addressables.LoadAssetAsync<Sprite>(address).ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Sprite = null;
            DebugUtility.LogWarning($"[BlockData] '{Id}': Addressable '{address}' 로드 실패. 기본 스프라이트를 씁니다. {exception.Message}");
            return;
        }

        if (Sprite == null)
        {
            DebugUtility.LogWarning($"[BlockData] '{Id}': Addressable '{address}' 스프라이트가 없어 기본 스프라이트를 씁니다.");
        }
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Id가 비어 있습니다.");
        }

        if (!BlockKey.IsValid(Key))
        {
            errors.Add($"Key는 1~{BlockKey.Max} 범위여야 합니다. ({Key})");
        }

        if (MaxHp <= 0)
        {
            errors.Add($"MaxHp는 0보다 커야 합니다. ({MaxHp})");
        }

        if (Gold < 0)
        {
            errors.Add($"Gold는 0 이상이어야 합니다. ({Gold})");
        }

        if (!_isColorValid)
        {
            errors.Add($"Color 형식이 잘못됐습니다. ({ColorHex})");
        }

        BlockTraitData.ValidateList(Traits, errors);
        ValidateNoDuplicateTypes(errors);

        return errors;
    }

    private void ValidateNoDuplicateTypes(List<string> errors)
    {
        HashSet<BlockTraitType> seenTypes = new HashSet<BlockTraitType>();

        for (int i = 0; i < Traits.Count; i++)
        {
            if (Traits[i] == null || seenTypes.Add(Traits[i].Type)) { continue; }

            errors.Add($"Traits[{i}] {Traits[i].Type} 특성이 이미 있습니다. 한 블록에는 같은 특성을 한 번만 넣으세요.");
        }
    }
}
