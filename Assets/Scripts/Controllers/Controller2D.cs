using UnityEngine;
//Resharper disable All

public class Controller2D : RaycastController
{
    #region Variables & Constants
    public LayerMask interactablesMask;
    public CollisionData collisionData;

    [Header("SLOPE")]
    public float maxSlopeAngle = 80f;

    Vector4 playerInput;
    private bool isPushable;

    #endregion

    protected override void Start()
    {
        base.Start();
        collisionData.faceDir = 1;

        if (gameObject.CompareTag("Pushable"))
        {
            isPushable = true;
        }
    }

    #region Methods

    #region Overload Methods
    public Vector2 Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        return Move(moveAmount, Vector4.zero, standingOnPlatform);
    }
    #endregion

    #region Movement Methods
    public Vector2 Move(Vector2 moveAmount, Vector4 _input, bool standingOnPlatform = false)
    {
        playerInput = _input;

        if (playerInput.z != collisionData.crouchInpOld)
        {
            if (playerInput.z == 1f)
            {
                collider.size = new Vector2(1f, .5f);
                collider.offset = new Vector2(0f, -.25f);
            }
            else
            {
                collider.size = colliderSize;
                collider.offset = Vector2.zero;
            }
            
            CalculateRaySpacing();
        }

        UpdateRaycastOrigins();

        collisionData.Reset();
        collisionData.moveAmountOld = moveAmount;
        collisionData.crouchInpOld = playerInput.z;

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        if (moveAmount.x != 0)
        {
            collisionData.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        HorizontalCollisions(ref moveAmount);
        // if (!isPushable)
        // {
        //     InteractablesCollisions(ref moveAmount);
        // }
        
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount);

        if (standingOnPlatform)
        {
            collisionData.below = true;
        }

        return moveAmount;
    }

    #endregion

    #region Collision Methods
    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisionData.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collisionData.isDescendingSlope)
                    {
                        collisionData.isDescendingSlope = false;
                        moveAmount = collisionData.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionData.previousSlopeAngle)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                if (!collisionData.isClimbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisionData.isClimbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisionData.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }

                    collisionData.left = directionX == -1;
                    collisionData.right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {

            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Platform Effector")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (collisionData.fallingThroughPlatform)
                    {
                        continue;
                    }
                    if (playerInput.z == 1f)
                    {
                        collisionData.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .1f);
                        continue;
                    }
                }

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisionData.isClimbingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisionData.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }

                collisionData.below = directionY == -1;
                collisionData.above = directionY == 1;
            }
        }

        if (collisionData.isClimbingSlope)
        {
            float directionX = collisionData.faceDir;
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisionData.slopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    collisionData.slopeAngle = slopeAngle;
                    collisionData.slopeNormal = hit.normal;
                }
            }
        }
    }

    void InteractablesCollisions(ref Vector2 moveAmount)
    {
        #region Horizontal Collision
        float originalMoveAmountX = moveAmount.x;
        Collider2D otherCollider = null;

        float directionX = collisionData.faceDir;
        float rayLengthModifier = 40f;
        float rayLengthX = Mathf.Abs(moveAmount.x) + skinWidth * rayLengthModifier;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
        {
            rayLengthX = 2 * skinWidth * rayLengthModifier;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin;
            if (!collisionData.isPushingObject)
            {
                rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            }
            else
            {
                rayOrigin = (transform.localScale.x == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            }

            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            
            RaycastHit2D hit;
            if (!collisionData.isPushingObject)
            {
                hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLengthX, interactablesMask);
            }
            else
            {
                hit = Physics2D.Raycast(rayOrigin, Vector2.right * transform.localScale.x, rayLengthX, interactablesMask);
            }

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLengthX, Color.yellow);
            
            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                otherCollider = hit.collider;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collisionData.isDescendingSlope)
                    {
                        collisionData.isDescendingSlope = false;
                        moveAmount = collisionData.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionData.previousSlopeAngle)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                if (!collisionData.isClimbingSlope || slopeAngle > maxSlopeAngle)
                {
                    if (hit.distance <= 0.1f && otherCollider.CompareTag("Pushable"))
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                    }
                    rayLengthX = hit.distance;
                
                    if (collisionData.isClimbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisionData.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }
                }
            }
            
            if (otherCollider != null)
            {
                if (otherCollider.CompareTag("Pushable"))
                {
                    if (hit.distance <= 0.1f)
                    {
                        collisionData.canInteract = true;
                    }
                    else
                    {
                        collisionData.canInteract = false;
                    }
                }
                else
                {
                    collisionData.canInteract = true;
                }
            }
            else
            {
                collisionData.canInteract = false;
            }
            
            if (collisionData.canInteract && otherCollider.CompareTag("Pushable"))
            {
                collisionData.canPushObject = true;
                collisionData.left = directionX == -1;
                collisionData.right = directionX == 1;
            }
            else
            {
                collisionData.canPushObject = false;
            }
        }
        #endregion
        
        #region Interactions
        if (collisionData.canInteract)
        {
            if (playerInput.w == 1f)
            {
                collisionData.isInteracting = true;
            }
            else
            {
                collisionData.isInteracting = false;
            }
            
            if(collisionData.canPushObject)
            {
                if (collisionData.isInteracting)
                {
                    collisionData.isPushingObject = true;
                    Vector2 pushAmount = otherCollider.gameObject.GetComponent<PushableObject>().Push(new Vector2(originalMoveAmountX, 0f));
                    moveAmount = new Vector2(pushAmount.x, moveAmount.y + pushAmount.y);
                    collisionData.left = false;
                    collisionData.right = false;
                }
                else
                {
                    collisionData.isPushingObject = false;
                }
            }
        }
        else
        {
            collisionData.isInteracting = false;
            collisionData.isPushingObject = false;
        }
        #endregion

        #region Vertical Collision
        if (moveAmount.y != 0f)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLengthY = Mathf.Abs(moveAmount.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLengthY, interactablesMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLengthY, Color.yellow);

                if (hit)
                {
                    if (hit.collider.CompareTag("Pushable"))
                    {
                        moveAmount.y = (hit.distance - skinWidth) * directionY;
                        rayLengthY = hit.distance;
    
                        if (collisionData.isClimbingSlope)
                        {
                            moveAmount.x = moveAmount.y / Mathf.Tan(collisionData.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                        }

                    
                        collisionData.below = directionY == -1;
                        collisionData.above = directionY == 1;
                    }
                }
            }

            if (collisionData.isClimbingSlope)
            {
                rayLengthY = Mathf.Abs(moveAmount.x) + skinWidth;
                Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLengthY, interactablesMask);

                if (hit)
                {
                    if (hit.collider.CompareTag("Pushable"))
                    {
                        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                        if (slopeAngle != collisionData.slopeAngle)
                        {
                            moveAmount.x = (hit.distance - skinWidth) * directionX;
                            collisionData.slopeAngle = slopeAngle;
                            collisionData.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }

        #endregion
    }

    private void ResetFallingThroughPlatform()
    {
        collisionData.fallingThroughPlatform = false;
    }

    #endregion

    #region Slopes
    private void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collisionData.below = true;
            collisionData.isClimbingSlope = true;
            collisionData.slopeAngle = slopeAngle;
            collisionData.slopeNormal = slopeNormal;
        }
    }

    private void DescendSlope(ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!collisionData.isSlidingDownSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0f && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            collisionData.slopeAngle = slopeAngle;
                            collisionData.isDescendingSlope = true;
                            collisionData.below = true;
                            collisionData.slopeNormal = hit.normal;
                        }
                    }
                }
            }

        }
    }

    private void SlideDownSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = hit.normal.x * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                collisionData.slopeAngle = slopeAngle;
                collisionData.isSlidingDownSlope = true;
                collisionData.slopeNormal = hit.normal;
            }
        }
    }

    #endregion
    #endregion

    #region Structs
    public struct CollisionData
    {
        public bool above, below;
        public bool left, right;

        public bool isClimbingSlope;
        public bool isDescendingSlope;
        public bool isSlidingDownSlope;

        public bool canInteract;
        public bool isInteracting;
        
        public bool canPushObject;
        public bool isPushingObject;

        public float slopeAngle, previousSlopeAngle;
        public Vector2 slopeNormal;

        public int faceDir;
        public bool fallingThroughPlatform;

        public Vector2 moveAmountOld;
        public float crouchInpOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            isClimbingSlope = false;
            isDescendingSlope = false;
            isSlidingDownSlope = false;

            slopeNormal = Vector2.zero;
            previousSlopeAngle = slopeAngle;
            slopeAngle = 0f;
        }
    }
    #endregion
}
