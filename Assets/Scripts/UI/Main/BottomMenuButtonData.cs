using System;
using UnityEngine;
using UnityEngine.UI;

// 하단 메뉴 버튼 표시 데이터 관리
[Serializable]
public sealed class BottomMenuButtonData
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    public Button Button => button;

    // 메뉴 선택 상태에 따라 스프라이트 변경
    public void SetSelected(bool isSelected)
    {
        if (image == null)
        {
            return;
        }

        Sprite targetSprite = isSelected ? selectedSprite : normalSprite;

        if (targetSprite != null)
        {
            image.sprite = targetSprite;
        }
    }
}