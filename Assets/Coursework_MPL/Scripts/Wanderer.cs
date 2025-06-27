using StarlightLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightLib
{
    public class Wanderer : Enemy
    {
        [SerializeField] GameObject wanderingFace, angryFace;
        [SerializeField] float wanderpointRetryTime;
        protected override void Start()
        {
            base.Start();
            InvokeRepeating(nameof(GetNewPosition), 0, wanderpointRetryTime);
        }
        void GetNewPosition()
        {
            agent.SetDestination(GameManager.instance.GetRandomWaypoint().position);
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected override void GetTarget()
        {
            base.GetTarget();
        }
        protected void Morph()
        {
            //Morphs this wanderer into a strong and scary Chaser
            if (!TryGetComponent(out Chaser ch))
            {
                CreateScaryChaser();
            }
            else
            {
                CreateScaryChaser(ch);
                ch.enabled = false;
            }
            wanderingFace.SetActive(false);
            angryFace.SetActive(true);
            ch.enabled = true;
            enabled = false;
        }
        void CreateScaryChaser(Chaser chaser = null)
        {
            if (chaser == null)
            {
                chaser = gameObject.AddComponent<Chaser>();
            }
            chaser.maxHealth = 100;
            chaser.transform.localScale = Vector3.one * 2;
            chaser.stunTime = 4;
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            //The wanderers are grouchy beasts. They won't hurt you until you make contact with them.
            if (collision.gameObject.CompareTag("Player"))
            {
                Morph();
            }
        }
    }
}