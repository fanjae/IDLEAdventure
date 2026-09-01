using System;

// 필드에서 플레이어가 마지막으로 위치한 좌표 저장
[Serializable]
public sealed class PlayerPositionSaveData
{
    // 저장된 위치가 존재하는지 확인
    public bool HasPosition { get; set; }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}