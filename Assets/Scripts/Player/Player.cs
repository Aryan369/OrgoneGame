using System.Collections;
using UnityEngine;
// ReSharper disable All

[RequireComponent(typeof(Controller2D), typeof(PlayerInputManager))]
public class Player : MonoBehaviour
{
    #region Variables & Constants

    #region Ref
    public static Player Instance;
    
    [HideInInspector] public Controller2D controller;
    [HideInInspector] public CameraEffects cameraEffects;
    
    #endregion

    #region BASIC MOVEMENT
    #region MOVEMENT
    [Header("MOVEMENT")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 8f;
    public float accelerationGrounded = .05f;
    public float accelerationAirborne = .1f;
    public float clampedFallSpeed = 30f;

    private bool canMove = true;
    private bool isCrouching;
    private bool isWalking;
    private bool isGrounded;
    
    #endregion
    
    #region JUMP
    [Header("JUMP")]
    public float maxJumpHeight = 4f;
    public float minJumpHeight = 1f;
    public float timeToJumpApex = .4f;

    private float coyoteTime = .2f;
    private float coyoteTimeCounter;

    [HideInInspector] public float jumpBufferTime = .2f;
    [HideInInspector] public float jumpBufferTimeCounter;

    [HideInInspector] public bool isJumping;

    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;
    
    #endregion
    
    #region ROLL
    [Header("ROLL")]
    public float rollDistance = 7f;
    public float rollTime = .175f;
    private bool canRoll;
    private bool isRolling;

    #endregion
    
    #region GLIDE
    [Header("GLIDE")]
    public float glideGravityMultiplier = 0.05f;
    private float glideGravity;
    [HideInInspector] public bool isGliding;
    
    #endregion
    
    #region WALLSLIDE
    [Header("WALL SLIDE")] 
    public float wallSlideSpeedMax = 5f;
    public float wallStickTime = 0.1f;
    [SerializeField] private Vector2 wallJump = new Vector2(35f, 20f);

    [SerializeField] private bool canSlideOnObjects;
    private bool isWallSliding;

    private float timeToWallUnstick;
    private int wallDirX;

    #endregion

    #endregion
    
    #region BOOMERANG
    [Header("BOOMERANG")]
    [HideInInspector] public bool isBoomeranging;
    private Boomerang boomerang;

    #endregion
    
    #region CHAKRA

    [Header("CHAKRA")] 
    public float maxChakra = 2.5f;
    private float chakra;
    
    [Header("SHARINGAN")] 
    public float sharinganTimeScale = .5f;
    private bool isUsingSharingan;
    
    [Header("RINNEGAN")] 
    public float range = 20f;
    public float rinneTimeScale = .2f;
    private bool isUsingRinnegan;
    private bool canTeleport;
    private bool isTeleporting;
    public float rinneBufferTime = .15f;
    private float rinneBufferTimeCounter;
    
    #endregion

    #region THROWABLE

    [Header("THROWABLE")] 
    [HideInInspector] public GameObject _throwable = null;
    [HideInInspector] public GameObject _pickable = null;
    [HideInInspector] public bool canPickThrowable;
    
    #endregion

    #region INTERACTION
    [Header("INTERACTION")]
    private bool canInteract;
    private bool isInteracting;

    private bool canPushObject;
    private bool isPushingObject;

    private bool _interactInp;

    #endregion

    #region Other
    [HideInInspector] public Vector3 velocity;
    private float velocityXSmoothing;

    private Vector4 directionalInput; // z = isCrouching, w = isInteracting
    
    #endregion
    
    #endregion
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        controller = GetComponent<Controller2D>();
        boomerang = GameObject.FindGameObjectWithTag("Boomerang").GetComponent<Boomerang>();
        Rinnegan.Instance.SetRange(range);
        Rinnegan.Instance.SetController(controller);
        chakra = maxChakra;

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        glideGravity = gravity * glideGravityMultiplier;
    }

