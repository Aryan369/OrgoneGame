using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player), typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    #region Variables
    Player player;

    [HideInInspector] public PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction glideAction;
    private InputAction walkAction;
    private InputAction actionBtnAction;

    #endregion


    private void Awake()
    {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        glideAction = playerInput.actions["Glide"];
        walkAction = playerInput.actions["Walk"];
        actionBtnAction = playerInput.actions["ActionBtn"];

        jumpAction.started += Jump;
        jumpAction.canceled += Jump;

        glideAction.started += Glide;
        glideAction.canceled += Glide;

        walkAction.started += Walk;
        walkAction.canceled += Walk;

        actionBtnAction.performed += ActionBtn;
    }

    void Update()
    {
        Vector2 directionalInput = moveAction.ReadValue<Vector2>();
        player.SetDirectionalInput(directionalInput);

        #region Crouch
        if (directionalInput.x == 0f)
        {
            if (directionalInput.y == -1f)
            {
                player.OnCrouchInputDown();
            }
            else if (directionalInput.y > -1f)
            {
                player.OnCrouchInputUp();
            }
        }
        #endregion

        #region Dash
        if (Input.GetKeyDown(KeyCode.X))
        {
            player.OnDashInput();
        }
        #endregion

        #region Roll
        if(Mathf.Abs(directionalInput.x) > 0f && directionalInput.y == -1f)
        {
            player.OnRollInput();
        }
        #endregion

        #region Boomerang
        if (Input.GetKeyDown(KeyCode.C))
        {
            player.OnBoomerangInput();
        }
        #endregion
    }

    #region Methods
    #region Jump
    void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnJumpInputPressed();
        }

        if (ctx.canceled)
        {
            player.OnJumpInputReleased();
        }
    }
    #endregion

    #region Glide
    void Glide(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnGlideInputPressed();
        }

        if (ctx.canceled)
        {
            player.OnGlideInputReleased();
        }
    }
    #endregion

    #region Walk
    void Walk(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnWalkInputPressed();
        }

        if (ctx.canceled)
        {
            player.OnWalkInputReleased();
        }
    }
    #endregion

    #region Action Btn
    void ActionBtn(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            player.OnActionBtnInput();
        }
    }
    #endregion

    #endregion
}
