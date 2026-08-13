using UnityEngine;

public sealed class TestHeroGrantButton : MonoBehaviour
{
    [SerializeField] private string heroId;

    public void GrantHero()
    {
        if (string.IsNullOrWhiteSpace(heroId))
        {
            Debug.LogWarning("영웅 아이디 불일치", this);
            return;
        }

        if (HeroManager.Instance == null || !HeroManager.Instance.IsInitialized)
        {
            Debug.LogWarning("영웅 매니저 없음", this);
            return;
        }

        if (!HeroManager.Instance.Controller.TryAcquireHero(heroId))
        {
            return;
        }

        if (SaveManager.Instance.CurrentData != null)
        {
            SaveManager.Instance.Save();
        }
    }
}
