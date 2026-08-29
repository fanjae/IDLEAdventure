using System;
using UnityEngine;
using UnityEngine.UI;

// 일괄 장착 버튼의 상태 표시와 클릭 입력 처리
public class EquipmentAutoEquipButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;

    [SerializeField] private Sprite availableSprite;
    [SerializeField] private Sprite unavailableSprite;

    private bool canAutoEquip;

    public event Action OnClicked;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    // 일괄 장착 가능 여부에 따른 이미지 변경 처리
    public void SetAvailable(bool available)
    {
        canAutoEquip = available;

        if (buttonImage != null)
        {
            buttonImage.sprite = available ? availableSprite : unavailableSprite;
        }
    }

    private void HandleClick()
    {
        // 장착 가능한 장비가 없으면 클릭 처리하지 않음
        if (!canAutoEquip)
        {
            return;
        }

        OnClicked?.Invoke();
    }
}
