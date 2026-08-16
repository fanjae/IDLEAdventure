using System;
using UnityEngine;
using UnityEngine.UI;

// 클래스 선택 버튼 한 개의 표시 데이터 관리
[Serializable]
public class EquipmentClassButtonData
{
    [SerializeField] private HeroClassType heroClass;
    [SerializeField] private Button button;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    public HeroClassType HeroClass => heroClass;
    public Button Button => button;

    // 선택 상태에 맞게 버튼 이미지 변경
    public void SetSelected(bool selected)
    {
        if (button == null || button.image == null)
        {
            return;
        }

        button.image.sprite = selected ? selectedSprite : normalSprite;
    }
}