using System;
using UnityEngine;

public sealed class StageFieldLoader : MonoBehaviour
{
    [Header("필드 경로")]
    [SerializeField]
    private string fieldPrefabPath = "Prefab/Field";

    [Header("필드 생성 위치")]
    [SerializeField] private Transform fieldRoot;

    private GameObject currentField;

    private void Awake()
    {
        if (fieldRoot == null)
        {
            throw new Exception("StageFieldLoader의 Field Root가 연결되어 있지 않습니다.");
        }
    }

    public void LoadField(string fieldName)
    {
        GameObject prefab = Resources.Load<GameObject>($"{fieldPrefabPath}/{fieldName}");

        if (prefab == null)
        {
            throw new Exception($"필드 Prefab을 찾을 수 없습니다. 경로: {fieldPrefabPath}/{fieldName}");
        }

        ClearField();

        currentField = Instantiate(prefab, fieldRoot);

        currentField.transform.localPosition = Vector3.zero;
        currentField.transform.localRotation = Quaternion.identity;
        currentField.transform.localScale = Vector3.one;

        Debug.Log($"필드 로드 완료: {fieldName}");
    }

    public void ClearField()
    {
        if (currentField == null)
        {
            return;
        }

        Destroy(currentField);

        currentField = null;
    }
}