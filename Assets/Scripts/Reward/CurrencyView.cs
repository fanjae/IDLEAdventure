using TMPro;
using UnityEngine;

/// <summary>
/// 특정 재화 값을 받아와 Text로 표기해 주는 클래스. <br/>
/// 재화 표기가 프리팹마다 나뉘어 있어 한 번에 관리가 힘들 것 같아 스크립트만 하나로 통일하고 UI마다 직접 넣어 사용하게끔 구현. <br/>
/// 사용법 Ex) Gem Slot <br/>
/// Gem 이미지와 값을 담고 있는 부모 객체 Gem slot에 컴포넌트로 해당 스크립트 추가 <br/>
/// currencyType: GEM 설정 <br/>
/// currencyText: TextUI 드래그 & 드랍 <br/>
/// 끝.
/// </summary>
public class CurrencyView : MonoBehaviour
{
    [Header("Select CurrencyType")]
    [SerializeField] private CurrencyType currencyType;

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

    private void UpdateUI(CurrencyType type, int amount)
    {
        if (type == currencyType && currencyText != null)
        {
            currencyText.text = amount.ToString("N0");
        }
    }
}