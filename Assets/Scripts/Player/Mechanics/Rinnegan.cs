using UnityEngine;

public class Rinnegan : MonoBehaviour
{
    public static Rinnegan Instance;
    
    public LayerMask collisionMask;

    public bool aimSelect = true;
    private float _range;
    [HideInInspector] public bool _isUsingRinnegan;
    [HideInInspector] public GameObject _replacedObj = null;

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

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Vector2 angle = (mousePos - transform.position);

            RaycastHit2D _hit = Physics2D.Raycast(transform.position, angle, _range);
            Debug.DrawRay(transform.position, angle, Color.green);
            
            if (_hit)
            {
                if (_hit.collider.CompareTag("Amenotejikara"))
                {
                    _hit.collider.GetComponent<Amenotejikarable>().isHovered = true;
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
