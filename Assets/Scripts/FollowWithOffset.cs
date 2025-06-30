using UnityEngine;

public class FollowWithOffset : MonoBehaviour
{
    [Header("Объекты")]
    public Transform follower;
    public Transform target;

    [Header("Настройки слежения")]
    public bool useLocalSpace = false;
    public float followRadius = 2f;
    public float smoothTime = 0.3f;
    public float velocityThreshold = 0.01f;

    [SerializeField] private AbsorptionHandler absorptionHandler;

    private Vector3 initialOffset;
    private Vector3 velocity = Vector3.zero;
    private Vector3 previousTargetPos;

    private bool useLocalWithSameParent;

    private const float MinDeltaTime = 0.0001f;
    private const float MaxDeltaTime = 0.05f;

    private void Start()
    {
        if (follower == null || target == null)
        {
            enabled = false;
            return;
        }

        previousTargetPos = target.position;

        useLocalWithSameParent =
            useLocalSpace &&
            follower.parent != null &&
            target.parent != null &&
            follower.parent == target.parent;

        // Сохраняем полный offset, включая Z
        if (useLocalWithSameParent)
            initialOffset = follower.localPosition - target.localPosition;
        else
            initialOffset = follower.position - target.position;
    }

    private void LateUpdate()
    {
        if (follower == null || target == null)
            return;

        float dt = Mathf.Clamp(Time.deltaTime, MinDeltaTime, MaxDeltaTime);
        Vector3 targetVelocity = (target.position - previousTargetPos) / dt;
        previousTargetPos = target.position;

        bool shouldCenter = targetVelocity.magnitude < velocityThreshold;

        if (useLocalWithSameParent)
        {
            Vector3 desired = target.localPosition + initialOffset;

            float dx = follower.localPosition.x - desired.x;
            float dy = follower.localPosition.y - desired.y;
            float distSqr = dx * dx + dy * dy;

            if (shouldCenter || distSqr > followRadius * followRadius)
            {
                if (distSqr > followRadius * followRadius * 16f)
                    velocity = Vector3.zero;

                if (distSqr < 0.0001f)
                {
                    follower.localPosition = desired;
                    velocity = Vector3.zero;
                    return;
                }

                follower.localPosition = Vector3.SmoothDamp(
                    follower.localPosition,
                    desired,
                    ref velocity,
                    smoothTime
                );
            }
        }
        else
        {
            Vector3 desired = target.position + initialOffset;

            float dx = follower.position.x - desired.x;
            float dy = follower.position.y - desired.y;
            float distSqr = dx * dx + dy * dy;

            if (shouldCenter || distSqr > followRadius * followRadius)
            {
                if (distSqr > followRadius * followRadius * 16f)
                    velocity = Vector3.zero;

                if (distSqr < 0.0001f)
                {
                    follower.position = desired;
                    velocity = Vector3.zero;
                    return;
                }

                follower.position = Vector3.SmoothDamp(
                    follower.position,
                    desired,
                    ref velocity,
                    smoothTime
                );
            }
        }
    }
}