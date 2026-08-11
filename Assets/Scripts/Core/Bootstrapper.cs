using UnityEngine;

/// <summary>
/// 게임 실행 시 불러와야 할 것들 호출하는 클래스. <br/>
/// 보통 매니저들이 들어올 것으로 보인다. <br/>
/// 현재 추가된 목록 <br/>
/// TestSaveManager, CurrencyManager, InventoryManager, HeroManager
/// </summary>
public class Bootstrapper
{
    // 씬 실행 시 호출
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeWhenSceneStart()
    {
        Debug.Log("초기 생성 호출.");

        TestSaveManager testSaveManager = TestSaveManager.Instance;
        CurrencyManager currency = CurrencyManager.Instance;

        ItemDatabaseSO itemDatabase = Resources.Load<ItemDatabaseSO>("GameData/ItemDatabase");

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabaseSO를 불러오지 못했습니다.");
            return;
        }

        // 인벤토리 시스템 초기화
        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.Initialize(itemDatabase);

        HeroDatabaseSO heroDatabase = Resources.Load<HeroDatabaseSO>("GameData/HeroDatabase");

        if (heroDatabase == null)
        {
            Debug.LogError("HeroDatabaseSO를 불러오지 못했습니다.");
            return;
        }

        HeroManager heroManager = HeroManager.Instance;
        heroManager.Initialize(heroDatabase);
      
        // 저장 데이터 초기화 (테스트 세이브 매니저 아직 미삭제)
        SaveManager saveManager = SaveManager.Instance;
        saveManager.Initialize();

        // 저장된 인벤토리와 장비 장착 상태 복원
        inventoryManager.Controller.LoadSaveData(saveManager.CurrentData);
      
        Debug.Log("초기 호출 완료.");
    }
}