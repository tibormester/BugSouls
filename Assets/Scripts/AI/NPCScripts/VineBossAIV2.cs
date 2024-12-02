using System.Collections;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class VineBossAIV2 : MonoBehaviour
{
    private Animator vineBossAnimator;
    public float attackInterval = 5f;       

    // Attack properties
    public float attackRange = 2f;         // Range within which the attack affects the player
    public float damage = 10f;             // Damage dealt to the player
    public float knockbackForce = 5f;      // Force applied to the player for knockback
    public LayerMask playerLayer;          // Layer mask to identify the player

    public Transform target;

    private static int numAttacks = 5;

    public bool dying = false;
    private bool firstTime = true;
    private string currentAnimation = "bossIdle";
    private float timeSinceAttack = 0f;
    private string[] attackNames = new string[numAttacks];
    private float[] attackDurations = new float[numAttacks];

    void Start()
    {
        vineBossAnimator = GetComponent<Animator>();
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;

        GetComponent<Health>().DeathEvent += () => { vineBossAnimator.CrossFade("die", 0.1f); dying = true; };

        AnimationClip[] clips = vineBossAnimator.runtimeAnimatorController.animationClips;

        int attacksIndex = 0;
        foreach (AnimationClip clip in clips) {
            if (clip.name.Contains("Hand")) {
                attackNames[attacksIndex] = clip.name;

                attackDurations[attacksIndex] = clip.length / vineBossAnimator.GetFloat("baseSpeed");

                attacksIndex++;
            }
        }
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

    private float attackTime = 8f;
    IEnumerator AttackLoop()
    {
        if (firstTime)
        {
            ChangeAnimation("bossEntry");
            firstTime = false;
        }

        while (!dying)
        {
            if (timeSinceAttack >= attackInterval && timeSinceAttack >= attackTime * 0.9f) {
                int randomAnim = Random.Range(0, numAttacks);
                ChangeAnimation(attackNames[randomAnim]);
                attackTime = attackDurations[randomAnim];
                Debug.Log("Attacking with " + attackNames[randomAnim]);

                timeSinceAttack = 0f;
            } else {
                if (timeSinceAttack >= attackTime * 0.9f) ChangeAnimation("bossIdle");
                timeSinceAttack += Time.deltaTime;
            }


            yield return null;
        }
    }

    public bool ChangeAnimation(string animationName)
    {
        if (dying || currentAnimation == animationName)
        {
            return false;
        }

        var state = vineBossAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.normalizedTime < 0.9f && currentAnimation != "bossIdle")
        {
            return false;
        }

        Debug.Log("Crossfading between: " + currentAnimation + " at " + state.normalizedTime + " and " + animationName);

        vineBossAnimator.CrossFade(animationName, 0.1f, 0);
        currentAnimation = animationName;
        return true;
    }

}
