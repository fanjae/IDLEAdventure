using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class HeroDetailViewSpawner : MonoBehaviour
{
    [SerializeField] private Transform viewRoot;
    [SerializeField] private List<HeroViewEntry> heroViews = new();

    private GameObject currentView;

    public void Show(string heroId)
    {
        Clear();

        if (string.IsNullOrEmpty(heroId))
        {
            return;
        }

        GameObject prefab = FindPrefab(heroId);

        if (prefab == null)
        {
            Debug.LogWarning($"[HeroDetailViewSpawner] Hero View를 찾을 수 없습니다. HeroId: {heroId}");
            return;
        }

        if (viewRoot == null)
        {
            Debug.LogWarning("[HeroDetailViewSpawner] ViewRoot가 지정되지 않았습니다.");
            return;
        }

        currentView = Instantiate(prefab, viewRoot);

        DisableBattleComponents(currentView);
        ResetTransform(currentView.transform);
        PlayIdle(currentView);
    }

    public void Clear()
    {
        if (currentView == null)
        {
            return;
        }

        Destroy(currentView);
        currentView = null;
    }

    private GameObject FindPrefab(string heroId)
    {
        foreach (HeroViewEntry entry in heroViews)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.HeroId == heroId)
            {
                return entry.Prefab;
            }
        }

        return null;
    }

    private void DisableBattleComponents(GameObject view)
    {
        BattleUnit battleUnit = view.GetComponent<BattleUnit>();
        UnitMovement movement = view.GetComponent<UnitMovement>();
        UnitAttack attack = view.GetComponent<UnitAttack>();
        UnitSkill skill = view.GetComponent<UnitSkill>();

        if (battleUnit != null)
        {
            battleUnit.enabled = false;
        }

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (attack != null)
        {
            attack.enabled = false;
        }

        if (skill != null)
        {
            skill.enabled = false;
        }
    }

    private void ResetTransform(Transform viewTransform)
    {
        viewTransform.localPosition = Vector3.zero;
        viewTransform.localRotation = Quaternion.identity;
        viewTransform.localScale = Vector3.one;
    }

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

[Serializable]
public sealed class HeroViewEntry
{
    [SerializeField] private string heroId;
    [SerializeField] private GameObject prefab;

    public string HeroId => heroId;
    public GameObject Prefab => prefab;
}