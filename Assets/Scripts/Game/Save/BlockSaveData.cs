using System;

[Serializable]
public struct BlockSaveData
{
    public int CellIndex;
    public double Hp;
    public int SplitBlockKey;
    public int? ShieldHits;
}
