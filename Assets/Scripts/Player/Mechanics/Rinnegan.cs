using UnityEngine;

public class Rinnegan : MonoBehaviour
{
    public static Rinnegan Instance;
    
    public LayerMask collisionMask;
    private Controller2D _controller;

    public bool aimSelect = true;
    private float _range;
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
        if (GameManager.Instance._gameState == GameState.Rinnegan)
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
                        if (Mathf.Sign(hitColliders[i].transform.position.x - transform.position.x) == _controller.collisionData.faceDir)
                        {
                            hitColliders[i].GetComponent<Aminotejikarable>().isActive = true;
                        }
                        else
                        {
                            hitColliders[i].GetComponent<Aminotejikarable>().isActive = false;
                        }
                    }
                    else
                    {
                        hitColliders[i].GetComponent<Aminotejikarable>().isActive = false;
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
                if (_hit.collider.CompareTag("Aminotejikarable"))
                {
                    if (_hit.collider.GetComponent<Aminotejikarable>().isActive == true)
                    {
                        _hit.collider.GetComponent<Aminotejikarable>().isHovered = true;
                    }
                }
            }
        }
    }

    public void SetRange(float range)
    {
        _range = range;
    }

    public void SetController(Controller2D controller)
    {
        _controller = controller;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}
