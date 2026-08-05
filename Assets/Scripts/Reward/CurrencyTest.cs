using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// CurrencyManager 작동 확인용 클래스.
/// </summary>
public class CurrencyTest : MonoBehaviour
{
    [Header("Binding Component")]
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        CurrencyManager.Instance.OnCurrencyChanged += UpdateGoldText;

        UpdateGoldText(CurrencyType.Gold, CurrencyManager.Instance.GetCurrency(CurrencyType.Gold));
    }

    public void ClickAddButton()
    {
        CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, 100);
    }
    public void ClickUseButton()
    {
        CurrencyManager.Instance.UseCurrency(CurrencyType.Gold, 50);
    }
    public void ClickSceneChangeButton()
    {
        SceneManager.LoadScene("SampleScene");
    }
    private void UpdateGoldText(CurrencyType type, int amount)
    {
        if (type == CurrencyType.Gold)
        {
            goldText.text = $"Gold : {amount}";
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateGoldText;
        }
    }
}