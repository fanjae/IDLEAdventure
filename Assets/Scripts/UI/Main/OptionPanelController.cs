using UnityEngine;
using UnityEngine.UI;

// 옵션 패널 내부 UI 및 설정값 저장 처리
public sealed class OptionPanelController : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button saveButton;

    [Header("사운드")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("패널 연출")]
    [SerializeField] private UIPanelTransition panelTransition;

    private void OnEnable()
    {
        if (cancelButton != null) cancelButton.onClick.AddListener(HandleCancelButtonClicked);
        if (saveButton != null) saveButton.onClick.AddListener(HandleSaveButtonClicked);

        LoadOptionValues();
        panelTransition?.PlayOpen();
    }

    private void OnDisable()
    {
        if (cancelButton != null) cancelButton.onClick.RemoveListener(HandleCancelButtonClicked);
        if (saveButton != null) saveButton.onClick.RemoveListener(HandleSaveButtonClicked);
    }

    // 설정 취소 버튼 클릭 처리
    private void HandleCancelButtonClicked()
    {
        ClosePanel();
    }

    private void ClosePanel()
    {
        if (panelTransition == null)
        {
            gameObject.SetActive(false);
            return;
        }

        panelTransition.PlayClose(() => gameObject.SetActive(false));
    }

    // 설정 저장 버튼 클릭 처리
    private void HandleSaveButtonClicked()
    {
        if (bgmSlider == null || sfxSlider == null)
        {
            return;
        }

        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) || saveManager.CurrentData == null)
        {
            return;
        }

        saveManager.CurrentData.Option ??= new OptionSaveData();
        saveManager.CurrentData.Option.BgmVolume = bgmSlider.value;
        saveManager.CurrentData.Option.SfxVolume = sfxSlider.value;

        saveManager.Save();
        ClosePanel();
    }

    // 저장된 설정값을 슬라이더에 반영
    private void LoadOptionValues()
    {
        if (bgmSlider == null || sfxSlider == null)
        {
            return;
        }

        if (!SaveManager.TryGetExistingInstance(out SaveManager saveManager) || saveManager.CurrentData == null)
        {
            return;
        }

        saveManager.CurrentData.Option ??= new OptionSaveData();

        bgmSlider.value = saveManager.CurrentData.Option.BgmVolume;
        sfxSlider.value = saveManager.CurrentData.Option.SfxVolume;
    }
}