using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    public ThrowableStates state = ThrowableStates.Idle;

    public LayerMask collisionMask;
    public float range = 1.25f;

    public bool canBePicked;
    public bool canThrow;

    private SpriteRenderer _sr;
    private Player _player;
    
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _player = Player.Instance;
    }
    
    void Update()
    {
        CollisionCheck();
        StateCheck();
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
            }
            return;
        }
        if (hit)
        {
            if (state == ThrowableStates.Idle && hit.CompareTag("Player"))
            {
                canBePicked = true;
            }
        }
    }

    private void StateCheck()
    {
        if (state == ThrowableStates.Picked)
        {
            _sr.enabled = false;
        }
        else
        {
            _sr.enabled = true;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
