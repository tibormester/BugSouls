using System.Collections;
using System.Linq;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class VineBossAIV2 : MonoBehaviour
{
    private Animator vineBossAnimator;
    private Health health;
    public float attackInterval = 5f;       

    // Attack properties
    public float attackRange = 2f;         // Range within which the attack affects the player
    public float damage = 10f;             // Damage dealt to the player
    public float knockbackForce = 5f;      // Force applied to the player for knockback
    public LayerMask playerLayer;          // Layer mask to identify the player

    public Transform target;
    public bool dying = false;
    public static int swarmCount = 5;
    public GameObject swarmBug;
    public GameObject swarmLineStart;
    public GameObject swarmLineEnd;
    public GameObject spine;
    public Image gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public float secondsToFade = 2f;



    private Vector3 swarmLineStartPos;
    private Vector3 swarmLineEndPos;


    private static int numAttacks = 5;

    private bool firstTime = true;
    private bool firstTimeAttack = true;
    private bool lastAttackWasSwarm = false;
    private bool swarmDead = true;
    private string currentAnimation = "bossIdle";
    private float timeSinceAttack = 0f;
    private string[] attackNames = new string[numAttacks];
    private float[] attackDurations = new float[numAttacks];
    private GameObject[] swarm = new GameObject[swarmCount];

    void Start()
    {
        vineBossAnimator = GetComponent<Animator>();
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;
        swarmLineStartPos = swarmLineStart.transform.position;
        swarmLineEndPos = swarmLineEnd.transform.position;

        health = GetComponent<Health>();
        health.DeathEvent += () => { 
            vineBossAnimator.CrossFade("die", 0.1f); dying = true; 
            
        };

        health.Damaged += () => {
            if (Vector3.Distance(spine.transform.position, target.transform.position) < 25f)
            {
                var direction = target.transform.position - spine.transform.position;
                direction = Vector3.ProjectOnPlane(direction, target.transform.up); //flatten so we dont hop on hit
                target.GetComponent<Rigidbody>()?.AddForce(direction.normalized * 4f * knockbackForce, ForceMode.Impulse);
            }
        };

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
            if (!swarmDead)
            {
                bool allDead = true;
                foreach (GameObject bug in swarm)
                {
                    Health dummy;
                    if (bug.TryGetComponent<Health>(out dummy) && bug.transform.position.y > 450f)
                    {
                        allDead = false;
                    }

                }
                swarmDead = allDead;
            }


            if (swarmDead && timeSinceAttack >= attackInterval && timeSinceAttack >= attackTime * 0.9f) {

                if (firstTimeAttack)
                {
                    firstTimeAttack = false;
                    SwarmAttack();


                    Debug.Log("Attacking with swarm");
                } else {
                    int randomAnim = Random.Range(0, numAttacks + (lastAttackWasSwarm ? 0 : 2));

                    if (randomAnim >= numAttacks)
                    {
                        SwarmAttack();

                        Debug.Log("Attacking with swarm");
                    }
                    else
                    {
                        lastAttackWasSwarm = false;
                        ChangeAnimation(attackNames[randomAnim]);
                        attackTime = attackDurations[randomAnim];
                        Debug.Log("Attacking with " + attackNames[randomAnim]);
                    }
                }

                timeSinceAttack = 0f;
            } else {
                if (timeSinceAttack >= attackTime * 0.9f) ChangeAnimation("bossIdle");
                timeSinceAttack += Time.deltaTime;
            }


            yield return null;
        }

        yield return new WaitForSeconds(3f);
        float fadeToBlack = 0f;
        while (dying && fadeToBlack < 1f)
        {

            gameOverScreen.color = new Color(0, 0, 0, fadeToBlack);
            gameOverText.color = new Color(1f, 1f, 1f, fadeToBlack);
            fadeToBlack += Time.deltaTime * 1f / secondsToFade;

            yield return null;
        }

        Time.timeScale = 0f;
    }

    public void SwarmAttack()
    {
        swarmDead = false;
        lastAttackWasSwarm = true;

        for (int i = 0; i < swarmCount; i++) {
            GameObject bug = Instantiate(swarmBug);
            
            float x = i * ((swarmLineEndPos.x - swarmLineStartPos.x) / ((float) swarmCount)) + swarmLineStartPos.x;
            bug.transform.position = new Vector3(x, swarmLineStartPos.y, swarmLineStartPos.z);
            bug.transform.LookAt(target.position);

            GeneralAI bugAI = bug.GetComponent<GeneralAI>();
            bugAI.RecievePlayer(target);


            swarm[i] = bug;
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
