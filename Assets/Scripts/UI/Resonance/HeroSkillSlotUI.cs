using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeroSkillSlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image skillIcon;
    [SerializeField] private GameObject selectionBorder;

    private SkillDataSO skillData;
    private Action<HeroSkillSlotUI, SkillDataSO> onSelected;

    public SkillDataSO SkillData => skillData;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    // 슬롯에 표시할 스킬 데이터를 연결
    public void Bind(SkillDataSO data, Action<HeroSkillSlotUI, SkillDataSO> selectedCallback)
    {
        skillData = data;
        onSelected = selectedCallback;

        if (skillIcon != null)
        {
            skillIcon.sprite = data != null ? data.Icon : null;
            skillIcon.preserveAspect = true;
            skillIcon.enabled = data != null && data.Icon != null;
        }

        SetSelected(false);
        gameObject.SetActive(data != null);
    }

    // 현재 슬롯의 선택 상태 표시
    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(selected);
        }
    }

    // 슬롯 클릭 시 현재 스킬 정보를 상위 UI에 전달
    private void HandleClicked()
    {
        if (skillData == null)
        {
            return;
        }

        onSelected?.Invoke(this, skillData);
    }
}