using System.Collections;
using UnityEngine;

namespace StarlightLib
{
    public class Player : Character
    {
        [SerializeField, Tooltip("The current move input")] Vector2 moveInput;
        [SerializeField, Tooltip("Camera-related transform")] Transform cam, camXRotator, camYRotator;
        [SerializeField, Tooltip("How far from the player to position the camera")] float camDistanceFromPlayer;
        [SerializeField, Tooltip("The camera's offset from the player's position")] Vector3 camOffsetFromPlayer;
        [SerializeField, Tooltip("The current look input")] Vector2 lookInput;
        [SerializeField, Tooltip("The multiplier to look input for rotating the camera")] float lookSpeed;
        /// <summary>
        /// The current camera pitch. Gets clamped by LookPitchClamp
        /// </summary>
        float pitchAngle;
        [SerializeField, Tooltip("The negative (x) and positive (y) angles to clamp the aim pitch between")] Vector2 lookPitchClamp;
        [SerializeField, Tooltip("How to apply the force to move the player. For testing purposes.")] ForceMode forceMode;
        [SerializeField, Tooltip("The angular drag applied to the player when on the ground, to help slow down the rolling")] float groundedAngularDrag;
        [SerializeField, Tooltip("The move force multiplier when airborne")] float airborneMoveForce;
        [SerializeField, Tooltip("The overall move force multiplier, increased by picking up items")] float moveForceMultiplier;
        [SerializeField, Tooltip("The move force multiplier gained per item")] float moveForcePerItem;

        public void Respawn()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            moveForceMultiplier = 1f;
            currentHealth = maxHealth;
            ModifyHealth(0);
        }

        public void SetMoveInput(Vector2 moveInput)
        {
            this.moveInput = moveInput;
        }
        public void SetLookInput(Vector2 lookInput)
        {
            this.lookInput = lookInput;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            Move();
        }

        protected override void Move()
        {
            Quaternion yRotation = Quaternion.Euler(0, cam.eulerAngles.y, 0);
            Vector3 axis = Vector3.ProjectOnPlane(yRotation * new Vector3(moveInput.x, 0, moveInput.y), grounded ? groundNormal : Vector3.up);
            rb.AddForce((grounded ? moveForce : airborneMoveForce) * moveForceMultiplier * axis, forceMode);
            rb.angularDrag = CheckGround() ? groundedAngularDrag : 0.05f;
        }

        protected override void Start()
        {
            base.Start();
            if(cam == null)
            {
                cam = Camera.main.transform;
            }
            moveForceMultiplier = 1;
        }
        protected override void Update()
        {
            base.Update();
            
        }
        private void LateUpdate()
        {
            cam.transform.localPosition = Vector3.back * camDistanceFromPlayer;
            camYRotator.SetPositionAndRotation(transform.position, camYRotator.rotation *= Quaternion.Euler(0, lookInput.x * Time.smoothDeltaTime * lookSpeed, 0));
            pitchAngle = Mathf.Clamp(pitchAngle -= Time.smoothDeltaTime * lookInput.y * lookSpeed, lookPitchClamp.x, lookPitchClamp.y);
            camXRotator.localRotation = Quaternion.Euler(pitchAngle, 0, 0);

        }
        internal override void ModifyHealth(float diff)
        {
            base.ModifyHealth(diff);
            GameManager.instance.PlayerHit();
        }
        
        internal void AddMoveForce()
        {
            moveForceMultiplier += moveForcePerItem;
        }
    }
}