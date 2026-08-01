using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged; //체력 변화에 대한 이벤트
    public event Action OnDead; //사망 시 경험치 지급, 애니메이션 재생 같은 것들을 호출할 때

    public int CurrentHp {  get; private set; }
    public int MaxHp { get; private set; }

    public bool IsDead => CurrentHp <= 0;

    private bool isInitialized;

    public void Initialize(int maxHp)
    {
        MaxHp = Mathf.Max(1, maxHp);
        CurrentHp = MaxHp;

        isInitialized = true;
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }
    public void TakeDamage(int damage)
    {
        if (!isInitialized || IsDead) return; //초기화되지 않았거나 이미 죽은상태면 리턴

        damage = Mathf.Max(0, damage); //마이너스 데미지가 들어와서 체력이 회복되는 것을 방지
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0)
        {
            OnDead?.Invoke();
        }
    }
    public void Heal(int amount)//힐
    {
        if (!isInitialized || IsDead) return; 

        amount = Mathf.Max(0, amount); //마이너스 힐이 들어와서 데미지를 입는 버그 방지

        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }
}
