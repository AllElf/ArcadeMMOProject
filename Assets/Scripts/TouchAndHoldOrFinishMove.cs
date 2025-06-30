using UnityEngine;
using UnityEngine.EventSystems;

public class TouchAndHoldOrFinishMove : MonoBehaviour
{
    [SerializeField] GameObject eart;
    public Camera mainCamera;
    public bool useLocalCoordinates = false;
    public float moveSpeed = 5f;
    public float movementRadius = 3f;

    [Header("Режим движения")]
    public bool continuousMove = true; // true = движется постоянно, false = до последней заданной точки
    [SerializeField] GameObject buttonTouchMove;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 originPosition;
    private bool directionLocked = false;
    private Vector3 lastTargetPos;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        originPosition = useLocalCoordinates && eart.transform.parent != null
            ? eart.transform.localPosition
            : eart.transform.position;
    }
    public void TouchMoves()
    {
        continuousMove = !continuousMove;
        if(continuousMove && buttonTouchMove != null) { buttonTouchMove.SetActive(true); }
        if (!continuousMove && buttonTouchMove != null) { buttonTouchMove.SetActive(false); }
    }
    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        bool isInputActive = false;
        Vector2 screenPos = Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButton(0))
        {
            screenPos = Input.mousePosition;
            isInputActive = true;
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            screenPos = Input.touches[0].position;
            isInputActive = true;
        }
#endif

        if (isInputActive)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = eart.transform.position.z;

            Vector3 targetPos = useLocalCoordinates && eart.transform.parent != null
                ? eart.transform.parent.InverseTransformPoint(worldPos)
                : worldPos;

            Vector3 current = GetCurrentPosition();
            moveDirection = (targetPos - current).normalized;
            lastTargetPos = targetPos;
            directionLocked = true;
        }
        else if (!continuousMove && HasReachedTarget())
        {
            directionLocked = false;
        }

        MoveInDirection();
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

    private bool HasReachedTarget()
    {
        Vector3 current = GetCurrentPosition();
        return (lastTargetPos - current).magnitude < 0.05f;
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
}