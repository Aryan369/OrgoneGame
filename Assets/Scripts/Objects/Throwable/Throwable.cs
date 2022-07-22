using UnityEngine;

public class Throwable : MonoBehaviour
{
    public ThrowableStates state = ThrowableStates.Idle;

    public LayerMask collisionMask;
    public float range = 1.25f;
    public float velocity = 10f;

    public bool canBePicked;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private Player _player;
    
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _player = Player.Instance;
    }
    
    void Update()
    {
        CollisionCheck();
        StateCheck();
    }

    public void Throw()
    {
        state = ThrowableStates.Thrown;
        transform.position = _player.transform.position;
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 angle = (mousePos - transform.position);

        _rb.velocity = angle * velocity;
    }
    
    private void CollisionCheck()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Collider2D hit = Physics2D.OverlapCircle(origin, range, collisionMask);
        
        if (!hit)
        {
            if (state == ThrowableStates.Idle)
            {
                canBePicked = false;
                _player.canPickThrowable = canBePicked;
            }
            return;
        }
        if (hit)
        {
            if (state == ThrowableStates.Idle && hit.CompareTag("Player"))
            {
                canBePicked = true;
                _player.canPickThrowable = canBePicked;
                _player._pickable = this.gameObject;
            }

            if (state == ThrowableStates.Thrown && !hit.CompareTag("Player"))
            {
                state = ThrowableStates.Discard;
            }
        }
    }

    private void StateCheck()
    {
        if (state == ThrowableStates.Picked)
        {
            _sr.enabled = false;
        }
        else if(state != ThrowableStates.Discard)
        {
            _sr.enabled = true;
        }

        if (state == ThrowableStates.Discard)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
