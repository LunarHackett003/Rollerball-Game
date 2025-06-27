using StarlightLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightLib
{
    public class Chaser : Enemy
    {
        [SerializeField] Transform viewPoint;
        [SerializeField] internal float viewConeDotThreshold;
        [SerializeField] internal float retreatTime = 10;
        [SerializeField] internal float currentRetreatTime;
        [SerializeField] Vector3 cachedStartPosition;
        protected override void Start()
        {
            base.Start();
            StartCoroutine(Retreat());
            cachedStartPosition = transform.position;
        }
        protected override void GetTarget()
        {
            base.GetTarget();
            //We don't want to do ANY of this if we haven't set the player transform
            if (GameManager.instance.playerTransform)
            {
                if (Vector3.Dot(transform.forward, GameManager.instance.playerTransform.position - transform.position) > viewConeDotThreshold)
                {
                    bool found = false;
                    Vector3 targetPosition = Vector3.zero;
                    //The chaser needs line of sight to the player to chase it.
                    //To do this, we'll perform a series of linecasts from the enemy to the player
                    //The first linecast is *directly* to the player's position. If we can see the centre of the player, we'll stop here.
                    //We'll linecast from the viewPoint transform - which acts kind of like the eye/head position.
                    //We also won't use a layermask, as we want to hit all the colliders.
                    if (Physics.Linecast(viewPoint.position, GameManager.instance.playerTransform.position, out RaycastHit hit) && hit.rigidbody)
                    {
                        Debug.DrawLine(viewPoint.position, GameManager.instance.playerTransform.position, Color.green);
                    }
                    else
                    {
                        Debug.DrawLine(viewPoint.position, GameManager.instance.playerTransform.position, Color.red);
                        //We didn't hit on the first one, so now we need to check just above the player.
                        //If this one doesn't hit, we'll check just below the player.
                        if (Physics.Linecast(viewPoint.position, GameManager.instance.playerTransform.position + (Vector3.up * 0.2f), out hit) && hit.rigidbody)
                        {
                            Debug.DrawLine(viewPoint.position, GameManager.instance.playerTransform.position + (Vector3.up * 0.2f), Color.green);

                        }
                        else
                        {
                            Debug.DrawLine(viewPoint.position, GameManager.instance.playerTransform.position + (Vector3.up * 0.2f), Color.red);

                            //Last try, otherwise we'll just leave the target position as it is, effectively making the enemy investiage that position.
                            if (Physics.Linecast(viewPoint.position, GameManager.instance.playerTransform.position - (Vector3.up * 0.2f), out hit) && hit.rigidbody)
                            {
                                if (hit.rigidbody)
                                {
                                    Debug.DrawLine(viewPoint.position, GameManager.instance.playerTransform.position - (Vector3.up * 0.2f), Color.green);
                                }
                            }
                            else
                                Debug.DrawLine(viewPoint.position, GameManager.instance.playerTransform.position - (Vector3.up * 0.2f), Color.red);
                        }
                    }

                    //If we hit something, then we'll set the enemy's state to active, otherwise, we'll set it to passive.
                    found = hit.rigidbody && hit.rigidbody.transform == GameManager.instance.playerTransform;
                    targetPosition = found ? GameManager.instance.playerTransform.position : agent.destination;
                    agent.SetDestination(targetPosition);
                    state = found ? EnemyState.Active : EnemyState.Passive;
                    //If an enemy is found, restart the retreat timer
                    if (found)
                        currentRetreatTime = 0;
                }
            }
            else
            {
                state = EnemyState.Passive;
            }
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
        }
        protected override void OnDrawGizmos()
        {
            if (viewPoint)
            {
                Gizmos.matrix = viewPoint.localToWorldMatrix;
                float angle = Mathf.Lerp(-180, 0, Mathf.InverseLerp(-1, 1, viewConeDotThreshold));
                Gizmos.DrawRay(Vector3.zero, Quaternion.Euler(angle, 0, 0) * Vector3.forward);
                Gizmos.DrawRay(Vector3.zero, Quaternion.Euler(-angle, 0, 0) * Vector3.forward);
                Gizmos.DrawRay(Vector3.zero, Quaternion.Euler(0, -angle, 0) * Vector3.forward);
                Gizmos.DrawRay(Vector3.zero, Quaternion.Euler(0, angle, 0) * Vector3.forward);
            }
            base.OnDrawGizmos();
        }
        IEnumerator Retreat()
        {
            var wff = new WaitForFixedUpdate();
            while (true)
            {
                while (state == EnemyState.Passive)
                {
                    currentRetreatTime += Time.fixedDeltaTime;
                    if(currentRetreatTime > GameManager.instance.retreatTime)
                    {
                        agent.SetDestination(cachedStartPosition);
                        currentRetreatTime = 0;
                    }
                    yield return wff;
                }
                yield return wff;

            }
        }
    }
}