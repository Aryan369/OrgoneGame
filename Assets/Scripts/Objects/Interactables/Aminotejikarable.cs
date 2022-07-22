using UnityEngine;
using UnityEngine.InputSystem;

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
        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            if (isSelected && Rinnegan.Instance._replacedObj != this.gameObject)
            {
                isSelected = false;
            }
            
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
            isActive = false;
            _sr.color = _color;
        }

        if (!Rinnegan.Instance.aimSelect)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (isHovered)
                {
                    Select();
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
                    Rinnegan.Instance._replacedObj = gameObject;
                }
                else
                {
                    Rinnegan.Instance._replacedObj = null;
                }
            }
        }
        
        isHovered = false;
    }

    void Select()
    {
        if (isActive)
        {
            if (!isSelected)
            {
                isSelected = true;
                Rinnegan.Instance._replacedObj = gameObject;
            }
            else
            {
                isSelected = false;
                Rinnegan.Instance._replacedObj = null;
            }
        }
    }
}