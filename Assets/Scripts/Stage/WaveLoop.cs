using UnityEngine;

public class WaveLoop : MonoBehaviour
{
    [Header("위 물결")]
    [SerializeField] private RectTransform topWave1;
    [SerializeField] private RectTransform topWave2;

    [Header("아래 물결")]
    [SerializeField] private RectTransform bottomWave1;
    [SerializeField] private RectTransform bottomWave2;

    [Header("이동")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float overlap = 3f;

    private float topStep;
    private float bottomStep;

    private void Start()
    {
        topStep = topWave1.rect.width - overlap;
        bottomStep = bottomWave1.rect.width - overlap;

        topWave1.anchoredPosition = new Vector2(0f, topWave1.anchoredPosition.y);
        topWave2.anchoredPosition = new Vector2(topStep, topWave2.anchoredPosition.y);

        bottomWave1.anchoredPosition = new Vector2(0f, bottomWave1.anchoredPosition.y);
        bottomWave2.anchoredPosition = new Vector2(bottomStep, bottomWave2.anchoredPosition.y);
    }

    private void Update()
    {
        MoveWave(topWave1, topWave2, topStep);
        MoveWave(topWave2, topWave1, topStep);

        MoveWave(bottomWave1, bottomWave2, bottomStep);
        MoveWave(bottomWave2, bottomWave1, bottomStep);
    }

    private void MoveWave(RectTransform wave, RectTransform otherWave, float step)
    {
        wave.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;

        if (wave.anchoredPosition.x <= -step) wave.anchoredPosition = new Vector2(otherWave.anchoredPosition.x + step, wave.anchoredPosition.y);
    }
}