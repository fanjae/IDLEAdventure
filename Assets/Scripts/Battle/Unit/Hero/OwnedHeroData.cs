public sealed class OwnedHeroData
{
    // 보유 영웅의 원본 데이터
    public HeroData HeroData { get; }

    // 현재 영웅 레벨
    public int Level { get; private set; }

    // 원본 영웅 데이터의 UnitID 반환
    public string HeroId => HeroData.UnitID;

    // 영웅 원본 데이터와 초기 레벨을 기준으로 보유 영웅 데이터 생성
    public OwnedHeroData(HeroData heroData, int level = 1)
    {
        HeroData = heroData;
        Level = level;
    }
    
    // 보유 영웅의 현재 레벨 변경
    public void SetLevel(int level)
    {
        Level = level;
    }
}