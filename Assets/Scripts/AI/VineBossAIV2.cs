using System.Collections;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class VineBossAIV2 : MonoBehaviour
{
    private Animator vineBossAnimator;
    public float changeInterval = 5f;

    // Attack properties
    public float attackRange = 2f;         // Range within which the attack affects the player
    public float damage = 10f;             // Damage dealt to the player
    public float knockbackForce = 5f;      // Force applied to the player for knockback
    public LayerMask playerLayer;          // Layer mask to identify the player

    public Transform target;

    private bool dying = false;

    void Start()
    {
        vineBossAnimator = GetComponent<Animator>();
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;

        ChangeAnimation("Boss Entry");
    }

    //Place to add listeners for player events
    public virtual void RecievePlayer(Transform player)
    {
        target = player;
        target.GetComponent<Health>().DeathEvent += OnPlayerKilled;
        //Starts the Enemy's AI
        StartCoroutine(AttackLoop());
        //Has to be here otherwise it doesnt restart
    }
    //When the player dies, stop attacking the corpse
    public virtual void OnPlayerKilled()
    {
        target = null;
    }

    IEnumerator AttackLoop()
    {
        yield return null;
    }

    private string currentAnimation;
    public bool ChangeAnimation(string animationName)
    {
        if (dying)
        {
            return false;
        }
        if (currentAnimation == animationName)
        {
            return false;
        }

        vineBossAnimator.CrossFade(animationName, 0.1f, 0);
        currentAnimation = animationName;
        return true;
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
