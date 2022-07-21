using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private Sprite cue0;
    [SerializeField] private Sprite cue1;
    private SpriteRenderer _sr;
    
    public bool _inRange { get; private set; }
    private bool _inRangeOld;

    private Controller2D _controller;
    
    private void Start()
    {
        _sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _sr.sprite = cue0;
        _controller = FindObjectOfType<Player>().GetComponent<Controller2D>();
    }

    private void Update()
    {
        Collision();
        CheckInRange();
    }

    private void Collision()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D hit =  Physics2D.CircleCast(origin, range, Vector2.right, 0f, collisionMask);

        if (!hit)
        {
            _inRange = false;
            return;
        }
        if (hit)
        {
            _inRange = true;
        }
    }

    void CheckInRange()
    {
        if (_inRange != _inRangeOld)
        {
            if (_inRange)
            {
                _sr.sprite = cue1;
            }
            else
            {
                _sr.sprite = cue0;
            }

            _controller.collisionData.canInteract = _inRange;
        }

        _inRangeOld = _inRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector3.right * range);
        Gizmos.DrawRay(transform.position, Vector3.left * range);
    }
}
