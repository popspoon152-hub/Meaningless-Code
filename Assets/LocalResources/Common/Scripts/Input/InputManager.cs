using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;

    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;

    public static bool RunIsHeld;

    public static bool DashWasPressed;

    public static bool AttackWasPressed;
    public static bool AttackIsHeld;
    public static bool AttackWasReleased;

    public static bool BackWasPressed;
    public static bool BackIsHeld;
    public static bool BackWasReleased;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _attackAction;
    private InputAction _backAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];
        _attackAction = PlayerInput.actions["Attack"];
        _backAction = PlayerInput.actions["Back"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();

        DashWasPressed = _dashAction.WasPressedThisFrame();

        AttackWasPressed = _attackAction.WasPressedThisFrame();
        AttackIsHeld = _attackAction.IsPressed();
        AttackWasReleased = _attackAction.WasReleasedThisFrame();

        BackWasPressed = _backAction.WasPressedThisFrame();
        BackIsHeld = _backAction.IsPressed();
        BackWasReleased = _backAction.WasReleasedThisFrame();
    }
}
