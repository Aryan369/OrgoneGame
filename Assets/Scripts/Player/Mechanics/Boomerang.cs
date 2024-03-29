using UnityEngine;

public class Boomerang : RaycastController
{
    #region Variables & Constants
    [Header("MOVEMENT")]
    public float speed = 13f;
    public float distance = 13f;
    public float waitTime = .15f;
    [Range(0f, 15f)] public float easeAmount = 1.5f;

    [HideInInspector] public bool isBoomeranging;
    [HideInInspector] public bool isReturning;
    [HideInInspector] public bool onReturn;

    private float distanceX;
    private float distanceY;

    private float percentBetweenPoints;
    private float nextMoveTime;

    private Player player;
    private Transform _player;
    private Vector3 startPoint;
    private Vector3 endPoint;

    private SpriteRenderer sr;
    private TrailRenderer tr;

    #endregion

    protected override void Start()
    {
        base.Start();
        player = Player.Instance;
        _player = player.transform;

        sr = GetComponent<SpriteRenderer>();
        tr = GetComponent<TrailRenderer>();
        
        DeactivateBoomerang();
    }

    void FixedUpdate()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculateBoomerangMovement();

        CollisionCheck(velocity);

        transform.Translate(velocity);
    }

    #region Methods
    public void ActivateBoomerang(Vector2 _direction)
    {
        Vector2 directionalInput = _direction;
        transform.position = _player.position;
        startPoint = transform.position;
        distanceX = distance * directionalInput.x;
        distanceY = distance * directionalInput.y;
        endPoint = new Vector3(transform.position.x + distanceX, transform.position.y + distanceY);
        sr.enabled = true;
        tr.enabled = true;
        collider.enabled = true;
        isBoomeranging = true;
    }

    void DeactivateBoomerang()
    {
        sr.enabled = false;
        tr.enabled = false;
        collider.enabled = false;
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculateBoomerangMovement()
    {
        if (isBoomeranging)
        {
            if (Time.time < nextMoveTime)
            {
                return Vector3.zero;
            }

            float distanceBetweenPoints = Vector3.Distance(startPoint, endPoint);
            percentBetweenPoints += Time.deltaTime * speed / distanceBetweenPoints;
            percentBetweenPoints = Mathf.Clamp01(percentBetweenPoints);

            float easedPercentBetweenPoints = Ease(percentBetweenPoints);

            Vector3 newPos = Vector3.Lerp(startPoint, endPoint, easedPercentBetweenPoints);

            if (percentBetweenPoints >= 1)
            {
                if (!isReturning)
                {
                    percentBetweenPoints = 0;
                    isReturning = true;
                    onReturn = true;
                    startPoint = endPoint;
                    endPoint = _player.position;
                    nextMoveTime = Time.time + waitTime;
                }
                else
                {
                    DeactivateBoomerang();
                    percentBetweenPoints = 0f;
                    isBoomeranging = false;
                    isReturning = false;
                    onReturn = false;
                    player.isBoomeranging = isBoomeranging;
                }
            }

            if (onReturn)
            {
                endPoint = _player.position;
            }

            return newPos - transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    void CollisionCheck(Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //Vertically Moving
        if (velocity.y != 0f)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                Physics2D.queriesStartInColliders = false;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                if (hit)
                {
                    if (!isReturning && !hit.collider.CompareTag("Player"))
                    {
                        percentBetweenPoints = 0.2f;
                        isReturning = true;
                        startPoint = transform.position;
                        endPoint = _player.position;
                        onReturn = true;
                        nextMoveTime = Time.time;
                    }
                }
            }
        }

        //Horizontally Moving 
        if (velocity.x != 0f)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                Physics2D.queriesStartInColliders = false;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    if (!isReturning && !hit.collider.CompareTag("Player"))
                    {
                        percentBetweenPoints = 0.2f;
                        isReturning = true;
                        startPoint = transform.position;
                        endPoint = _player.position;
                        onReturn = true;
                        nextMoveTime = Time.time;
                    }
                }
            }
        }
    }
    #endregion
}
