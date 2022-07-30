using UnityEngine;

public class Aminotejikarable : MonoBehaviour
{
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool isHovered;
    private bool isSelected;
    private bool isSelectedOld;

    private SpriteRenderer _sr;
    private Color _color;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _color = _sr.color;
    }

    private void Update()
    {
        ColorSelect();
        
        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            if (Vector2.Distance(transform.position, Player.Instance.transform.position) > Chakra.Instance.range)
            {
                isActive = false;
            }
            
            if (isSelected && Chakra.Instance._replacedObj != this.gameObject)
            {
                isSelected = false;
            }
        }
        else
        {
            isActive = false;
        }
        
        Aim();
        
        isHovered = false;
    }

    #region Methods
    void Aim()
    {
        if (!Chakra.Instance.aimToSelect)
        {
            if (InputProvider.Instance.attackAction.triggered)
            {
                if (isHovered)
                {
                    if (isActive)
                    {
                        if (!isSelected)
                        {
                            isSelected = true;
                            Chakra.Instance._replacedObj = gameObject;
                        }
                        else
                        {
                            isSelected = false;
                            Chakra.Instance._replacedObj = null;
                        }
                    }
                }
            }
        }
        else
        {
            isSelectedOld = isSelected;
            if (isHovered && isActive)
            {
                isSelected = true;
            }
            else
            {
                isSelected = false;
            }

            if (isSelected != isSelectedOld)
            {
                if (isSelected)
                {
                    Chakra.Instance._replacedObj = gameObject;
                }
                else
                {
                    Chakra.Instance._replacedObj = null;
                }
            }
        }
    }

    #endregion


    #region Graphic
    void ColorSelect()
    {
        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            if (isSelected)
            {
                _sr.color = new Color(0f, 1f, 1f);
            }
            else if (isHovered)
            {
                _sr.color = new Color(1f, 0.8443396f, 0.9693651f);
            }
            else if (isActive)
            {
                _sr.color = new Color(0.25f, 0.8971107f, 0.4841958f);
            }
            else
            {
                _sr.color = new Color(1f, 0f, 0.6135602f);
            }
        }
        else
        {
            _sr.color = _color;
        }
    }

    #endregion
}
