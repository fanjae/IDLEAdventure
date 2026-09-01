using System;
using UnityEngine;

// 필드 플레이어 위치의 저장 및 복원을 관리하는 클래스
public sealed class FieldPlayerPositionController : MonoBehaviour
{
    public static FieldPlayerPositionController Current { get; private set; }

    // 현재 플레이어 위치를 저장 데이터에 반영
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.PlayerPosition ??= new PlayerPositionSaveData();

        Vector3 position = transform.position;

        saveData.PlayerPosition.HasPosition = true;
        saveData.PlayerPosition.X = position.x;
        saveData.PlayerPosition.Y = position.y;
        saveData.PlayerPosition.Z = position.z;
    }

    // 저장된 위치를 기준으로 플레이어 위치 복원
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        saveData.PlayerPosition ??= new PlayerPositionSaveData();

        // 저장된 위치가 없는 경우 현재 씬의 기본 위치 사용
        if (!saveData.PlayerPosition.HasPosition)
        {
            return;
        }

        transform.position = new Vector3(saveData.PlayerPosition.X,saveData.PlayerPosition.Y,saveData.PlayerPosition.Z);
    }

    private void Awake()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }
}