    void Update()
    {
        CalculateVelocity();
        Flip();
        HandleInteractions();
        if (canMove)
        {
            HandleJump();
            HandleGlide();
            HandleCrouch();
            StartCoroutine(HandleRoll());
            HandleChakra();
            HandleSharingan();
            HandleRinnegan();
            HandlePushObject();
            HandleWallSliding();
            HandleClampedFallSpeed();
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisionData.below)
        {
            isGrounded = true;
            if (!isRolling) canRoll = true;
            isJumping = false;
            isGliding = false;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (isGrounded || controller.collisionData.above)
        {
            if (controller.collisionData.isSlidingDownSlope)
            {
                velocity.y += controller.collisionData.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0f;
            }
        }

        if (!canPickThrowable)
        {
            _pickable = null;
        }
    }

    #region Methods
    #region Other
    void HandleClampedFallSpeed()
    {
        if (!isWallSliding && !isGrounded)
        {
            if (velocity.y < -clampedFallSpeed)
            {
                velocity.y = -clampedFallSpeed;
            }
        }
    }

    void Flip()
    {
        if (!controller.collisionData.isPushingObject)
        {
            if (velocity.x < 0f)
            {
                transform.localScale = new Vector2(-1f, transform.localScale.y);
            }
            else
            {
                transform.localScale = new Vector2(1f, transform.localScale.y);
            }
        }
    }
    #endregion¯

    #region Mechanics
    void HandleJump()
    {
        if (coyoteTimeCounter > 0f && jumpBufferTimeCounter > 0f)
        {
            isJumping = true;

            if (controller.collisionData.isSlidingDownSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisionData.slopeNormal.x))
                {
                    velocity.y = maxJumpVelocity * controller.collisionData.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisionData.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }

            coyoteTimeCounter = 0f;
            jumpBufferTimeCounter = 0f;
            //cameraEffects.Shake(50f, 0.35f);
        }

        jumpBufferTimeCounter -= Time.deltaTime;
    }


    IEnumerator HandleRoll()
    {
        if (canMove && isGrounded && isRolling)
        {
            canRoll = false;
            isCrouching = true;
            float rollVelocity = rollDistance / rollTime;

            velocity.x = rollVelocity * controller.collisionData.faceDir;
            velocity.y = 0f;

            //impulseSource.GenerateImpulse(new Vector3(10, 10));
            // cameraEffects.Shake(8f, 0.01f);

            yield return new WaitForSeconds(rollTime);

            velocity.x = controller.collisionData.faceDir;
            isRolling = false;
            isCrouching = false;
        }
    }


