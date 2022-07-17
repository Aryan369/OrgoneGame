using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindingManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private PlayerInputManager playerInputManager;
    
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference glideAction;
    [SerializeField] private InputActionReference walkAction;
    [SerializeField] private InputActionReference actionBtnAction;

    [SerializeField] private TMP_Text bindingDisplayNameText;
    [SerializeField] private GameObject startRebindingObject;
    [SerializeField] private GameObject waitingForInputObject;
    #endregion

    public void StartRebinding()
    {
        startRebindingObject.SetActive(false);
        waitingForInputObject.SetActive(true);
        
        playerInputManager.playerInput.SwitchCurrentActionMap("UI");
    }
}
