using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Idle 시스템 적용 확인용 클래스. <br/>
/// UI 관련 내용은 아직 어떤 방식으로 할지 정하지 못 함. <br/>
/// 일단 Update에서 하는 것은 피하기 위해 코루틴을 사용. <br/>
/// 현재 방치 보상 계산 방식이 기존 시간 ~ 수령 시간을 통해 스테이지 단계에 맞는 보상 값으로 수령이 되기에 쌓아놓고 스테이지 단계가 올라간 후 수령하면 올라간 단계의 보상으로 수령됨. <br/>
/// 하지만, 그렇다고 실시간으로 수령 가능한 재화를 계산하고, 쌓아두는 방식은 성능상 문제가 있다고 판단됨. <br/>
/// 때문에 현재 방식으로는 스테이지 클리어 시 단계를 올리기 전에 기존 방치 보상을 수령시키고 해당 시점을 최종 수령 시간으로 재설정 후 스테이지를 올리는 방법으로 진행하려 함.
/// </summary>
public class IdleTest : MonoBehaviour
{
    [Header("Binding UI Component")]
    [SerializeField] private TMP_Text lastRewardTimeText;
    [SerializeField] private TMP_Text rewardPercentText;

    [Header("Setting Reward")]
    [SerializeField] private TextAsset rewardCSVData;    // 보상 테이블로 사용할 CSV 데이터 연결
    [SerializeField] private float maxIdleTime = 100.0f;    // 최대 방치 시간 (단위: 초)
    [SerializeField] private int testClearStageNum = 1;    // 최종 클리어 스테이지 단계 적용용 임시 변수

    private DateTime lastTime;  // 시간을 계산하기 위한 DateTime 구조체 사용

    private Coroutine uiUpdateCoroutine;    // Update를 사용하지 않기 위한 코루틴
    // 스테이지 단계별 보상 테이블 저장용 딕셔너리 | Key: 스테이지 번호, Value: CSV에서 받아온 보상 테이블 데이터
    private Dictionary<int, StageRewardData> stageRewards = new Dictionary<int, StageRewardData>();
    // 방치 보상 계산 방식으로 인해 버려지는 시간 저장용 딕셔너리 | Key: 보상 ID, Value: 보상에 반영되지 못 한 버려진 시간
    private Dictionary<string, decimal> leftRewards = new Dictionary<string, decimal>();

    // 프로퍼티
    public float MaxIdleTime => maxIdleTime;

    private void Start()
    {
        LoadRewardCSVData();
        InitializeTime();
    }

    // 보상 CSV 파일 받아오는 함수
    private void LoadRewardCSVData()
    {
        if (rewardCSVData == null) return;
        // 줄 개행을 기준으로 분할
        string[] lines = rewardCSVData.text.Split('\n');
        // 줄 수 만큼 반복
        // 첫 줄은 데이터 이름이기에 1번 줄부터 검사
        for (int i = 1; i < lines.Length; i++)
        {
            // Trim() 함수를 통해 의도치 않은 공백 제거
            string line = lines[i].Trim();
            // 문자열이 비었는지, 공백으로만 이루어져있는지 확인
            if (string.IsNullOrWhiteSpace(line)) continue;
            // 줄 마다 ',' 기준으로 분리
            string[] row = lines[i].Split(',');
            // 분리했을 때 stageId, resourceId, amount 세 가지가 제대로 들어있는지 확인
            if (row.Length >= 3)
            {
                // 문자열로 적혀있는 숫자를 각각 int, float 형식으로 변환
                // 안전하게 Try함수 사용 및 Trim() 함수를 통해 공백 제거
                if (int.TryParse(row[0].Trim(), out int stageId) &&
                    float.TryParse(row[2].Trim(), out float amountPerSecond))
                {
                    // 문자열 그대로 쓰기에 변환 없이 공백만 제거 후 저장
                    string resourceId = row[1].Trim();
                    // 보상 정보를 담을 인터페이스 객체 선언
                    IReward reward;
                    // true 매개변수를 통해 대소문자 구분 x
                    // 재화일 경우 (CSV의 ResourceID가 CurrencyType으로 제대로 변환이 된 경우)
                    // 재화 ID 열거형으로 반환
                    if (Enum.TryParse(resourceId, true, out CurrencyType type))
                    {
                        reward = new CurrencyReward(type, amountPerSecond);
                    }
                    // 아이템인 경우 (CSV의 ResourceID가 CurrencyType으로 변환에 실패한 경우)
                    // 보상 지급될 아이템들도 열거형으로 관리가 된다면 열거현 변환으로 처리하면 좋을듯?
                    else
                    {
                        reward = new ItemReward(resourceId, amountPerSecond);
                    }
                    // 만약 스테이지 ID값이 저장해둔 스테이지 보상 테이블에 존재하지 않는 값이 들어왔다면
                    if (!stageRewards.ContainsKey(stageId))
                    {
                        // 해당 번호의 데이터 추가
                        stageRewards[stageId] = new StageRewardData();
                    }
                    // 스테이지 번호에 맞게 보상 데이터 저장
                    stageRewards[stageId].GetReward(reward);
                }
                else
                {
                    Debug.Log("파싱 실패");
                }
            }
        }
    }

