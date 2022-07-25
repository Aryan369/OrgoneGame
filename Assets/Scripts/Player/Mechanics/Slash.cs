using UnityEngine;

public class Slash : MonoBehaviour
{
    #region Variables
    
    public static Slash Instance;
    
    private BoxCollider2D hitbox;
    
    #region ATTACK
    
    #region SLASH

    [Header("SLASH")] 
    private float slashRange = 3f;
    [HideInInspector] public float slashCooldown = .25f;
    [HideInInspector] public float slashCooldownCounter;

    #endregion

    #region THROWABLE

    [Header("THROWABLE")] 
    public LayerMask _throwableMask;
    private float _throwablePickableRange = 2.5f;
    private GameObject _throwable = null;
    private GameObject _pickable = null;
    private bool canPickThrowable;
    
    #endregion
    
    #endregion

    #endregion
    
    private void Awake()
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
    
    private void Start()
    {
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.size = new Vector2(slashRange, 0.2f);
        hitbox.offset = new Vector2(slashRange / 2f, 0f);
        hitbox.enabled = false;
    }

    private void Update()
    {
        HandleSlash();
        HandleThrowableCollision();
    }


    #region Methods
    
    #region Slash
    
    private void HandleSlash()
    {
        if (PlayerInputManager.Instance.attackAction.triggered)
        {
            if (slashCooldownCounter <= 0f)
            {
                if (GameManager.Instance._gameState != GameState.Paused && GameManager.Instance._gameState != GameState.Rinnegan)
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(PlayerInputManager.Instance.mousePosAction.ReadValue<Vector2>());
                    
                    if (Mathf.Sign(mousePos.x - transform.position.x) != Player.Instance.controller.collisionData.faceDir)
                    {
                        Player.Instance.controller.collisionData.faceDir *= -1;
                    }
                    
                    Vector2 slashDir = (mousePos - transform.position).normalized;
                    float angle = Mathf.Atan2(slashDir.y, slashDir.x) * Mathf.Rad2Deg;
                    transform.eulerAngles = new Vector3(0f, 0f, angle);
                    hitbox.enabled = true;
                }  
            }
        }
        else
        {
            if (slashCooldownCounter > 0f)
            {
                slashCooldownCounter -= Time.deltaTime;
            }
        }
    }
    
    #endregion
    
    #region Throwable
    public void HandleThrowable()
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

    void HandleThrowableCollision()
    {
        Collider2D throwableHit = Physics2D.OverlapCircle(transform.position, _throwablePickableRange, _throwableMask);
        if (!throwableHit)
        {
            canPickThrowable = false;
            _pickable = null;
        }
        if (throwableHit)
        {
            if (throwableHit.GetComponent<Throwable>().state == ThrowableStates.Idle)
            {
                canPickThrowable = true;
                _pickable = throwableHit.gameObject;
            }
        }
    }
    
    #endregion

    #endregion
    
    #region TriggerStay

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            print("hit enemy");
            slashCooldownCounter = slashCooldown;
        }
        
        hitbox.enabled = false;
    }
    
    #endregion
    
    #region Gizmos

    private void OnDrawGizmos()
    {
        //Slash
        Gizmos.color = new Color(1f, 0.8f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, slashRange);
        
        //Throwable
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, _throwablePickableRange);
    }

    #endregion
}
