using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 업적 보상 수령 결과를 짧게 알리는 토스트 UI임
public sealed class AchievementRewardToast : MonoBehaviour
{
    [SerializeField] private GameObject toastRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Transform iconRoot;
    [SerializeField] private Image iconTemplate;
    [Min(0.1f)] [SerializeField] private float duration = 2f;

    private Coroutine hideRoutine;
    private readonly List<Image> iconViews = new();

    // 수령한 업적의 아이콘과 보상 수량을 토스트로 표시함
    public void Show(IReadOnlyList<AchievementClaimReward> rewards)
    {
        if (toastRoot == null || messageText == null || rewards == null || rewards.Count == 0)
        {
            return;
        }

        RefreshIcons(rewards);

        messageText.text = "업적 보상 획득\n" + string.Join("\n", rewards
            .Select(reward => $"+{reward.Amount}"));

        toastRoot.SetActive(true);
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    // 수령한 업적 수만큼 아이콘을 만들어 일괄 수령 결과도 구분해 보여줌
    private void RefreshIcons(IReadOnlyList<AchievementClaimReward> rewards)
    {
        if (iconRoot == null || iconTemplate == null)
        {
            return;
        }

        foreach (Image iconView in iconViews)
        {
            if (iconView != null)
            {
                Destroy(iconView.gameObject);
            }
        }

        iconViews.Clear();
        iconTemplate.gameObject.SetActive(false);

        foreach (AchievementClaimReward reward in rewards)
        {
            Image iconView = Instantiate(iconTemplate, iconRoot);
            iconView.sprite = reward.Definition != null ? reward.Definition.Icon : null;
            iconView.enabled = iconView.sprite != null;
            iconView.gameObject.SetActive(iconView.enabled);
            iconViews.Add(iconView);
        }
    }

    // 설정된 시간 뒤 토스트를 자동으로 숨김
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        toastRoot.SetActive(false);
        hideRoutine = null;
    }
}
