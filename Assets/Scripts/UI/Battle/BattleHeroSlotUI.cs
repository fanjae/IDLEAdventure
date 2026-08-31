using UnityEngine;
using UnityEngine.UI;

public class BattleHeroSlotUI : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private RectTransform hpBarFill;

    private BattleUnit unit;
    private float fullHpWidth;


    private void Awake()
    {
        if (hpBarFill != null) fullHpWidth = hpBarFill.rect.width;
    }
    private void Update()
    {
        UpdateHp();
    }


    public void Initialize(BattleUnit battleUnit)
    {
        unit = battleUnit;
        if (unit == null) return;
        if (unit.UnitData is HeroData heroData && portrait != null) portrait.sprite = heroData.Portrait;

        UpdateHp();
    }

    private void UpdateHp()
    {
        if (unit == null || hpBarFill == null) return;
        if (unit.MaxHp <= 0) return;

        float hpRatio = Mathf.Clamp01((float)unit.CurrentHp / unit.MaxHp);
        hpBarFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullHpWidth * hpRatio);
    }
}
