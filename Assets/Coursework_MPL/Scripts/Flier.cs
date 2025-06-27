using StarlightLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightLib
{
    public class Flier : Chaser
    {
        [SerializeField] internal float targetHeight;
        [SerializeField] internal float currentHeight;
        [SerializeField] internal float maxHeight;
        [SerializeField] internal float verticalForce;
        [SerializeField] internal float lookSpeed;

        [SerializeField] Vector3 targetRotation;
        [SerializeField] PID xPidPos, yPidPos, zPidPos;

        [SerializeField] Vector3 pidForce;
        [SerializeField] Vector2 pidForceMinMax;
        [SerializeField] ParticleSystem laserWarmupParticle;
        [SerializeField] LineRenderer laserBeam;
        [SerializeField] float laserDeleteTime;
        [SerializeField] float maxLaserWidthOverChargeup;
        [SerializeField] float laserWidthOnFire;
        [SerializeField] float laserWidthDecayTime;
        [SerializeField] float laserMaxDistance;
        [SerializeField] bool firingLaser;
        [SerializeField] float attackIntervalMultiplier;
        [SerializeField] float laserRampTime;
        [SerializeField] float laserSpherecastWidth;
        protected override void Start()
        {
            base.Start();
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        IEnumerator Laser()
        {
            //Get everything ready for the laser
            laserWarmupParticle.Play(true);
            float time = 0;
            float duration = laserWarmupParticle.main.duration - 0.5f;
            var lr = Instantiate(laserBeam);
            var wff = new WaitForFixedUpdate();
            //Start charging the laser
            while (time < duration)
            {
                lr.widthMultiplier = Mathf.Lerp(0, maxLaserWidthOverChargeup, Mathf.InverseLerp(0, duration, time));
                Physics.Raycast(transform.position, transform.forward, out RaycastHit hit1, laserMaxDistance);
                lr.SetPosition(0, transform.position);
                if (hit1.collider)
                {
                    lr.SetPosition(1, hit1.point);
                }
                else
                {
                    lr.SetPosition(1, transform.position + transform.forward * laserMaxDistance);
                }
                time += Time.fixedDeltaTime;
                yield return wff;
            }
            time = 0;
            //Fire the laser
            if (Physics.SphereCast(transform.position, laserSpherecastWidth, transform.forward, out RaycastHit hit, laserMaxDistance, GameManager.instance.playerMask)) 
            { 
                if (hit.rigidbody)
                {
                    hit.rigidbody.GetComponent<Player>().ModifyHealth(-outgoingDamage / (hit.distance / 1.05f));
                    hit.rigidbody.AddForce(outgoingDamage * transform.forward);
                }
            }
            while (time < laserRampTime)
            {
                lr.widthMultiplier = Mathf.Lerp(maxLaserWidthOverChargeup, laserWidthOnFire, Mathf.InverseLerp(0, laserRampTime, time));
                time += Time.fixedDeltaTime;
                yield return wff;
            }
            time = 0;
            while (time < laserWidthDecayTime)
            {
                lr.widthMultiplier = Mathf.Lerp(laserWidthOnFire, 0, Mathf.InverseLerp(0, laserWidthDecayTime, time));
                time += Time.fixedDeltaTime;
                yield return wff;
            }
            Destroy(lr.gameObject, 1);
            yield return new WaitForSeconds(GameManager.instance.outgoingDamageInterval * 2);
            firingLaser = false;
        }
        protected override void Move()
        {
            //We need full custom movement for the flying enemy.
            //base.Move();
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxHeight);
            currentHeight = hit.distance;

            pidForce.x = xPidPos.GetOutput(agent.nextPosition.x - transform.position.x, Time.fixedDeltaTime, pidForceMinMax) * moveForce;
            pidForce.y = yPidPos.GetOutput((agent.nextPosition.y + targetHeight) - currentHeight , Time.fixedDeltaTime, pidForceMinMax) * verticalForce;
            pidForce.z = zPidPos.GetOutput(agent.nextPosition.z - transform.position.z, Time.fixedDeltaTime, pidForceMinMax) * moveForce;

            rb.AddForce(pidForce);
            transform.forward = Vector3.RotateTowards(transform.forward, agent.destination - transform.position, lookSpeed * Time.fixedDeltaTime, 5);
        }
        protected override void GetTarget()
        {
            //Line of Sight checks from the Chaser enemy
            base.GetTarget();
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected override void AttackPlayer()
        {
            //We need custom attack implementation
            //base.AttackPlayer();
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, GameManager.instance.playerMask) && !firingLaser)
            {
                StartCoroutine(Laser());
                firingLaser = true;
            }
        }
        
    }
}