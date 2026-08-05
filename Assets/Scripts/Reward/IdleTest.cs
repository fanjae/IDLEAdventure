using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Idle 시간 적용 확인용 클래스 <br/>
/// UI 관련 내용은 아직 어떤 방식으로 할지 정하지 못 함. <br/>
/// 일단 Update에서 하는 것은 피하기 위해 코루틴을 사용.
/// </summary>
public class IdleTest : MonoBehaviour
{
    [Header("Binding Component")]
    [SerializeField] private TMP_Text lastRewardTimeText;
    [SerializeField] private TMP_Text rewardPercentText;

    [Header("Setting Reward")]
    [SerializeField] private int goldPerTime = 10;
    [SerializeField] private float maxIdleTime = 10.0f;

    private DateTime lastTime;  // 시간을 계산하기 위한 DateTime 구조체 사용

    private Coroutine uiUpdateCoroutine;

    public int GoldPerTime => goldPerTime;
    public float MaxIdleTime => maxIdleTime;

    private void Start()
    {
        InitializeTime();
    }

    // 방치 보상 수령 버튼 클릭 시 호출될 함수
    // 시간당 보상을 계산해 획득
    public void ClickIdleRewardButton()
    {
        // 시간 비교
        int rewardAmount = CalculateReward();

        if (rewardAmount > 0)
        {
            // 보상 획득
            CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, rewardAmount);
            Debug.Log($"방치 보상 {rewardAmount} 골드 획득");
            // 시간 저장
            SaveCurrentTime();
            RestartUI();
        }
        Debug.Log("획득할 보상이 없습니다.");
    }
    // 게임 실행 시 시간 설정
    // 첫 실행이라면 현 시간을 저장
    private void InitializeTime()
    {
        long savedTime = TestSaveManager.Instance.CurrentSaveData.LastGetIdleRewardTime;
        if (savedTime == 0)
        {
            SaveCurrentTime();
            RestartUI();
            return;
        }
        lastTime = DateTime.FromBinary(savedTime);

        RestartUI();
    }
    // 현재 시간을 저장하는 함수
    private void SaveCurrentTime()
    {
        lastTime = DateTime.UtcNow;
        // 시간 값을 DateTime.ToBinary 함수를 통해 int64값으로 변환
        TestSaveManager.Instance.CurrentSaveData.SetLastIdleRewardTime(lastTime.ToBinary());
        TestSaveManager.Instance.SaveGame();
    }
    // 보상 수치를 결정할 시간 값 계산 함수
    private float GetRewardTime()
    {
        TimeSpan rewardTime = DateTime.UtcNow - lastTime;
        float passedTime = (float)rewardTime.TotalMinutes;

        if (passedTime < 0.0f)
        {
            passedTime = 0.0f;
        }

        return Mathf.Clamp(passedTime, 0.0f, MaxIdleTime);
    }
    // 계산된 시간 값을 통해 보상 수치를 계산하는 함수
    private int CalculateReward()
    {
        float rewardTime = GetRewardTime();
        return (int)(rewardTime * goldPerTime);
    }


    // 테스트용 UI 연결
    private void UpdateUI()
    {
        float rewradTime = GetRewardTime();
        float percent = (rewradTime / MaxIdleTime) * 100.0f;

        lastRewardTimeText.text = $"LastGetIdleRewardTime: {lastTime.ToLocalTime():yyyy/MM/dd HH:mm:ss}";
        rewardPercentText.text = $"IdleReward: {CalculateReward()} G ({percent:F1} %)";
    }
    private void RestartUI()
    {
        if (uiUpdateCoroutine != null)
        {
            StopCoroutine(uiUpdateCoroutine);
        }
        uiUpdateCoroutine = StartCoroutine(UpdateUICo());
    }
    IEnumerator UpdateUICo()
    {
        while (true)
        {
            UpdateUI();

            float currnetTime = GetRewardTime();

            if (currnetTime >= maxIdleTime)
            {
                rewardPercentText.text = "IdleReward: 보상이 가득 찼습니다. 수령해주세요.";
                uiUpdateCoroutine = null;
                break;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}