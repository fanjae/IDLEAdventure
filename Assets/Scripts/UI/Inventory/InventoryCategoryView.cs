using UnityEngine;

// 인벤토리 카테고리 선택 관리
public sealed class InventoryCategoryView : MonoBehaviour
{
    [SerializeField] private InventoryCategoryTabView[] tabs;
    [SerializeField] private int defaultSelectedIndex;

    private int selectedIndex = -1;

    private void Awake()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int tabIndex = i;
            tabs[i].Button.onClick.AddListener(() => SelectTab(tabIndex));
        }

        SelectTab(defaultSelectedIndex);
    }

    // 선택된 카테고리 탭만 활성 상태로 표시
    private void SelectTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Length)
        {
            return;
        }

        selectedIndex = tabIndex;

        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetSelected(i == selectedIndex);
        }
    }
}