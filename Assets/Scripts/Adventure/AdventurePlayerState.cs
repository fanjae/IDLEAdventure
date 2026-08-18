using UnityEngine;

/// <summary>
/// 플레이어가 가질 상태 정의 추상 클래스.
/// </summary>
public abstract class AdventurePlayerState : MonoBehaviour
{
    protected AdventurePlayerStateMachine stateMachine;

    public virtual void Initialize(AdventurePlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void OnEnter() { this.enabled = true; }
    public virtual void OnExit() { this.enabled = false; }
    public virtual void OnUpdate() { }
}