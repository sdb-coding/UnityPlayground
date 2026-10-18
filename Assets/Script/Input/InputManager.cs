using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerInput playerInput;

    private Stack<InputActionMap> controlsStack = new Stack<InputActionMap>();

    public bool debugEnableGameView = true;
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        Instance = this;
       
    }
    private void Start()
    {
        if (debugEnableGameView)
        {
            var gameView = playerInput.actions.FindActionMap("GameView");
            gameView.Enable();
            controlsStack.Push(gameView);
        }
    }
    public void EnableControlMap(InputActionMap map)
    {
        map.Enable();
        controlsStack.Peek().Disable();
        controlsStack.Push(map);
    }
    public void DisableControlMap(InputActionMap map)
    {
        if (controlsStack.Count == 1)
        {
            Debug.LogError("Asked to disable and pop the only active control map");
            return;
        }
        if (controlsStack.Peek() == map)
        {
            controlsStack.Pop();
            map.Disable();
            controlsStack.Peek().Enable();
        } else
        {
            Debug.LogError("Requested control map wasn't the most recent added to the stack");
        }
    }
    #region Utility
    public IEnumerator ListenForAction(InputAction inputAction, Action callback)
    {
        while (true)
        {
            yield return null;
            if (inputAction.WasPressedThisFrame())
            {
                callback.Invoke();
            }
        }
    }
    #endregion
}
