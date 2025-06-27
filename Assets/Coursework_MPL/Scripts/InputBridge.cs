using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightLib
{
    public class InputBridge : MonoBehaviour
    {
        [SerializeField] Player p;
        private void Awake()
        {
            if (!p)
            {
                p = FindObjectOfType<Player>();
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public void MoveInput(InputAction.CallbackContext context)
        {
            p.SetMoveInput(context.ReadValue<Vector2>());
        }
        public void LookInpt(InputAction.CallbackContext context)
        {
            p.SetLookInput(context.ReadValue<Vector2>());
        }
    }
}