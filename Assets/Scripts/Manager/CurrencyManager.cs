using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 재화 종류 열거
/// </summary>
public enum CurrencyType
{
    None = -1,
    GOLD, EXP, UPGRADE,     // 이후 추가될 재화 추가
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
public class CurrencyManager : Singleton<CurrencyManager>
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
            // 임시 세이브 매니저에 저장되어 있는 재화 받아오기.
            currencies[(CurrencyType)i] = TestSaveManager.Instance.CurrentSaveData.CurrencyDatas[i];
        }
    }

    // 재화 획득 함수
    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        currencies[type] += amount;
        OnCurrencyChanged?.Invoke(type, currencies[type]);

        // 임시 재화 저장용 코드
        TestSaveManager.Instance.CurrentSaveData.SetCurrency(type, currencies[type]);
        TestSaveManager.Instance.SaveGame();

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

        // 임시 재화 저장용 코드
        TestSaveManager.Instance.CurrentSaveData.SetCurrency(type, currencies[type]);
        TestSaveManager.Instance.SaveGame();

        Debug.Log($"[{type}] 사용에 성공했습니다. | 남은 재화 : {currencies[type]}");

        return true;
    }
    // 현재 재화 반환 함수
    public int GetCurrency(CurrencyType type)
    {
        return currencies[type];
    }
}