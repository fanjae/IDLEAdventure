using System;
using System.Collections;
using UnityEngine;

public sealed class TitleCharacterEntryController : MonoBehaviour
{
    [Serializable]
    private sealed class CharacterEntry
    {
        public Transform character;
        public Animator animator;

        [Header("등장 시간")]
        public float startDelay;

        [HideInInspector]
        public Vector3 targetPosition;

        [HideInInspector]
        public Quaternion targetRotation;
    }

    [Header("캐릭터")]
    [SerializeField]
    private CharacterEntry[] characters;

    [Header("이동")]
    [SerializeField]
    private float startOffsetX = -12f;

    [SerializeField]
    private float moveSpeed = 4f;

    [SerializeField]
    private float arrivalDistance = 0.02f;

    [Header("달리는 방향")]
    [SerializeField] private float runRotationOffsetY = 0f;

    [Header("애니메이션 State 이름")]
    [SerializeField] private string runStateName = "Run";

    [SerializeField] private string idleStateName = "Idle";


    private void Start()
    {
        PrepareCharacters();

        foreach (CharacterEntry entry in characters)
        {
            StartCoroutine(EnterCharacter(entry));
        }
    }


    private void PrepareCharacters()
    {
        foreach (CharacterEntry entry in characters)
        {
            if (entry.character == null)
            {
                continue;
            }
            entry.targetPosition = entry.character.position;

            entry.targetRotation = entry.character.rotation;

            Vector3 startPosition = entry.targetPosition;

            startPosition.x += startOffsetX;

            entry.character.position = startPosition;

            Vector3 moveDirection = entry.targetPosition - startPosition;

            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 0f)
            {
                entry.character.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0f, runRotationOffsetY, 0f);
            }
        }
    }


    private IEnumerator EnterCharacter(CharacterEntry entry)
    {
        if (entry.character == null || entry.animator == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(entry.startDelay);

        // 달리기 시작
        entry.animator.CrossFade(runStateName, 0.1f);


        while (Vector3.Distance(entry.character.position, entry.targetPosition) > arrivalDistance)
        {
            entry.character.position = Vector3.MoveTowards(entry.character.position, entry.targetPosition, moveSpeed * Time.deltaTime);

            yield return null;
        }


        entry.character.position = entry.targetPosition;

        entry.character.rotation = entry.targetRotation;

        entry.animator.CrossFade(idleStateName, 0.1f);
    }
}