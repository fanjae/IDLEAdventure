using UnityEngine;

// 전투 배치 영웅 목록 UI 관리
public sealed class FormationHeroListController : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private FormationHeroCardView heroCardPrefab;
    [SerializeField] private HeroClassIconCatalog classIconCatalog;

    private HeroController heroController;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    // 보유 영웅 데이터를 기준으로 목록 초기화
    private void Initialize()
    {
        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            Debug.LogError("HeroManager가 초기화되지 않았습니다.");
            return;
        }

        if (content == null)
        {
            Debug.LogError("FormationHeroListController의 Content가 없습니다.");
            return;
        }

        if (heroCardPrefab == null)
        {
            Debug.LogError("FormationHeroListController의 HeroCardPrefab이 없습니다.");
            return;
        }

        heroController = HeroManager.Instance.Controller;
        heroController.OnHeroCollectionChanged += Refresh;
        heroController.OnHeroLevelChanged += HandleHeroLevelChanged;

        Refresh();
    }

    // 현재 보유 영웅 목록을 기준으로 카드 갱신
    private void Refresh()
    {
        ClearCards();

        foreach (OwnedHeroData hero in heroController.Heroes)
        {
            FormationHeroCardView cardView = Instantiate(heroCardPrefab, content);
            cardView.Bind(hero, classIconCatalog);
        }
    }

    // 영웅 레벨 변경 시 목록 갱신
    private void HandleHeroLevelChanged(OwnedHeroData _)
    {
        Refresh();
    }

    // 생성된 영웅 카드 제거
    private void ClearCards()
    {
        for (int index = content.childCount - 1; index >= 0; index--)
        {
            Destroy(content.GetChild(index).gameObject);
        }
    }

    // 영웅 데이터 이벤트 구독 해제
    private void Unsubscribe()
    {
        if (heroController == null)
        {
            return;
        }

        heroController.OnHeroCollectionChanged -= Refresh;
        heroController.OnHeroLevelChanged -= HandleHeroLevelChanged;
        heroController = null;
    }
}