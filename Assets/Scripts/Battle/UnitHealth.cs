using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged; //체력 변화에 대한 이벤트
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDead; //사망 시 경험치 지급, 애니메이션 재생 같은 것들을 호출할 때

    public int CurrentHp {  get; private set; }
    public int MaxHp { get; private set; }

    public bool IsDead {  get; private set; }
    public bool IsInitialized { get; private set; }

    public void Initialize(int maxHp)
    {
        MaxHp = Mathf.Max(1, maxHp);
        CurrentHp = MaxHp;

        IsDead = false;
        IsInitialized = true;

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }
    public int TakeDamage(int damage)
    {
        if (!IsInitialized || IsDead) return 0; //초기화되지 않았거나 이미 죽은상태면 리턴

        damage = Mathf.Max(0, damage);
        if (damage <= 0) return 0;

        int previousHp = CurrentHp;
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
       
        int appliedDamage = previousHp - CurrentHp;
        if (appliedDamage <= 0) return 0;

        OnDamaged?.Invoke(appliedDamage);
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0)
        {
            Die();
        }

        return appliedDamage;
    }
    public int Heal(int amount)//힐
    {
        if (!IsInitialized || IsDead) return 0; 

        amount = Mathf.Max(0, amount); //마이너스 힐이 들어와서 데미지를 입는 버그 방지
        if (amount <= 0 ) return 0;

        int previousHp = CurrentHp;
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        
        int appliedHeal = CurrentHp - previousHp;
        if (appliedHeal <= 0) return 0;

        OnHealed?.Invoke(appliedHeal);
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        
        return appliedHeal;
    }
    public void SetMaxHp(int newMaxHp)
    {
        if (!IsInitialized)
        {
            Initialize(newMaxHp);
            return;
        }

        newMaxHp = Mathf.Max(1, newMaxHp);

        int previousMaxHp = MaxHp;
        int changedHp = newMaxHp - previousMaxHp;
        MaxHp = newMaxHp;

        if (changedHp > 0) CurrentHp += changedHp;

        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }
    private void Die()
    {
        if (IsDead) return;

        CurrentHp = 0;
        IsDead = true;

        OnDead?.Invoke();
    }


    //public void ResetHealth()
    //{

    //}
    //public void RestoreFullHealth()
    //{

    //}
}
