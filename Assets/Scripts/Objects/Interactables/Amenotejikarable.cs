using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amenotejikarable : MonoBehaviour
{
    public bool isActive;

    private SpriteRenderer sr;
    private Color _color;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        _color = sr.color;
    }

    private void Update()
    {
        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            if (isActive)
            {
                sr.color = Color.cyan;
            }
            else
            {
                sr.color = Color.magenta;
            }
        }
        else
        {
            sr.color = _color;
        }
    }
}
