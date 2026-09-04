using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleIntro : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RawImage blackOverlay;
    [SerializeField] private GameObject closeEye;
    [SerializeField] private GameObject openEye;
    [SerializeField] private Camera mainCamera;


    [Header("시작 설정")]
    [SerializeField] private float revealDuration = 1f;
    [SerializeField] private float maxReveal = 0.55f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("잠깐 멈추는 시간")]
    [SerializeField] private float firstCameraMoveDuration = 0.3f;
    [SerializeField] private float secondCameraMoveDuration = 0.7f;

    [SerializeField] private float characterDelay = 1f;

    [Header("카메라 위치")]
    [SerializeField] private Vector3 cameraStartPos = new Vector3(-0.2f, 1f, -0.8f);
    [SerializeField] private Vector3 firstCameraTargetPos = new Vector3(0f, 1f, -3.2f);
    [SerializeField] private Vector3 secondCameraTargetPos = new Vector3(0f, 1f, -10f);


    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private Animator archerAnimator;
    [SerializeField] private Animator healerAnimator;

    [Header("궁수")]
    [SerializeField] private Transform archerCharacter;
    [SerializeField] private float archerDropHeight = 8f;

    [Header("힐러")]
    [SerializeField] private GameObject healerCharacter;
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private float healerDelay = 0.25f;

    [Header("비행 몬스터")]
    [SerializeField] private Transform flyingMonster;
    [SerializeField] private float monsterDelay = 0.5f;
    [SerializeField] private float monsterMoveDuration = 2f;
    [SerializeField] private Vector3 monsterStartPos = new Vector3(-16f, 3f, -12f);
    [SerializeField] private Vector3 monsterEndPos = new Vector3(16f, 3f, -12f);

    private Vector3 archerTargetPosition;


    private Material material;

    private readonly int RevealID = Shader.PropertyToID("_Reveal");
    private readonly int GlobalFadeID = Shader.PropertyToID("_GlobalFade");

    private void Start()
    {
        material = blackOverlay.material;

        mainCamera.transform.position = cameraStartPos;

        closeEye.SetActive(true);
        openEye.SetActive(false);

        archerTargetPosition = archerCharacter.position;
        archerCharacter.position += Vector3.up * archerDropHeight;

        flyingMonster.position = monsterStartPos;

        healerCharacter.SetActive(false);

        material.SetFloat(RevealID, 0f);
        material.SetFloat(GlobalFadeID, 0f);

        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        float time = 0f;

        while (time < revealDuration)
        {
            time += Time.deltaTime;
            material.SetFloat(RevealID, Mathf.Lerp(0f, maxReveal, time / revealDuration));
            yield return null;
        }

        material.SetFloat(RevealID, maxReveal);

        closeEye.SetActive(false);
        openEye.SetActive(true);

        yield return new WaitForSeconds(firstCameraMoveDuration);

        time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            material.SetFloat(GlobalFadeID, Mathf.Lerp(0f, 1f, t));
            mainCamera.transform.position = Vector3.Lerp(cameraStartPos, firstCameraTargetPos, t);

            yield return null;
        }

        material.SetFloat(GlobalFadeID, 1f);
        animator.Play("attack1", 0, 0f);
        mainCamera.transform.position = firstCameraTargetPos;

        yield return new WaitForSeconds(characterDelay);

        smokeEffect.Play();

        time = 0f;
        bool healerAppeared = false;

        archerAnimator.Play("Jump", 0, 0f);

        while (time < secondCameraMoveDuration)
        {
            time += Time.deltaTime;
            float t = time / secondCameraMoveDuration;

            mainCamera.transform.position = Vector3.Lerp(firstCameraTargetPos, secondCameraTargetPos, t);
            archerCharacter.position = Vector3.Lerp(archerTargetPosition + Vector3.up * archerDropHeight, archerTargetPosition, t);

            if (!healerAppeared && time >= healerDelay)
            {
                healerAppeared = true;
                healerCharacter.SetActive(true);
            }

            yield return null;
        }

        mainCamera.transform.position = secondCameraTargetPos;
        archerCharacter.position = archerTargetPosition;

        while (time < monsterMoveDuration)
        {
            time += Time.deltaTime;
            flyingMonster.position = Vector3.Lerp(monsterStartPos, monsterEndPos, time / monsterMoveDuration);

            yield return null;
        }

        flyingMonster.position = monsterEndPos;
    }
}