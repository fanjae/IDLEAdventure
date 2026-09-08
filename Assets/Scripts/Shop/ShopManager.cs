using System;

// 상점 컨트롤러를 전역에서 하나만 관리함
public sealed class ShopManager : Singleton<ShopManager>, ISaveDataWriter
{
    private ShopController controller;

    public bool IsInitialized => controller != null;

    public ShopController Controller
    {
        get
        {
            if (controller == null)
            {
                throw new InvalidOperationException("ShopManager 초기화되지 않음");
            }

            return controller;
        }
    }

    // 상점 데이터베이스를 기준으로 컨트롤러를 생성함
    public void Initialize(ShopDatabaseSO database)
    {
        if (database == null)
        {
            throw new ArgumentNullException(nameof(database));
        }

        if (IsInitialized)
        {
            return;
        }

        if (!database.TryValidate(out string validationError))
        {
            throw new InvalidOperationException($"ShopDatabase 설정 오류: {validationError}");
        }

        controller = new ShopController(database);

        // 상점 상태를 저장 대상으로 등록
        SaveManager.Instance.RegisterWriter(this);
    }

    // 저장 데이터를 상점 컨트롤러에 복원함
    public void LoadSaveData(GameSaveData saveData)
    {
        if (IsInitialized)
        {
            controller.LoadSaveData(saveData);
        }
    }

    // 현재 상점 상태를 저장 데이터에 기록함
    public void WriteSaveData(GameSaveData saveData)
    {
        if (IsInitialized)
        {
            controller.WriteSaveData(saveData);
        }
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
