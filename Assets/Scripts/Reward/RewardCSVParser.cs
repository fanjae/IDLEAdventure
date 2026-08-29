using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보상 테이블 (.CSV) 파일을 받아오는 클래스. <br/>
/// 방치 및 클리어 보상 테이블 받아오는 과정이 동일해서 분리 후 활용.
/// </summary>
public class RewardCSVParser
{
    private const string eqipboxId = "EQUIPBOX";
    // CSV 파싱 함수
    // 전역 사용을 위해 static으로 선언
    public static Dictionary<int, StageRewardData> Parse(TextAsset csvData)
    {
        // CSV 파일이 비어있을 때
        if (csvData == null)
        {
            Debug.Log("CSV 파일이 연결되지 않았습니다."); 
            return null;
        }

        // 결과 데이터를 담을 임시 딕셔너리
        Dictionary<int, StageRewardData> parsedRewardData = new Dictionary<int, StageRewardData>();
        
        // 줄 개행을 기준으로 분리
        string[] lines = csvData.text.Split('\n');
        // 줄 수 만큼 반복
        // 첫 줄은 데이터 이름이기에 제외
        for (int i = 1; i < lines.Length; i++)
        {
            // string.Trim() 함수를 통해 공백 제거
            string line = lines[i].Trim();
            // 문자열이 비었는지, 공백으로만 이루어져있는지 확인
            if (string.IsNullOrWhiteSpace(line)) continue;
            // ','를 기준으로 분리
            string[] row = line.Split(',');
            // 재화 지급을 위한 최소 데이터들이 잘 들어있는지 확인
            // stageId, resourceId, amount
            if (row.Length >= 3)
            {
                // 문자열 데이터들 형변환 및 공백 제거
                // stageId = int, amoauntValue = float
                if (int.TryParse(row[0].Trim(), out int stageId) &&
                    float.TryParse(row[2].Trim(), out float amountValue))
                {
                    // resourceId를 받아오며 공백 제거
                    string resourceId = row[1].Trim();
                    // 보상 제공을 위한 임시 인터페이스 객체
                    IReward reward;

                    // 재화 ID인지 확인
                    // resourceId 문자열 재화 타입으로 형변환 시도
                    if (Enum.TryParse(resourceId, true, out CurrencyType type))
                    {
                        // 재화 보상 클래스를 통한 재화 지급
                        reward = new CurrencyReward(type, amountValue);
                    }
                    // 아이템 지급용 ID인지 확인
                    else if (IsEquipbox(resourceId))
                    {
                        // 아이템 보상 클래스를 통한 아이템 지급
                        reward = new ItemReward(resourceId, amountValue);
                    }
                    else
                    {
                        Debug.Log($"{resourceId} 라는 보상은 없습니다.");
                        continue;
                    }

                    // 반환용 딕셔너리에 Key값으로 stageuId값이 존재하지 않는다면
                    if (!parsedRewardData.ContainsKey(stageId))
                    {
                        // 해당 stageId값을 Key값으로 갖는 보상 데이터 생성
                        parsedRewardData[stageId] = new StageRewardData();
                    }

                    // stageId 별로 보상 데이터 저장
                    parsedRewardData[stageId].GetReward(reward);
                }
                // stageId, amountValue가 정해진 형식과 다른 경우
                else
                {
                    Debug.Log("파싱에 실패했습니다.");
                }
            }
        }
        // 결과 딕셔너리 반환
        return parsedRewardData;
    }
    // 장비 지급 ID인지 확인용(예외 처리용) 함수
    // 보상 테이블 안에 실제 보상 ID가 존재하지 않아도 되기에 장비를 준다 라는 비교만 하면 되기에 문자열 비교로 처리.
    private static bool IsEquipbox(string resourceId)
    {
        return resourceId == eqipboxId;
    }
}