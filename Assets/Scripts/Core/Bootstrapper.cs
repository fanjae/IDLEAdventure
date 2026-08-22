using UnityEngine;

/// <summary>
/// 게임 실행 시 불러와야 할 것들 호출하는 클래스. <br/>
/// 보통 매니저들이 들어올 것으로 보인다. <br/>
/// 현재 추가된 목록 <br/>
/// SaveManager, CurrencyManager, InventoryManager, HeroManager, QuestManager
/// </summary>
public class Bootstrapper
{
    // 씬 실행 시 호출
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeWhenSceneStart()
    {
        Debug.Log("초기 생성 호출.");

        CurrencyManager currencyManager = CurrencyManager.Instance;

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
        heroManager.Initialize(heroDatabase, inventoryManager.Controller);

        // 공명 시스템 초기화
        ResonanceManager resonanceManager = ResonanceManager.Instance;
        resonanceManager.Initialize(heroManager.Controller);

        // 저장 데이터 초기화
        SaveManager saveManager = SaveManager.Instance;
        saveManager.Initialize();

        // 저장된 인벤토리와 장비 장착 상태 복원
        inventoryManager.Controller.LoadSaveData(saveManager.CurrentData);

        // 저장된 재화 상태 복원
        currencyManager.LoadSaveData(saveManager.CurrentData);

        // 저장된 보유 영웅 상태 복원
        heroManager.Controller.LoadSaveData(saveManager.CurrentData);

        // 가챠 데이터베이스가 없으면 개발 확인용 기본 배너를 사용함
        GachaDatabaseSO gachaDatabase = Resources.Load<GachaDatabaseSO>("GameData/GachaDatabase");
        if (gachaDatabase == null)
        {
            Debug.LogWarning("GachaDatabase 없음. 개발용 기본 배너 사용함.");
            gachaDatabase = GachaDatabaseSO.CreateDevelopmentDatabase(heroDatabase);
        }

        // 저장된 배너별 천장 진행도를 복원함
        GachaManager gachaManager = GachaManager.Instance;
        gachaManager.Initialize(gachaDatabase, heroDatabase);
        gachaManager.LoadSaveData(saveManager.CurrentData);

        AchievementDatabaseSO achievementDatabase = Resources.Load<AchievementDatabaseSO>("GameData/AchievementDatabase");
        if (achievementDatabase == null)
        {
            Debug.LogError("AchievementDatabaseSO를 불러오지 못했습니다. 업적 기능 비활성화함.");
        }
        else
        {
            try
            {
                // 업적 초기화 실패도 업적 기능만 비활성화하고 이후 초기화 계속함
                AchievementManager achievementManager = AchievementManager.Instance;
                achievementManager.Initialize(saveManager.CurrentData, achievementDatabase, gachaManager.Controller);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"업적 초기화 실패. 업적 기능 비활성화함. {exception.Message}");
            }
        }

        // 저장된 공명 상태 복원
        resonanceManager.Controller.LoadSaveData(saveManager.CurrentData);

        // 방치 시간 적용은 아직 되지 않았기에 추가
        TestSaveManager testSaveManager = TestSaveManager.Instance;

        // 퀘스트 매니저 추가
        QuestManager questManager = QuestManager.Instance;

        Debug.Log("초기 호출 완료.");
    }
}
