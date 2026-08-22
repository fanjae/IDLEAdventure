using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스테이지 진행 노드 UI 관리
public sealed class StageNodeView : MonoBehaviour
{
    [SerializeField] private Image nodeIcon;
    [SerializeField] private TMP_Text stageNumberText;

    [Header("Stage State Sprites")]
    [SerializeField] private Sprite clearedSprite;
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Sprite lockedSprite;

    // 스테이지 번호와 진행 상태 설정
    public void Bind(int stageNumber, bool isCurrent, bool isCleared)
    {
        stageNumberText.text = stageNumber.ToString();

        UpdateState(isCurrent, isCleared);
    }

    // 스테이지 진행 상태에 맞춰 UI 갱신
    private void UpdateState(bool isCurrent, bool isCleared)
    {
        if (isCurrent)
        {
            nodeIcon.sprite = currentSprite;
            return;
        }

        if (isCleared)
        {
            nodeIcon.sprite = clearedSprite;
            return;
        }

        nodeIcon.sprite = lockedSprite;
    }
}