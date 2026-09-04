using UnityEngine;
using UnityEngine.UI;

// UI 버튼 클릭 사운드 재생 처리
[RequireComponent(typeof(Button))]
public sealed class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSfx;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(PlayClickSound);
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(PlayClickSound);
    }

    // 버튼 클릭 사운드 재생
    private void PlayClickSound()
    {
        if (clickSfx == null || SoundManager.Instance == null)
        {
            return;
        }

        SoundManager.Instance.PlaySfx(clickSfx);
    }
}