    // 방치 보상 수령 버튼 클릭 시 호출될 함수
    // 시간당 보상을 계산해 획득
    public void ClickIdleRewardButton()
    {
        if (testClearStageNum <= 0) return;
        // 재화 별 시간당 보상 계산
        Dictionary<string, RewardResult> rewards = CalculateRewards();
        /***********
        LINQ의 Any()함수 사용
        컬렉션에 들어있는 자료형 (배열, 리스트, 딕셔너리) 안에 조건에 맞는 데이터가 하나라도 존재하는지 확인하는 함수.
        전부 검사하지 않고, 중간에 조건에 맞는 데이터를 찾는 순간 검사를 중단하기에 성능상 이점이 있다.
        ***********/
        // 버튼을 눌렀을 때 보상 딕셔너리 값 중 0보다 큰 값이 있는지 확인. (획득 가능한 재화가 존재하는지 확인)
        if (rewards.Any(reward => reward.Value.FinalReward > 0))
        {
            // 스테이지 보상 테이블에서 최종 클리어 스테이지 값을 통해 알맞은 보상 테이블 반환
            if (stageRewards.TryGetValue(testClearStageNum, out StageRewardData stageData))
            {
                /***********
                유니티 5.5버전 이후로 foreach의 GC 문제가 해결되어 일반적으로 for문과 성능상 차이가 거의 나지 않는다.
                또한, 순회 속도가 딕셔너리 가준으로는 for문보다 foreach문이 더 빠르다.
                해당 파트는 나중에 실험해 보면 좋을듯.
                KeyValuePair<,> 방식은 딕셔너리가 사용하는 반환형. 정 귀찮다면 var를 써도 무방할듯.
                ***********/
                // 딕셔너리를 순회하며 반환 받은 보상 테이블의 정보를 key: 보상 ID, value: 보상량 형태로 사용
                foreach (KeyValuePair<string, RewardResult> reward in rewards)
                {
                    string id = reward.Key;    // 보상 ID 저장
                    int amount = reward.Value.FinalReward;    // 보상량 저장
                    decimal leftover = reward.Value.LeftoverReward;    // 보상 정산 후 남은 시간 저장

                    leftRewards[id] = leftover;    // 남은 시간 딕셔너리에 해당 보상 ID를 Key로 저장

                    // 보상량이 존재하고, 보상 테이블에 해당 ID값이 존재한다면
                    if (amount > 0 && stageData.Rewards.ContainsKey(id))
                    {
                        // ID에 따른 보상 제공.
                        stageData.Rewards[id].GiveReward(amount);
                    }
                }
            }
            Debug.Log("방치 보상 획득 완료.");
            SaveCurrentTime();
            RestartUI();
            //return;
        }
        else
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
        // 현재 실제 시간 - 저징된 시간
        TimeSpan rewardTime = DateTime.UtcNow - lastTime;
        // 계산된 방치 시간 초 단위로 변환
        float passedTime = (float)rewardTime.TotalSeconds;
        // 방치 시간이 음수로 가지 않게 처리
        if (passedTime < 0.0f)
        {
            passedTime = 0.0f;
        }
        // 최대 방치 시간을 넘어가지 않게 처리
        return Mathf.Clamp(passedTime, 0.0f, MaxIdleTime);
    }
    // 계산된 시간 값을 통해 보상 수치를 계산하는 함수
    // 재화 별로 모두 대응하기 위해 딕셔너리 자료형으로 구현
    private Dictionary<string, RewardResult> CalculateRewards()
    {
        Dictionary<string, RewardResult> calculateRewards = new Dictionary<string, RewardResult>();
        /***********
        계산 오차 방지를 위해 decimal 자료형 사용.
        decimal: 부동 소수점 자료형. float의 ~f처럼 ~m으로 사용한다.
        double, float 등 다른 실수 자료형과 달리 2진수가 아닌 10진수를 기반으로 연산
        성능 면에서는 두 실수형보다 효율적이지 못 하다.
        재화/금액(돈) 등 0.000001의 오차도 존재하면 안 되는 계산에 사용.
        ***********/
        // float으로 계산하면, 계산상 0.01 * 100 = 1이 나와야 하는데, 0.999999... 가 나오는 오차가 존재
        // 계산된 방치 시간 형변환 후 저장
        decimal rewardTime = (decimal)GetRewardTime();
        // 스테이지에 맞는 보상 테이블 반환
        if (stageRewards.TryGetValue(testClearStageNum, out StageRewardData stageData))
        {
            // Key: 보상 ID, Value: 보상 객체
            foreach (KeyValuePair<string, IReward> rewardData in stageData.Rewards)
            {
                string id = rewardData.Key;    // 순회중인 보상의 ID값 저장

                decimal rewardAmount = (decimal)rewardData.Value.RewardValue;    // 기준이 될 재화량 저장
                decimal leftReward = leftRewards.ContainsKey(id) ? leftRewards[id] : 0m;    // 이전 정산 후 남은 보상량 저장
                
                decimal maxReward = (decimal)maxIdleTime * rewardAmount;    // 수령 가능한 최대 보상량 미리 계산
                decimal finalAmount = Math.Min(((rewardTime * rewardAmount) + leftReward), maxReward);    // 계산된 보상량이 최대 수령 가능 보상량을 넘지 못 하게 제한

                int finalReward = (int)finalAmount;    // 최종 보상 형변환
                decimal leftoverReward = finalAmount - finalReward;    // 혹시 남는 보상이 있는지 체크

                calculateRewards[id] = new RewardResult(finalReward, leftoverReward);    // 최종 보상량과 남는 보상량 저장
            }
        }
        return calculateRewards;    // 최종 보상 결과가 담긴 딕셔너리 반환
    }

    // 테스트용 UI 연결
    private void UpdateUI()
    {
        float rewradTime = GetRewardTime();
        float percent = (rewradTime / MaxIdleTime) * 100.0f;

        lastRewardTimeText.text = $"LastGetIdleRewardTime: {lastTime.ToLocalTime():yyyy/MM/dd HH:mm:ss}";
        rewardPercentText.text = $"IdleReward: {percent:F1} %";
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
                rewardPercentText.text = "IdleReward: 100%";
                uiUpdateCoroutine = null;
                break;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}