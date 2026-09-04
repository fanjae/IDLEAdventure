using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 방치 시간을 UI에 표시해줄 클래스.
/// </summary>
public class IdleView : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private TMP_Text idleTimeText;

    [Header("Idle Data")]
    [SerializeField] private IdleReward idleReward;

    private DateTime lastTime;

    private Coroutine uiUpdateCo;
    private WaitForSeconds uiUpdateDelay = new WaitForSeconds(1.0f);

    private void OnEnable()
    {
        if (idleReward != null)
        {
            idleReward.OnGetIdleReward += UpdateUI;
        }
    }
    private void Start()
    {
        UpdateUI();
    }

    private void OnDisable()
    {
        if (idleReward != null)
        {
            idleReward.OnGetIdleReward -= UpdateUI;
        }
    }

    // 방치 보상 시간 갱신 함수.
    private void UpdateUI()
    {
        // 기준 시간 저장.
        lastTime = idleReward.GetLastRewardTime();

        // 코루틴이 실행중이라면 중지 후 실행.
        if (uiUpdateCo != null)
        {
            StopCoroutine(uiUpdateCo);
        }
        uiUpdateCo = StartCoroutine(UIUpdateCoroutine());
    }
    // 방치 보상 시간 갱신 코루틴.
    // 실제 기능은 여기에서 구현.
    IEnumerator UIUpdateCoroutine()
    {
        // 예외처리 후 최대 방치 시간 저장.
        if (idleReward == null)
        {
            Debug.Log("IdleReward 컴포넌트가 연결되지 않았습니다.");
            yield break;
        }
        float maxIdleTime = idleReward.MaxIdleTime;
        
        while (true)
        {
            // 방치 중인 시간 저장.
            float currentIdleTime = IdleCalculator.GetRewardTime(lastTime, maxIdleTime);

            // 방치 중인 시간 float형을 시간 길이인 TimeSpan형으로 형변환.
            TimeSpan idleTimeSpan = TimeSpan.FromSeconds(currentIdleTime);
            // 시간 형태에 맞는 형태로 text 출력.
            idleTimeText.text = $"{(int)idleTimeSpan.TotalHours:D2}:{idleTimeSpan.Minutes:D2}:{idleTimeSpan.Seconds:D2}";

            // 방치 시간이 최대로 찼다면 중지.
            if (currentIdleTime >= maxIdleTime) break;

            // UI 갱신 딜레이. (단위: 1초)
            yield return uiUpdateDelay;
        }
    }
}