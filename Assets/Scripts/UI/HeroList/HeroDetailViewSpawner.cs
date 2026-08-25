using System;
using System.Collections.Generic;
using UnityEngine;

// 영웅 상세 패널에 표시할 3D 프리뷰 생성 관리
public sealed class HeroDetailViewSpawner : MonoBehaviour
{
    [SerializeField] private Transform previewSpawnPoint;
    [SerializeField] private List<HeroViewEntry> heroViews = new();

    private GameObject currentView;

    // 영웅 ID에 맞는 프리뷰 프리팹 생성
    public void Show(string heroId)
    {
        Clear();

        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        HeroViewEntry entry = FindEntry(heroId);

        if (entry == null || entry.Prefab == null)
        {
            Debug.LogWarning($"[HeroDetailViewSpawner] Hero View를 찾을 수 없습니다. HeroId: {heroId}");
            return;
        }

        if (previewSpawnPoint == null)
        {
            Debug.LogWarning("[HeroDetailViewSpawner] Preview Spawn Point가 지정되지 않았습니다.");
            return;
        }

        currentView = Instantiate(entry.Prefab, previewSpawnPoint);

        ApplyTransform(currentView.transform, entry);
        PlayIdle(currentView);
    }

    // 현재 생성된 영웅 프리뷰 제거
    public void Clear()
    {
        if (currentView == null)
        {
            return;
        }

        Destroy(currentView);
        currentView = null;
    }

    // 영웅 ID에 해당하는 프리뷰 설정 데이터 검색
    private HeroViewEntry FindEntry(string heroId)
    {
        foreach (HeroViewEntry entry in heroViews)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.HeroId == heroId)
            {
                return entry;
            }
        }

        return null;
    }

    // 영웅별 프리뷰 위치와 회전, 크기 적용
    private void ApplyTransform(Transform viewTransform, HeroViewEntry entry)
    {
        viewTransform.localPosition = entry.LocalPosition;
        viewTransform.localRotation = Quaternion.Euler(entry.LocalEulerAngles);
        viewTransform.localScale = entry.LocalScale;
    }

    // 생성한 영웅 프리뷰를 Idle 상태로 재생
    private void PlayIdle(GameObject view)
    {
        Animator animator = view.GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("[HeroDetailViewSpawner] Animator가 없습니다.");
            return;
        }

        animator.Play("Base Layer.Idle", 0, 0f);
        animator.Update(0f);
    }
}

// 영웅별 프리뷰 프리팹과 Transform 보정값 관리
[Serializable]
public sealed class HeroViewEntry
{
    [SerializeField] private string heroId;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 localPosition = Vector3.zero;
    [SerializeField] private Vector3 localEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 localScale = Vector3.one;

    public string HeroId => heroId;
    public GameObject Prefab => prefab;
    public Vector3 LocalPosition => localPosition;
    public Vector3 LocalEulerAngles => localEulerAngles;
    public Vector3 LocalScale => localScale;
}