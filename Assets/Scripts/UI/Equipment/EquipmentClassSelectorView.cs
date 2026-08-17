using System;
using UnityEngine;

// 장비 클래스 선택 버튼의 클릭 입력과 선택 상태 처리
public class EquipmentClassSelectorView : MonoBehaviour
{
    [SerializeField] private EquipmentClassButtonData[] classButtons;

    public event Action<HeroClassType> OnClassSelected;

    private void Awake()
    {
        foreach (EquipmentClassButtonData buttonData in classButtons)
        {
            if (buttonData == null || buttonData.Button == null)
            {
                continue;
            }

            EquipmentClassButtonData capturedButtonData = buttonData;
            buttonData.Button.onClick.AddListener(() => SelectClass(capturedButtonData.HeroClass));
        }
    }

    private void OnDestroy()
    {
        foreach (EquipmentClassButtonData buttonData in classButtons)
        {
            if (buttonData == null || buttonData.Button == null)
            {
                continue;
            }

            buttonData.Button.onClick.RemoveAllListeners();
        }
    }

    // 선택한 클래스 버튼 상태 변경 후 클래스 전달
    private void SelectClass(HeroClassType heroClass)
    {
        UpdateSelectedButton(heroClass);
        OnClassSelected?.Invoke(heroClass);
    }

    // 선택한 클래스 버튼만 선택 상태로 변경
    public void UpdateSelectedButton(HeroClassType heroClass)
    {
        foreach (EquipmentClassButtonData buttonData in classButtons)
        {
            if (buttonData == null)
            {
                continue;
            }

            buttonData.SetSelected(buttonData.HeroClass == heroClass);
        }
    }
}