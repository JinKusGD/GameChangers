using System.Collections.Generic;
using UnityEngine;

public class StageTable
{
    public void Load()
    {
        // TODO: 스테이지 정보 테이블 로딩
    }

    public StageInfo Get(int chapter, int stage)
    {
        // TODO: 스테이지 정보 테이블에서 (chapter, stage) 정보를 받아오기
        return CreateDefaultStageInfo();
    }

    private static StageInfo CreateDefaultStageInfo()
    {
        StageInfo stageInfo = new StageInfo();
        stageInfo.LayoutId = "heart";
        stageInfo.FieldPosition = Vector2.zero;
        stageInfo.BlockIdsByKey = new Dictionary<int, string>
        {
            { 1, "gray" },
            { 2, "orange" },
            { 3, "yellow" },
            { 4, "skyblue" },
            { 5, "purple" },
            { 6, "green" },
            { 7, "pink" },
            { 8, "blue" },
            { 9, "black" },
        };

        return stageInfo;
    }
}
