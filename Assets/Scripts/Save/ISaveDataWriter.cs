// 현재 런타임 상태를 전체 저장 데이터에 반영할 수 있는 객체
public interface ISaveDataWriter
{
    void WriteSaveData(GameSaveData saveData);
}