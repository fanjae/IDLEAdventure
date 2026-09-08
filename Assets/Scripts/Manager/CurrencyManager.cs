using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 재화 종류 열거
/// </summary>
public enum CurrencyType
{
    None = -1,
    GOLD, EXP, UPGRADE, GEM,     // 이후 추가될 재화 추가
    Length
}
/// <summary>
/// 재화 관리 클래스. <br/>
/// 플레이어가 획득 및 사용할 재화들을 통합 관리하는 매니저 클래스. <br/>
/// 사용법 <br/>
/// 재화 획득: AddCurrency(재화 타입, 재화량) <br/>
/// 재화 소모: UseCurrency(재화 타입, 재화량) <br/>
/// 재화 확인(값 반환): GetCurrency(재화 타입)
/// </summary>
public class CurrencyManager : Singleton<CurrencyManager>, ISaveDataWriter
{
    // 필요 없긴 한데 혹시 어떻게 적용하는지 확인해보실 분들을 위해 주석처리
    // 씬 실행 시 강제로 매니저 생성하는 부분
    // 로딩 구현 시 해당 파트에서 생성하는 방식으로 변경 예정
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void InitializeWhenSceneStart()
    //{
    //    var init = Instance;
    //}

    // 재화 종류별 값 관리
    // 재화 종류가 적으니 최적화 보다는 사용 편리성을 위해 Dictionary 자료구조 선택
    private Dictionary<CurrencyType, int> currencies = new Dictionary<CurrencyType, int>();
    public event Action<CurrencyType, int> OnCurrencyChanged;    // 재화 변화 시 발송할 이벤트

    // 프로퍼티
    // 필요할까?

    protected override void Awake()
    {
        base.Awake();

        // 재화 초기 설정
        for (int i = 0; i < (int)CurrencyType.Length; i++)
        {
            // 재화 종류 별로 초기화
            currencies[(CurrencyType)i] = 0;
        }

        // 현재 재화 상태를 저장 대상으로 등록
        SaveManager.Instance.RegisterWriter(this);
    }

    // 재화 획득 함수
    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        currencies[type] += amount;
        OnCurrencyChanged?.Invoke(type, currencies[type]);

        Debug.Log($"[재화 획득] [{type}] +{amount} | 현재 보유 : {currencies[type]}");
    }
    // 재화 소모 함수
    public bool UseCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return false;
        if (currencies[type] < amount)
        {
            Debug.Log($"[{type}] 소지 재화가 부족합니다.");
            return false;
        }
        currencies[type] -= amount;
        OnCurrencyChanged?.Invoke(type, currencies[type]);

        Debug.Log($"[{type}] 사용에 성공했습니다. | 남은 재화 : {currencies[type]}");

        return true;
    }
    // 현재 재화 반환 함수
    public int GetCurrency(CurrencyType type)
    {
        return currencies[type];
    }

    // 현재 재화 상태를 저장 데이터에 반영 (저장/로드 관련 작업을 위해 0812 추가)
    public void WriteSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        // 저장 데이터에 재화 관련 정보를 저장
        saveData.Currency = new CurrencySaveData
        {
            Gold = currencies[CurrencyType.GOLD],
            Exp = currencies[CurrencyType.EXP],
            Upgrade = currencies[CurrencyType.UPGRADE],
            Gem = currencies[CurrencyType.GEM]
        };
    }

    // 저장 데이터를 기준으로 재화 상태 복원 (저장/로드 관련 작업을 위해 0812 추가) 
    public void LoadSaveData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        // 저장 데이터가 비어 있는 경우 기본 데이터 사용
        saveData.Currency ??= new CurrencySaveData();

        currencies[CurrencyType.GOLD] = saveData.Currency.Gold;
        currencies[CurrencyType.EXP] = saveData.Currency.Exp;
        currencies[CurrencyType.UPGRADE] = saveData.Currency.Upgrade;
        currencies[CurrencyType.GEM] = saveData.Currency.Gem;

        OnCurrencyChanged?.Invoke(CurrencyType.GOLD, currencies[CurrencyType.GOLD]);
        OnCurrencyChanged?.Invoke(CurrencyType.EXP, currencies[CurrencyType.EXP]);
        OnCurrencyChanged?.Invoke(CurrencyType.UPGRADE, currencies[CurrencyType.UPGRADE]);
        OnCurrencyChanged?.Invoke(CurrencyType.GEM, currencies[CurrencyType.GEM]);
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