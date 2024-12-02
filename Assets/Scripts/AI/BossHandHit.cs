using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHandHit : MonoBehaviour
{
    public VineBossAIV2 parent;

    private void OnTriggerEnter(Collider other)
    {
        if (parent.dying || !other.CompareTag("Player")) return;
        

        Health playerHealth = other.GetComponent<Health>();
        playerHealth?.ApplyDamage(parent.damage);

        var direction = playerHealth.transform.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, playerHealth.transform.up); //flatten so we dont hop on hit
        playerHealth.GetComponent<Rigidbody>()?.AddForce(direction.normalized * parent.knockbackForce, ForceMode.Impulse);
    }
}
