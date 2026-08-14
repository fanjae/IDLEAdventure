using UnityEngine;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour
{
    [Header("대상 유닛")]
    [SerializeField] private BattleUnit unit;

    [Header("체력 게이지")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private GameObject hpFill;

    [Header("스킬 쿨타임 게이지")]
    [SerializeField] private Slider skillSlider;
    [SerializeField] private GameObject skillFill;

    private UnitSkill skill;
    private Camera mainCamera;

    private void Awake()
    {
        if (unit == null) unit = GetComponentInParent<BattleUnit>();
        if (unit != null) skill = unit.GetComponent<UnitSkill>();

        mainCamera = Camera.main;

        InitializeSliders();

        gameObject.SetActive(false);
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted += HandleBattleStarted;
        }
    }
    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= HandleBattleStarted;
        }
    }
    void Update()
    {
        UpdateHpSlider();
        UpdateSkillSlider();
    }
    private void LateUpdate()
    {
        FaceCamera();
    }

    private void InitializeSliders()
    {
        if (hpSlider != null)
        {
            hpSlider.minValue = 0.0f;
            hpSlider.maxValue = 1.0f;
            hpSlider.interactable = false;
        }
        if (skillSlider != null)
        {
            skillSlider.minValue = 0.0f;
            skillSlider.maxValue = 1.0f;
            skillSlider.interactable = false;
            //스킬이 없는 유닛은 스킬 게이지 표시 X
            skillSlider.gameObject.SetActive(skill != null && skill.HasSkill);
        }
    }
    private void HandleBattleStarted()
    {
        //전투 시작 후 머리 위 UI 표시
        gameObject.SetActive(true);
    }
    private void UpdateHpSlider()
    {
        if (unit == null || hpSlider == null) return;

        float hpRatio = unit.MaxHp > 0 ? (float)unit.CurrentHp / unit.MaxHp : 0.0f;
        hpRatio = Mathf.Clamp01(hpRatio);
        hpSlider.value = hpRatio;

        if (hpFill != null) hpFill.SetActive(hpRatio > 0.0f);
    }
    private void UpdateSkillSlider()
    {
        if (skill == null || skillSlider == null) return;

        float cooldownRatio = Mathf.Clamp01(skill.CooldownRatio);
        skillSlider.value = cooldownRatio;

        if (skillFill != null) skillFill.SetActive(cooldownRatio > 0.0f);
    }
    private void FaceCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        transform.rotation = mainCamera.transform.rotation;
    }
}
