using System.Collections;
using TMPro;
using UnityEngine;

public class MatriarchDialogueScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Health health;
    public GeneralAI ai;
    public TextMeshPro text;
    public CharacterMovement charMovement;
    public GameObject SpiderlingPrefab;
    public Vector3[] SpawnLocations = new Vector3[] {Vector3.right, Vector3.left};
    public string[] dialogue = new string[] {
        "The corruption... it twists, it consumes. I fought to protect...\nBut I only delayed the inevitable. ",
    };
    public int heals = 1;
    void Start(){
        text = GetComponent<TextMeshPro>();
        charMovement = transform.parent.GetComponent<CharacterMovement>();

        //Setup the spider to stop talking and attack if damaged
        health = transform.parent.GetComponent<Health>();
        ai = transform.parent.GetComponent<GeneralAI>();
        var droppable = ai.GetComponent<Droppable>();

        
        health.DeathEvent += () => {
            StartCoroutine(ReadDialogue());
        };
        //Spawn a spider at each spawn location whenever the matriarch drops her crown
        droppable.DroppedItem += (GameObject crown) => {
            foreach (var direction in SpawnLocations){
                var spider = Instantiate(SpiderlingPrefab, gameObject.transform.parent.position + gameObject.transform.TransformDirection(direction), gameObject.transform.parent.rotation);
                spider.GetComponent<GeneralAI>().target = ai.target;
            }
        };
        //Heal whenever we pickup the crown and if we are out of heals stop trying to retrieve it
        ai.BehaviourFinished += (AIBehaviour behaviour) => {
            if (behaviour is RetrieveItem) {
                if (droppable.item != null){//Have to check since retrieve item doesn't always end in success
                    health.ApplyDamage(-.5f * health.maxHealth);
                    heals -= 1;
                    if (heals < 0){
                        ai.AIBehaviours.Remove(behaviour);
                    }
                }
            }
        };
        
    }
    private void Update(){
        text.transform.LookAt(ai.target);
    }
    public IEnumerator ReadDialogue(){
        //time after each character
        var charWait = new WaitForSeconds(0.01f);
        //how to get to the next line
        var lineWait = new WaitUntil( () => Input.GetKeyDown(KeyCode.Tab)); 
        foreach(string line in dialogue){
            foreach(char letter in line){
                text.text += letter;
                charMovement.look_direction = ai.target.position - transform.parent.position;
                yield return charWait;
            }
            yield return lineWait;
            text.text = "";
        }

    }




}
