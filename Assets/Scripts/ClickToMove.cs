using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Управление объектом с джойстиком в заданном радиусе и с возможностью поворота индикатора.
/// </summary>
public class ClickAndHoldOrFinishMove : MonoBehaviour
{
    [SerializeField] GameObject eart;                     // 🎯 Объект, который двигается
    [SerializeField] FloatingJoystick joystick;           // 🎮 Джойстик для ввода
    public Camera mainCamera;                             // 📷 Камера
    public bool useLocalCoordinates = false;              // 🧭 Координатное пространство
    public float moveSpeed = 5f;                          // 🚀 Скорость
    public float movementRadius = 3f;                     // 🌀 Радиус ограничений
    [SerializeField] private bool continuousMovement = false; // 🔁 Режим постоянного движения
    [SerializeField] GameObject buttonMove;               // UI кнопка
    [SerializeField] private RectTransform rotatingIndicator; // 🎯 Индикатор направления (UI)

    private Vector3 moveDirection = Vector3.zero;         // ➡️ Текущее направление
    private Vector3 originPosition;                       // 🏁 Исходная точка
    private bool directionLocked = false;                 // 🔒 Закреплено ли движение

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        originPosition = useLocalCoordinates && eart.transform.parent != null
            ? eart.transform.localPosition
            : eart.transform.position;
    }

    private void Update()
    {
        Vector2 joystickDir = joystick.Direction;

        if (joystickDir.magnitude > 0.05f)
        {
            moveDirection = new Vector3(joystickDir.x, joystickDir.y, 0f).normalized;

            if (!continuousMovement)
                directionLocked = true;

            // 🔄 Вращаем индикатор
            if (rotatingIndicator != null)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                rotatingIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        else
        {
            if (!continuousMovement)
                directionLocked = false;
        }

        // В режиме постоянного движения — зафиксировать направление после первого движения
        if (continuousMovement && !directionLocked && moveDirection != Vector3.zero)
        {
            directionLocked = true;
        }

        MoveInDirection();
        UpdateButton();
    }

    private void MoveInDirection()
    {
        if (!directionLocked || moveDirection == Vector3.zero) return;

        Vector3 current = GetCurrentPosition();
        Vector3 origin = originPosition;
        Vector3 nextPos = current + moveDirection * moveSpeed * Time.deltaTime;

        Vector3 offset = nextPos - origin;
        if (offset.magnitude > movementRadius)
        {
            nextPos = origin + offset.normalized * movementRadius;
        }

        SetCurrentPosition(nextPos);
    }

    private Vector3 GetCurrentPosition()
    {
        return useLocalCoordinates ? eart.transform.localPosition : eart.transform.position;
    }

    private void SetCurrentPosition(Vector3 pos)
    {
        if (useLocalCoordinates && eart.transform.parent != null)
            eart.transform.localPosition = new Vector3(pos.x, pos.y, eart.transform.localPosition.z);
        else
            eart.transform.position = new Vector3(pos.x, pos.y, eart.transform.position.z);
    }

    private void OnDrawGizmos()
    {
        if (eart == null) return;

        Vector3 center = useLocalCoordinates && eart.transform.parent != null
            ? eart.transform.parent.TransformPoint(originPosition)
            : originPosition;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, movementRadius);
    }

    private void UpdateButton()
    {
        if (buttonMove != null)
            buttonMove.SetActive(continuousMovement);
    }

    public void ToggleMovementMode()
    {
        continuousMovement = !continuousMovement;
    }
}