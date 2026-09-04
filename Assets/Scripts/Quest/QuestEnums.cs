using UnityEngine;

/// <summary>
/// 퀘스트 종류를 정의하기 위한 열거형. <br/>
/// 현재 퀘스트 종류 <br/>
/// Main, Sub
/// </summary>
public enum QuestType
{ 
    None,
    Main, Sub,
    Length
}
/// <summary>
/// 퀘스트 진행 방식을 정의하기 위한 열거형. <br/>
/// 현재 퀘스트 종류 <br/>
/// Talk, Fight, Gather
/// </summary>
public enum QuestKind
{
    None,
    Talk, Fight, Gather,
    Length
}