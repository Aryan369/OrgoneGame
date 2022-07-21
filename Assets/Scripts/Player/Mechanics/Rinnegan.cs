using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rinnegan : MonoBehaviour
{
    public LayerMask collisionMask;
    
    private float _range;
    [HideInInspector] public bool _isUsingRinnegan;

    private void Update()
    {
        if (_isUsingRinnegan)
        {
            Vector2 _origin = new Vector2(transform.position.x, transform.position.y);
            var hitColliders = Physics2D.OverlapCircleAll(_origin, _range, collisionMask);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Vector2 dir = hitColliders[i].transform.position -  transform.position;
                Physics2D.queriesStartInColliders = false;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir);
                Debug.DrawRay(transform.position, dir, Color.black);

                if (hit)
                {
                    if (hit.collider == hitColliders[i])
                    {
                        hitColliders[i].GetComponent<Amenotejikarable>().isActive = true;
                    }
                    else
                    {
                        hitColliders[i].GetComponent<Amenotejikarable>().isActive = false;
                    }
                }
            }
        }
    }

    public void SetRange(float range)
    {
        _range = range;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.right * _range);
        Gizmos.DrawRay(transform.position, Vector3.left * _range);
    }
}
