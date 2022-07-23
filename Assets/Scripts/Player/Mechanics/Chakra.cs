using UnityEngine;
//ReSharper disable All

public class Chakra : MonoBehaviour
{
    public static Chakra Instance;
    
    private Controller2D _controller;

    #region CHAKRA

    [Header("CHAKRA")] 
    public float maxChakra = 2.5f;
    private float chakra;
    
    [Header("SHARINGAN")] 
    public float sharinganTimeScale = .5f;
    [HideInInspector] public bool isUsingSharingan;
    
    [Header("RINNEGAN")] 
    public LayerMask collisionMask;
    
    public float range = 20f;
    
    public float rinneTimeScale = 0.0001f;
    private float rinneTimeScaleBufferFactor = 2000f;
    
    [HideInInspector] public bool isUsingRinnegan;
    
    private bool canTeleport;
    private bool isTeleporting;
    
    public float rinneBufferTime = .15f;
    private float rinneBufferTimeCounter;
    
    public bool aimToSelect = true;
    public bool _360Vision;
    
    #endregion

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

    private void Start()
    {
        _controller = Player.Instance.controller;
        chakra = maxChakra;
    }

    private void Update()
    {
        HandleChakra();
        HandleSharingan();
        HandleRinnegan();
        
        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            CollisionCheck();
        }
    }


    #region Methods
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
            if (!aimToSelect)
            {
                canTeleport = false;
            }
        }

        if (canTeleport)
        {
            if (!aimToSelect)
            {
                if (_replacedObj != null)
                {
                    isTeleporting = true;
                    Vector3 _to = _replacedObj.transform.position;
                    _replacedObj.transform.position = transform.position;
                    Player.Instance.transform.position = _to;
                    if (_replacedObj.CompareTag("Throwable"))
                    {
                        _replacedObj.GetComponent<Throwable>().Replace();
                    }
                    _replacedObj = null;
                    isTeleporting = false;
                    isUsingRinnegan = false;
                    Time.timeScale = rinneTimeScale * rinneTimeScaleBufferFactor;
                    rinneBufferTimeCounter = rinneBufferTime;
                }
            }
            else
            {
                if (!isUsingRinnegan)
                {
                    if (_replacedObj != null)
                    {
                        isTeleporting = true;
                        Vector3 _to = _replacedObj.transform.position;
                        _replacedObj.transform.position = transform.position;
                        Player.Instance.transform.position = _to;
                        if (_replacedObj.CompareTag("Throwable"))
                        {
                            _replacedObj.GetComponent<Throwable>().Replace();
                        }
                        _replacedObj = null;
                        isTeleporting = false;
                        Time.timeScale = rinneTimeScale * rinneTimeScaleBufferFactor;
                        rinneBufferTimeCounter = rinneBufferTime;
                    }
                    
                    canTeleport = false;
                }
            }
        }

        rinneBufferTimeCounter -= Time.deltaTime;
    }
    

    #endregion

    #region Collision
    void CollisionCheck()
    {
        Vector2 _origin = new Vector2(transform.position.x, transform.position.y);
        var hitColliders = Physics2D.OverlapCircleAll(_origin, range, collisionMask);

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
                    if (_360Vision)
                    {
                        hitColliders[i].GetComponent<Aminotejikarable>().isActive = true;
                    }
                    else
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
                }
            }
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(PlayerInputManager.Instance.mousePosAction.ReadValue<Vector2>());
        Vector2 angle = (mousePos - transform.position);

        RaycastHit2D _hit = Physics2D.Raycast(transform.position, angle, range);
        Debug.DrawRay(transform.position, angle, Color.green);
        
        if (_hit)
        {
            if (_hit.collider.CompareTag("Aminotejikarable") || _hit.collider.CompareTag("Throwable"))
            {
                if (_hit.collider.GetComponent<Aminotejikarable>().isActive)
                {
                    _hit.collider.GetComponent<Aminotejikarable>().isHovered = true;
                }
            }
        }
    }
    
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    
    #endregion
}
