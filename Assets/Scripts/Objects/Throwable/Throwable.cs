using UnityEngine;

public class Throwable : MonoBehaviour
{
    [HideInInspector] public ThrowableStates state = ThrowableStates.Idle;

    public LayerMask collisionMask;
    public LayerMask playerMask;
    
    private float velocity = 2.5f;

    public float playerMaskRangeFactor = 1.5f;
    private float playerMaskRange;
    
    private float collisionMaskRange;

    public bool canBePicked;
    
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Player _player;
    
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _player = Player.Instance;
        playerMaskRange = transform.localScale.x * playerMaskRangeFactor;
        collisionMaskRange = transform.localScale.x * 0.5f;
    }
    
    void Update()
    {
        ThrownCollisionCheck();
        StateCheck();
        PlayerCollisionCheck();
    }

    #region Methods
    public void Throw()
    {
        state = ThrowableStates.Thrown;
        _rb.gravityScale = 0f;
        transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y + 0.25f);
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(PlayerInputManager.Instance.mousePosAction.ReadValue<Vector2>());
        Vector2 angle = (mousePos - transform.position);

        _rb.velocity = angle * velocity;
    }

    public void Replace()
    {
        state = ThrowableStates.Replaced;
        _rb.velocity = Vector2.zero;;
        _rb.gravityScale = velocity;
    }
    
    #endregion
    
    
    #region Collision Check

    private void PlayerCollisionCheck()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Collider2D playerHit = Physics2D.OverlapCircle(origin, playerMaskRange, playerMask);
        if (!playerHit)
        {
            if (state == ThrowableStates.Idle)
            {
                canBePicked = false;
            }
            return;
        }
        if (playerHit)
        {
            if (state == ThrowableStates.Idle && playerHit.CompareTag("Player"))
            {
                canBePicked = true;
                _player.canPickThrowable = canBePicked;
                _player._pickable = this.gameObject;
            }
        }
    }

    private void ThrownCollisionCheck()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Collider2D thrownHit = Physics2D.OverlapCircle(origin, collisionMaskRange, collisionMask);
        if (thrownHit)
        {
            if (state == ThrowableStates.Thrown || state == ThrowableStates.Replaced)
            {
                state = ThrowableStates.Discard;
            }
        }
    }
    
    #endregion


    #region State Check
    private void StateCheck()
    {
        if (state == ThrowableStates.Picked)
        {
            _sr.enabled = false;
            _collider.enabled = false;
        }
        else if(state != ThrowableStates.Discard)
        {
            _sr.enabled = true;
            _collider.enabled = true;
        }

        if (state == ThrowableStates.Discard)
        {
            Destroy(gameObject);
        }
        
        if (state == ThrowableStates.Thrown)
        {
            Destroy(gameObject, 10f);
        }
    }
    
    #endregion
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, playerMaskRange);
        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, collisionMaskRange);
    }
}
