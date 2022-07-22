using UnityEngine;
using UnityEngine.InputSystem;

public class Throwable : MonoBehaviour
{
    public ThrowableStates state = ThrowableStates.Idle;

    public LayerMask playerMask;
    public LayerMask collisionMask;
    
    public float velocity = 4f;

    public float playerMaskRangeFactor = 1.5f;
    private float playerMaskRange;
    
    private float collisionMaskRange;

    private bool canBePicked;
    
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
        PlayerCollisionCheck();
        StateCheck();
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
                _player.canPickThrowable = canBePicked;
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
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, playerMaskRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collisionMaskRange);
    }
}
