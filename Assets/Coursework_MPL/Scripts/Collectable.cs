using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightLib
{
    public class Collectable : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<Player>();
                player.AddMoveForce();
                player.ModifyHealth(GameManager.instance.pickupHealth);
                Destroy(gameObject);
                GameManager.instance.UpdateScore();
            }
        }
    }
}