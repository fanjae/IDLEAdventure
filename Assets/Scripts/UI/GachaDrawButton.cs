using System;
using UnityEngine;
using UnityEngine.Events;

// 버튼 OnClick에서 1회 또는 10회 소환을 호출하는 중계 컴포넌트임
public sealed class GachaDrawButton : MonoBehaviour
{
    [SerializeField] private string bannerId = "Standard";
    [Min(1)] [SerializeField] private int drawCount = 1;
    [SerializeField] private UnityEvent onDrawCompleted;
    [SerializeField] private UnityEvent<GachaDrawFailure> onDrawFailed;

    // 직전 성공 결과를 결과 패널에서 읽을 수 있게 보관함
    public GachaDrawResult LastResult { get; private set; }

    // 코드 기반 결과 UI가 소환 성공을 받을 수 있게 알림
    public event Action<GachaDrawResult> DrawCompleted;

    // 코드 기반 결과 UI가 소환 실패를 받을 수 있게 알림
    public event Action<GachaDrawFailure> DrawFailed;

    // 선택된 배너에 맞게 버튼 소환 정보를 갱신함
    public void Configure(string targetBannerId, int targetDrawCount)
    {
        bannerId = targetBannerId;
        drawCount = Mathf.Max(1, targetDrawCount);
    }

    // 버튼 클릭 시 설정된 배너와 횟수로 소환 시도함
    public void Draw()
    {
        if (GachaManager.Instance == null || !GachaManager.Instance.IsInitialized)
        {
            LastResult = null;
            DrawFailed?.Invoke(GachaDrawFailure.BannerNotFound);
            onDrawFailed?.Invoke(GachaDrawFailure.BannerNotFound);
            return;
        }

        if (GachaManager.Instance.Controller.TryDraw(bannerId, drawCount, out GachaDrawResult result, out GachaDrawFailure failure))
        {
            LastResult = result;
            DrawCompleted?.Invoke(result);
            onDrawCompleted?.Invoke();
            return;
        }

        LastResult = null;
        DrawFailed?.Invoke(failure);
        onDrawFailed?.Invoke(failure);
    }
}
