using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 가상 패트를 이용한 이동 담당 클래스. <br/>
/// 사실상 UI 조작을 통한 움직임이기에 InputManager 등은 구현하지 않음.
/// </summary>
public class VirtualPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI Componenets")]
    [SerializeField] private RectTransform padBackground;   // 가상 패드 배경
    [SerializeField] private RectTransform padStick;        // 가상 패드 스틱

    private Vector2 inputDirection = Vector2.zero;      // 계산된 이동 방향 벡터 저장

    // 프로퍼티
    public Vector2 InputDirection => inputDirection;

    // IPointerDownHandler 인터페이스 필수 구현 함수
    // 클릭이 된 순간 호출
    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 된 순간에 바로 할 건 없고 드래그 시작만 호출
        // 해당 호출을 해주지 않으면, 가만히 클릭만 하고있으면 인식을 못할 수 있음.
        OnDrag(eventData);
    }
    // IDragHandler 인터페이스 필수 구현 함수
    // 드래그가 지속되는 동안 매 프레임 호출
    public void OnDrag(PointerEventData eventData)
    {
        // 좌표 변환 함수. 스크린 좌표 > 로컬 좌표
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            padBackground,              // 로컬 좌표의 기준이 될 객체
            eventData.position,         // 눌린 위치값
            eventData.pressEventCamera, // 기준 카메라
            out Vector2 localPosition)) // 변환된 좌표값
        {
            // 로컬 좌표값을 패드 배경 기준으로 나눠 비율 계산
            // 패드는 중앙을 기점으로 끝이 0.5이기에 *2를 해주어 1로 변환
            localPosition.x = (localPosition.x / padBackground.sizeDelta.x) * 2;
            localPosition.y = (localPosition.y / padBackground.sizeDelta.y) * 2;
            // 이동을 위한 방향 벡터에 저장
            inputDirection = new Vector2(localPosition.x, localPosition.y);
            // 패드 스틱의 속도 조절 기능을 유지하기 위해 최대 비율인 1.0을 넘어가려 할 때만 정규화 진행.
            // 무조건 최대 속도를 주고싶으면 그냥 무조건 정규화 해버리면 됨.
            inputDirection = (inputDirection.magnitude > 1.0f) ? inputDirection.normalized : inputDirection;

            // 패드가 범위를 벗어나지 않게 계산.
            float moveRadiusX = (padBackground.sizeDelta.x / 2) - (padStick.sizeDelta.x / 2);
            float moveRadiusY = (padBackground.sizeDelta.y / 2) - (padStick.sizeDelta.y / 2);

            // 스틱 UI 이동
            // 스틱의 x, y 위치 값을 패드 배경 비율에 맞춰 이동
            // 패드의 끝을 1로 계산해뒀기에 실제 스틱의 이동은 그 절반으로.
            padStick.anchoredPosition = new Vector2(
                inputDirection.x * moveRadiusX,
                inputDirection.y * moveRadiusY);
        }
    }
    // IPointerUpHandler 인터페이스 필수 구현 함수
    // 클릭이 끝나는 순간 호출
    public void OnPointerUp(PointerEventData eventData)
    {
        // 움직일 때 변동했던 값들 초기화
        inputDirection = Vector2.zero;
        padStick.anchoredPosition = Vector2.zero;
    }
}