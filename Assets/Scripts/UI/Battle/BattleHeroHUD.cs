using System.Collections;
using UnityEngine;

public class BattleHeroHUD : MonoBehaviour
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private BattleHeroSlotUI heroSlotPrefab;

    private IEnumerator Start()
    {
        //BattleUnit들이 BattleManager에 등록될 때까지 한 프레임 기다림
        yield return null;

        CreateHeroSlots();
    }
    private void CreateHeroSlots()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager가 없음");
            return;
        }
        if (slotContainer == null || heroSlotPrefab == null)
        {
            Debug.LogError("BattleHeroHUD의 UI 연결 확인 ㄱㄱ");
            return;
        }

        foreach (BattleUnit hero in BattleManager.Instance.HeroUnits)
        {
            if (hero == null || !hero.gameObject.activeInHierarchy) continue;

            BattleHeroSlotUI slot = Instantiate(heroSlotPrefab, slotContainer);
            slot.Initialize(hero);
        }
    }
}
