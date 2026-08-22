using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 데이터베이스 배너 목록을 탭 UI로 생성함
public sealed class GachaBannerTabList : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GachaBannerTabButton tabTemplate;

    private readonly List<GachaBannerTabButton> tabs = new();

    public event Action<string> BannerSelected;

    // 노출 가능한 배너만 정렬해 탭으로 구성함
    public void Initialize(IReadOnlyList<GachaBannerDataSO> banners, string selectedBannerId)
    {
        ClearRuntimeTabs();

        if (tabTemplate == null || content == null || banners == null)
        {
            return;
        }

        tabTemplate.gameObject.SetActive(false);

        foreach (GachaBannerDataSO banner in banners
                     .Where(value => value != null && value.IsVisible)
                     .OrderBy(value => value.DisplayOrder))
        {
            GachaBannerTabButton tab = Instantiate(tabTemplate, content);
            tab.gameObject.SetActive(true);
            tab.Configure(banner, banner.BannerId == selectedBannerId);
            tab.Clicked += HandleTabClicked;
            tabs.Add(tab);
        }
    }

    // 선택 변경 시 모든 탭 선택 상태 갱신함
    public void SetSelected(string selectedBannerId)
    {
        foreach (GachaBannerTabButton tab in tabs)
        {
            tab.SetSelected(tab.BannerId == selectedBannerId);
        }
    }

    // 생성한 런타임 탭만 정리하고 템플릿은 유지함
    private void ClearRuntimeTabs()
    {
        foreach (GachaBannerTabButton tab in tabs)
        {
            if (tab == null)
            {
                continue;
            }

            tab.Clicked -= HandleTabClicked;
            Destroy(tab.gameObject);
        }

        tabs.Clear();
    }

    // 탭 클릭을 상위 패널에 전달함
    private void HandleTabClicked(string bannerId)
    {
        BannerSelected?.Invoke(bannerId);
    }
}
