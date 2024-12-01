using System.Collections;
using UnityEngine;

public class VineBossAI : MonoBehaviour
{
    private Animator vineBossAnimator;
    public float changeInterval = 5f;

    // Attack properties
    public float attackRange = 2f;         // Range within which the attack affects the player
    public float damage = 10f;             // Damage dealt to the player
    public float knockbackForce = 5f;      // Force applied to the player for knockback
    public LayerMask playerLayer;          // Layer mask to identify the player

    void Start()
    {
        vineBossAnimator = GetComponent<Animator>();
        StartCoroutine(ChangeAnimation());
    }

    IEnumerator ChangeAnimation()
    {
        while (true)
        {
            // Randomly select an attack animation (assuming animations are indexed from 0 to 4)
            int randomAnim = Random.Range(0, 5);
            vineBossAnimator.SetInteger("attackselector", randomAnim);

            // Wait for the animation to process
            yield return null;

            // Reset the attack selector to avoid unintended animations
            vineBossAnimator.SetInteger("attackselector", -1);

            // Wait before switching to the next attack
            yield return new WaitForSeconds(changeInterval);
        }
    }

    // This method should be called via animation events at the exact frame when the attack hits
    public void Attack()
    {
        // Detect the player within the attack range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

        foreach (var hitCollider in hitColliders)
        {
            // Ensure we're hitting the player
            if (hitCollider.CompareTag("Player"))
            {
                // Apply damage
                Health playerHealth = hitCollider.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.ApplyDamage(damage);
                }

                // Apply knockback
                Rigidbody playerRb = hitCollider.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    knockbackDirection.y = 0; // Prevent vertical knockback
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }

    // Optional: Visualize the attack range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
