using System;

// 가챠 컨트롤러를 전역에서 하나만 관리함
public sealed class GachaManager : Singleton<GachaManager>, ISaveDataWriter
{
    private GachaController controller;

    // 컨트롤러 생성 완료 여부 반환함
    public bool IsInitialized => controller != null;

    // 외부 시스템이 사용할 가챠 컨트롤러 반환함
    public GachaController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("GachaManager 초기화되지 않음");
            }

            return controller;
        }
    }

    // 가챠 데이터베이스를 기준으로 컨트롤러를 생성함
    public void Initialize(GachaDatabaseSO database, HeroDatabaseSO heroDatabase)
    {
        if (database == null)
        {
            throw new ArgumentNullException(nameof(database));
        }

        if (heroDatabase == null)
        {
            throw new ArgumentNullException(nameof(heroDatabase));
        }

        if (IsInitialized)
        {
            return;
        }

        controller = new GachaController(database, heroDatabase);

        // 가챠 천장 진행 상태를 저장 대상으로 등록
        SaveManager.Instance.RegisterWriter(this);
    }

    // 현재 천장 진행도를 저장 데이터에 반영함
    public void WriteSaveData(GameSaveData saveData)
    {
        if (!IsInitialized)
        {
            return;
        }

        controller.WriteSaveData(saveData);
    }

    // 저장된 천장 진행도를 컨트롤러에 복원함
    public void LoadSaveData(GameSaveData saveData)
    {
        if (!IsInitialized)
        {
            return;
        }

        controller.LoadSaveData(saveData);
    }

    protected override void OnDestroy()
    {
        if (SaveManager.TryGetExistingInstance(out SaveManager saveManager))
        {
            saveManager.UnregisterWriter(this);
        }

        base.OnDestroy();
    }
}
