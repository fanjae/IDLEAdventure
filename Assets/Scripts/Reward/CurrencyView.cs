using TMPro;
using UnityEngine;

/// <summary>
/// 특정 재화 값을 받아와 Text로 표기해 주는 클래스. <br/>
/// 재화 표기가 프리팹마다 나뉘어 있어 한 번에 관리가 힘들 것 같아 스크립트만 하나로 통일. <br/>
/// UI마다 직접 넣어 사용하게끔 구현.
/// </summary>
public class CurrencyView : MonoBehaviour
{
    // 표시할 재화 타입 선택
    [Header("Select CurrencyType")]
    [SerializeField] private CurrencyType currencyType;
    // 표시할 Text UI 연결
    [Header("Binding UI Component")]
    [SerializeField] private TMP_Text currencyText;

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateUI;

            int currentAmount = CurrencyManager.Instance.GetCurrency(currencyType);
            UpdateUI(currencyType, currentAmount);
        }
    }
    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateUI;
        }
    }
    // UI 갱신 함수.
    private void UpdateUI(CurrencyType type, int amount)
    {
        if (type == currencyType && currencyText != null)
        {
            currencyText.text = amount.ToString("N0");
        }
    }
}