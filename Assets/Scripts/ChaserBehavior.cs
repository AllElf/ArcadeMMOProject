using UnityEngine;

public class ChaserBehavior : MonoBehaviour
{
    [Header("Преследование")]
    public float detectionRadius = 5f;
    public float chaseAcceleration = 2f;
    public float maxChaseSpeed = 2f;

    [Header("Блуждание")]
    public float wanderRadius = 3f;
    public float wanderInterval = 2f;
    public float wanderAcceleration = 1f;
    public float maxWanderSpeed = 1.2f;

    [Header("Плавность")]
    public float directionSmoothness = 3f;

    [Header("Цель (Eart)")]
    [SerializeField] private Transform targetEart;

    private Vector2 velocity;
    private Vector2 currentTarget;
    private Vector2 startPosition;
    private Vector2 movementDirection;

    private float nextWanderTime = 0f;
    private bool returningHome = false;

    private void Start()
    {
        startPosition = transform.position;
        currentTarget = startPosition;
        movementDirection = Random.insideUnitCircle.normalized;

        if (targetEart == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Eart");
            if (found != null)
                targetEart = found.transform;
        }

        nextWanderTime = Time.time + Random.Range(0f, wanderInterval);
    }

    private void Update()
    {
        Vector2 currentPos = transform.position;
        bool hasTarget = targetEart != null;

        bool eartInRange = false;

        if (hasTarget)
        {
            float distance = Vector2.Distance(currentPos, targetEart.position);
            if (detectionRadius > 0f && distance <= detectionRadius)
                eartInRange = true;
        }

        if (eartInRange)
        {
            returningHome = false;
            currentTarget = targetEart.position;
            UpdateMovement(currentPos, currentTarget, chaseAcceleration, maxChaseSpeed);
        }
        else
        {
            if (!returningHome && Vector2.Distance(currentPos, startPosition) > 0.2f)
            {
                currentTarget = startPosition;
                UpdateMovement(currentPos, currentTarget, chaseAcceleration, maxChaseSpeed);
            }
            else
            {
                returningHome = true;

                if (Time.time >= nextWanderTime)
                {
                    ChooseWanderPoint();
                    nextWanderTime = Time.time + wanderInterval;
                }

                UpdateMovement(currentPos, currentTarget, wanderAcceleration, maxWanderSpeed);
            }
        }

        transform.position = currentPos + velocity * Time.deltaTime;
    }

    private void UpdateMovement(Vector2 currentPos, Vector2 targetPos, float acceleration, float maxSpeed)
    {
        Vector2 desiredDir = (targetPos - currentPos).normalized;
        movementDirection = Vector2.Lerp(movementDirection, desiredDir, Time.deltaTime * directionSmoothness);
        velocity += movementDirection * acceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
    }

    private void ChooseWanderPoint()
    {
        currentTarget = startPosition + Random.insideUnitCircle * wanderRadius;
    }
}