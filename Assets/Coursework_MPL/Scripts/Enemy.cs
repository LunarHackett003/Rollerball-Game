using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace StarlightLib
{
    public class Enemy : Character
    {
        [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected Transform targetPoint;
        [SerializeField] Vector3 attackBoxOffset;
        [SerializeField] Vector3 attackBoxSize;
        [SerializeField] internal float stunTime;
        [SerializeField] protected float stunTimeLeft;

        [SerializeField] protected float outgoingDamage;
        [SerializeField] protected bool canAttack;
        [SerializeField] protected UnityEvent attackEvent;
        [SerializeField] bool stunFlag;
        protected enum EnemyState
        {
            Passive = 0,
            Active = 1,
            Stunned = 2,
            Dead = 4
        }
        [SerializeField] protected EnemyState state;
        protected override void Start()
        {
            base.Start();
            targetPoint = FindAnyObjectByType<Player>().transform;
        }
        protected override void Move()
        {

                //Navmesh agent movement code, since we might need to do something special here
                agent.updatePosition = false;
                //we need to calculate the delta for the navmesh agent to find out which direction we're moving in
                Vector3 deltaPos = agent.nextPosition - transform.position;
                //Then we zero the y axis so we don't end up countering gravity
                deltaPos.y = 0;
                //We normalise deltaPos to get a 1 to -1 input
                deltaPos.Normalize();
                //Multiply it by the distance to the target, clamped from 0 to 1, multiplied by 0.5
                deltaPos *= Mathf.Clamp01(agent.remainingDistance * 0.5f);
                //Then we project the lateral delta pos along the normal of the ground we're walking on, and use that as the multiplier for move force
                rb.AddForce(moveForce * Vector3.ProjectOnPlane(deltaPos, groundNormal));
        }
        protected virtual void GetTarget()
        {
            //This will be overridden in the child class.
            //Make sure to set the agent's target position!
        }
        protected virtual void AttackPlayer()
        {
            if (canAttack)
            {
                if (state == EnemyState.Active)
                {
                    //Performs an overlapBox check to see if the player is currently in the attack box, deals damage if it is, and then disables attacking for a short time
                    Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(attackBoxOffset), attackBoxOffset / 2, transform.rotation, GameManager.instance.playerMask);
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Collider col = colliders[i];
                        if (col.attachedRigidbody != null)
                        {
                            //If the player is moving fast, we'll prevent them being hit.
                            if (col.attachedRigidbody.velocity.magnitude > 10)
                                return;
                            col.attachedRigidbody.GetComponent<Player>().ModifyHealth(-outgoingDamage);
                            StartCoroutine(ResetAttack());
                            attackEvent?.Invoke();
                        }
                    }
                }
            }
        }
        protected IEnumerator ResetAttack()
        {
            canAttack = false;
            yield return new WaitForSeconds(GameManager.instance.outgoingDamageInterval);
            canAttack = true;
            yield break;
        }
        protected IEnumerator Stun()
        {
            //Prevents the enenmy from moving for a while.
            //Receiving hits while stunned increases the stun time.
            stunFlag = true;
            var wff = new WaitForFixedUpdate();
            while (stunTimeLeft > 0)
            {
                stunTimeLeft -= Time.fixedDeltaTime;
                state = EnemyState.Stunned;
                yield return wff;
            }
            state = EnemyState.Passive;
            stunFlag = false;
            yield break;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (state != EnemyState.Stunned && state != EnemyState.Dead && !stunFlag)
            {
                GetTarget();
                Move();
                AttackPlayer();
            }
        }
        internal override void ModifyHealth(float diff)
        {
            base.ModifyHealth(diff);
            stunTimeLeft = stunTime;
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (state != EnemyState.Stunned)
                    StartCoroutine(Stun());
            }
        }
        protected virtual void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackBoxOffset, attackBoxSize);
            if (targetPoint)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetPoint.position, 1);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody)
            {
                if (collision.rigidbody.CompareTag("Player"))
                {
                    float mag = collision.impulse.magnitude;
                    if (mag > GameManager.instance.damageImpulseThreshold)
                    {
                        ModifyHealth(-mag * GameManager.instance.damageImpulseMultiplier);
                    }
                }
            }
        }
        protected void Die()
        {
            if (GameManager.instance)
            {
                GameManager.instance.UpdateScore();
            }
            Destroy(gameObject);
        }
    }
}