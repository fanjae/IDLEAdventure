using UnityEngine;

/// <summary>
/// 카메라가 플레이어를 따라가게 하는 클래스. <br/>
/// 나중에 씨네머신으로 교체 가능성 있음. 
/// 씨네머신은 안 써봐서 써보고싶기 때문.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [Header("Move Setting")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        if (offset == Vector3.zero && target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targePosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, targePosition, moveSpeed * Time.deltaTime);
    }
}