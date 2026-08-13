using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroPresentationCatalog", menuName = "Game Data/UI/Hero Presentation Catalog")]
public sealed class HeroPresentationCatalog : ScriptableObject
{
    [SerializeField] private Sprite unknownPortrait;
    [SerializeField] private Sprite unknownDetailIllustration;
    [SerializeField] private List<HeroPresentationEntry> entries = new();

    private Dictionary<string, HeroPresentationEntry> entryMap;

    public Sprite UnknownPortrait => unknownPortrait;
    public Sprite UnknownDetailIllustration => unknownDetailIllustration;

    public bool TryGet(string heroId, out HeroPresentationEntry entry)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(heroId))
        {
            entry = null;
            return false;
        }

        return entryMap.TryGetValue(heroId, out entry);
    }

    public Sprite GetPortrait(string heroId)
    {
        return TryGet(heroId, out HeroPresentationEntry entry) && entry.Portrait != null
            ? entry.Portrait
            : unknownPortrait;
    }

    public Sprite GetDetailIllustration(string heroId)
    {
        return TryGet(heroId, out HeroPresentationEntry entry) && entry.DetailIllustration != null
            ? entry.DetailIllustration
            : unknownDetailIllustration;
    }

    private void OnEnable()
    {
        RebuildIndex();
    }

    private void OnValidate()
    {
        RebuildIndex();
    }

    private void EnsureInitialized()
    {
        if (entryMap == null)
        {
            RebuildIndex();
        }
    }

    private void RebuildIndex()
    {
        entryMap = new Dictionary<string, HeroPresentationEntry>(StringComparer.Ordinal);

        foreach (HeroPresentationEntry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.HeroId))
            {
                continue;
            }

            if (!entryMap.TryAdd(entry.HeroId, entry))
            {
                Debug.LogWarning($"[HeroPresentationCatalog] 중복된 HeroId가 있습니다. HeroId: {entry.HeroId}", this);
            }
        }
    }
}

[Serializable]
public sealed class HeroPresentationEntry
{
    [SerializeField] private string heroId;
    [SerializeField] private Sprite portrait;
    [SerializeField] private Sprite detailIllustration;

    public string HeroId => heroId;
    public Sprite Portrait => portrait;
    public Sprite DetailIllustration => detailIllustration;
}
