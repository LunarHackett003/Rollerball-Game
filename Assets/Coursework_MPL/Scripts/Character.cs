using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace StarlightLib
{
    /// <summary>
    /// Character component
    /// </summary>
    public abstract class Character : MonoBehaviour
    {
        [SerializeField] protected string Name = "";
        /// <summary>
        /// Public accessor for Name. Doesn't need to be a method.
        /// </summary>
        public string GetName { get { return Name; } }

        [SerializeField] internal float moveForce;
        /// <summary>
        /// Sets the movement force;
        /// </summary>
        /// <param name="additive"></param>
        public void SetMoveForce(float force)
        {
            moveForce = force;
        }

        protected Rigidbody rb;
        [SerializeField] protected float moveDrag;

        [SerializeField] internal float maxHealth, currentHealth;

        [SerializeField] protected Transform particleRoot;
        [SerializeField] protected Vector3 groundNormal;


        [SerializeField] protected ParticleSystem speedParticleSystem;
        [SerializeField] protected Vector2 velocityParticleScaling;
        [SerializeField] protected Vector3 particleRootOffset;
        [SerializeField] protected float maxSpeedParticles;
        protected ParticleSystem.EmissionModule emissionModule;
        protected bool grounded;
        protected float particleLerp;
        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            emissionModule = speedParticleSystem.emission;
            speedParticleSystem.Play(true);
            currentHealth = maxHealth;
        }
        protected virtual void Update()
        {

        }
        /// <summary>
        /// Remember to call Move() on all children! this has different conditions for running on each child class!
        /// </summary>
        protected virtual void FixedUpdate()
        {
            rb.drag = moveDrag;
            grounded = CheckGround();
            if (rb.velocity != Vector3.zero)
                particleRoot.forward = Vector3.ProjectOnPlane(rb.velocity, groundNormal);
            particleLerp = Mathf.InverseLerp(velocityParticleScaling.x, velocityParticleScaling.y, rb.velocity.magnitude);
            emissionModule.rateOverTimeMultiplier = grounded ? particleLerp * maxSpeedParticles : 0;

        }
        protected abstract void Move();
        internal virtual void ModifyHealth(float diff)
        {
            currentHealth += diff;
        }

        protected bool CheckGround()
        {
            if (Physics.SphereCast(transform.position, 0.4f, Vector3.down, out RaycastHit hit, 0.7f))
            {
                groundNormal = hit.normal;
                particleRoot.position = hit.point;
                if (Vector3.Dot(Vector3.up, hit.normal) > 0.2f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
