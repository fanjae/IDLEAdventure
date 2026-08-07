using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CurrencyManager 작동 확인용 클래스.
/// </summary>
public class CurrencyTest : MonoBehaviour
{
    [Header("Binding Component")]
    //[SerializeField] private TMP_Text goldText;
    //[SerializeField] private TMP_Text expText;
    //[SerializeField] private TMP_Text diamondText;
    [SerializeField] private TMP_Text[] currencyTexts = new TMP_Text[3];

    private void Awake()
    {
        CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyText;

        for (int i = 0; i < (int)CurrencyType.Length; i++)
        {
            UpdateCurrencyText((CurrencyType)i, CurrencyManager.Instance.GetCurrency((CurrencyType)i));
        }
    }

    public void ClickAddButton()
    {
        CurrencyManager.Instance.AddCurrency(CurrencyType.GOLD, 100);
    }
    public void ClickUseButton()
    {
        CurrencyManager.Instance.UseCurrency(CurrencyType.GOLD, 50);
    }
    public void ClickSceneChangeButton()
    {
        SceneManager.LoadScene("SampleScene");
    }
    private void UpdateCurrencyText(CurrencyType type, int amount)
    {
        if ((int)type >= 0 && (int)type < currencyTexts.Length)
        {
            currencyTexts[(int)type].text = $"{type}: {amount}";
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyText;
        }
    }
}