    void HandleWallSliding()
    {
        if ((controller.collisionData.left || controller.collisionData.right) && !controller.collisionData.below && velocity.y < 0)
        {
            if (canPushObject)
            {
                if (canSlideOnObjects)
                {
                    isWallSliding = true;
                }
                else
                {
                    isWallSliding = false;
                }
            }
            else if (controller.collisionData.wallAhead)
            {
                isWallSliding = true;
            }
        }
        else
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            wallDirX = (controller.collisionData.left) ? -1 : 1;

            if ((controller.collisionData.left || controller.collisionData.right) && !isGrounded && velocity.y < 0)
            {
                if (velocity.y < -wallSlideSpeedMax)
                {
                    velocity.y = -wallSlideSpeedMax;
                }

                if (timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;
                    velocity.x = 0;

                    if (directionalInput.x != wallDirX && directionalInput.x != 0)
                    {
                        timeToWallUnstick -= Time.deltaTime;
                    }
                    else
                    {
                        timeToWallUnstick = wallStickTime;
                    }
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
        }
    }


    void HandleGlide()
    {
        if (isGliding)
        {
            if(velocity.y < glideGravity)
            {
                velocity.y = glideGravity;
            }
        }
    }


    void HandleCrouch()
    {
        if (isCrouching)
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, 1f, directionalInput.w);
        }
        else
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, 0f, directionalInput.w);
        }
    }


    void HandlePushObject()
    {
        if (canInteract)
        {
            canPushObject = controller.collisionData.canPushObject;

            if (canPushObject)
            {
                isPushingObject = controller.collisionData.isPushingObject;
            }
            else
            {
                isPushingObject = false;
            }
        }
        else
        {
            canPushObject = false;
            isPushingObject = false;
        }
    }

    void HandleInteractions()
    {
        canInteract = controller.collisionData.canInteract;

        if (_interactInp)
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, directionalInput.z, 1f);
            if (!canPushObject)
            {
                _interactInp = false;
            }
        }
        else
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, directionalInput.z, 0f);
        }
        
        if (canInteract)
        {
            isInteracting = controller.collisionData.isInteracting;
        }
        else
        {
            isInteracting = false;
            _interactInp = false;
        }

        if (isInteracting && !canPushObject)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }
    }


    void HandleBoomerang()
    {
        if (!isBoomeranging)
        {
            isBoomeranging = true;
            Vector2 normalizedInput = directionalInput.normalized;
            if (normalizedInput == Vector2.zero)
            {
                boomerang.directionalInput = (controller.collisionData.faceDir == 1) ? Vector2.right : Vector2.left;
            }
            else
            {
                boomerang.directionalInput = normalizedInput;
            }
            boomerang.ActivateBoomerang();
            boomerang.isBoomeranging = isBoomeranging;
        }
        else
        {
            boomerang.isReturning = true;
            boomerang.onReturn = true;
            transform.position = boomerang.transform.position;
        }
    }


    void HandleChakra()
    {
        if (chakra > 0f)
        {
            if (isUsingRinnegan)
            {
                chakra -= Time.deltaTime * 2f;
            }
            else if (isUsingSharingan)
            {
                chakra -= Time.deltaTime;
            }
        }
        else
        {
            canTeleport = false;
            isUsingRinnegan = false;
            isUsingSharingan = false;
        }

        if (chakra < maxChakra && !isUsingRinnegan && !isUsingSharingan)
        {
            chakra += Time.deltaTime;
        }
    }

    
    void HandleSharingan()
    {
        if (!isUsingRinnegan)
        {
            if (isUsingSharingan)
            {
                GameManager.Instance._gameState = GameState.Sharingan;
            }
            else
            {
                GameManager.Instance._gameState = GameState.Play;
            }
        }
        
        if (isUsingSharingan && !isUsingRinnegan)
        {
            Time.timeScale = sharinganTimeScale;
        }
        else if (!canTeleport && rinneBufferTimeCounter <= 0f)
        {
            Time.timeScale = 1f;
        }
    }
    
    
    void HandleRinnegan()
    {
        if (isUsingRinnegan)
        {
            GameManager.Instance._gameState = GameState.Rinnegan;
        }
        else
        {
            if (isUsingSharingan)
            {
                GameManager.Instance._gameState = GameState.Sharingan;
            }
            else
            {
                GameManager.Instance._gameState = GameState.Play;
            }
        }
        
        if (isUsingRinnegan)
        {
            canTeleport = true;
            Time.timeScale = rinneTimeScale;
        }
        else 
        {
            if (!Rinnegan.Instance.aimSelect)
            {
                canTeleport = false;
            }
        }

        if (canTeleport)
        {
            if (!Rinnegan.Instance.aimSelect)
            {
                if (Rinnegan.Instance._replacedObj != null)
                {
                    isTeleporting = true;
                    Vector3 _to = Rinnegan.Instance._replacedObj.transform.position;
                    Rinnegan.Instance._replacedObj.transform.position = transform.position;
                    transform.position = _to;
                    Rinnegan.Instance._replacedObj = null;
                    isTeleporting = false;
                    isUsingRinnegan = false;
                    rinneBufferTimeCounter = rinneBufferTime;
                }
            }
            else
            {
                if (!isUsingRinnegan)
                {
                    if (Rinnegan.Instance._replacedObj != null)
                    {
                        isTeleporting = true;
                        Vector3 _to = Rinnegan.Instance._replacedObj.transform.position;
                        Rinnegan.Instance._replacedObj.transform.position = transform.position;
                        transform.position = _to;
                        Rinnegan.Instance._replacedObj = null;
                        isTeleporting = false;
                        rinneBufferTimeCounter = rinneBufferTime;
                    }
                    
                    canTeleport = false;
                }
            }
        }

        rinneBufferTimeCounter -= Time.deltaTime;
    }


    void HandleThrowable()
    {
        if (canPickThrowable)
        {
            if (_throwable != null)
            {
                _throwable.GetComponent<Throwable>().state = ThrowableStates.Discard;
                _throwable = _pickable;
                _throwable.GetComponent<Throwable>().state = ThrowableStates.Picked;
            }
            else
            {
                _throwable = _pickable;
                _throwable.GetComponent<Throwable>().state = ThrowableStates.Picked;
            }
            
            canPickThrowable = false;
        }
        else
        {
            if (_throwable != null)
            {
                _throwable.GetComponent<Throwable>().Throw();
                _throwable = null;
            }
        }
    }

    void CalculateVelocity()
    {
        if (canMove)
        {
            float targetVelocityX;

            if (isWalking || isPushingObject)
            {
                targetVelocityX = directionalInput.x * walkSpeed;
            }
            else
            {
                targetVelocityX = directionalInput.x * runSpeed;
            }

            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (isGrounded) ? accelerationGrounded : accelerationAirborne);
        }
        else
        {
            velocity.x = 0f;
            directionalInput = Vector2.zero;
        }
        velocity.y += gravity * Time.deltaTime;
    }
    #endregion

    #endregion

    #region Input

    #region Directional Input
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }
    #endregion

    #region Jump
    public void OnJumpInputPressed()
    {
        if (isWallSliding)
        {
            velocity.x = -wallDirX * wallJump.x;
            velocity.y = wallJump.y;

            isWallSliding = false;
        }

        jumpBufferTimeCounter = jumpBufferTime;
    }

    public void OnJumpInputReleased()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }
    #endregion

    #region Glide
    public void OnGlideInputPressed()
    {
        if (isJumping)
        {
            isGliding = true;
        }
    }

    public void OnGlideInputReleased()
    {
        isGliding = false;
    }
    #endregion

    #region Crouch
    public void OnCrouchInputDown()
    {
        if (isGrounded)
        {
            isCrouching = true;
            //canMove = false;
        }
        else
        {
            isCrouching = false;
            //canMove = true;
        }
    }

    public void OnCrouchInputUp()
    {
        if (!controller.collisionData.above)
        {
            isCrouching = false;
            //canMove = true;
        }
    }
    #endregion

    #region Walk
    public void OnWalkInputPressed()
    {
        if (isGrounded)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    public void OnWalkInputReleased()
    {
        isWalking = false;
    }
    #endregion

    #region Roll
    public void OnRollInput()
    {
        if (canRoll)
        {
            isRolling = true;
        }
    }
    #endregion

    #region Interact
    public void OnInteractInput()
    {
        if (canInteract)
        {
            _interactInp = !_interactInp;
        }
    }
    #endregion

    #region Boomerang
    public void OnBoomerangInput()
    {
        HandleBoomerang();
    }
    #endregion

    #region Sharingan
    public void OnSharinganInputPressed()
    {
        isUsingSharingan = true;
    }

    public void OnSharinganInputReleased()
    {
        isUsingSharingan = false;
    }
    #endregion
    
    #region Aminotejikara
    public void OnRinneganInputPressed()
    {
        isUsingRinnegan = true;
    }

    public void OnRinneganInputReleased()
    {
        isUsingRinnegan = false;
    }
    #endregion

    #region Throwable
    public void OnThrowableInput()
    {
        HandleThrowable();
    }

    #endregion

    #endregion
}
