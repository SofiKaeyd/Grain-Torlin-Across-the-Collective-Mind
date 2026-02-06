using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlayerAttack;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        //playerInputActions.Combat.Attack.started += PlayerAttack_started;
    }

    private void PlayerAttack_started(InputAction.CallbackContext obj)
    {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputActions = playerInputActions.Player.Move.ReadValue<Vector2>();

        return inputActions;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }
}







//Vector2 inputVector = new Vector2(0, 0); // Vector2.zero

//if (Input.GetKey(KeyCode.W))
//{
//    inputVector.y = 1f;
//}

//if (Input.GetKey(KeyCode.S))
//{
//    inputVector.y = -1f;
//}

//if (Input.GetKey(KeyCode.A))
//{
//    inputVector.x = -1f;
//}

//if (Input.GetKey(KeyCode.D))
//{
//    inputVector.x = 1f;
//}

//inputVector = inputVector.normalized;