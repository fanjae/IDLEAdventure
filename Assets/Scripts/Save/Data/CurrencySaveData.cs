using System;

// 재화 관련 저장 데이터
/* Newtonsoft.Json을 사용하고 있어, Dictionary 직렬화는 가능함.
 * 다만 CurrencyType이 변경되도, 저장 호환성이 깨지지 않도록 DTO 형태로 처리.
 */
[Serializable]
public sealed class CurrencySaveData
{
    public int Gold { get; set; }
    public int Exp { get; set; }
    public int Upgrade { get; set; }
    public int Diamond { get; set; }
}