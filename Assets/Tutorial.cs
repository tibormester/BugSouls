using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{   
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        StartCoroutine(TutorialText());
    }
    public IEnumerator TutorialText(){
        var wait = new WaitForSeconds(0.5f);
        yield return ReadLine("Double Tap WASD to Dash,\nPress SPACE BAR to Jump");
        yield return  new WaitUntil(() => Input.GetKeyDown(KeyCode.Space)); 
        yield return wait;
        yield return ReadLine("Press E to Shoot Webs,\nHold Q to Throw Items");
        yield return  new WaitUntil(() => Input.GetKeyDown(KeyCode.E)); 
        yield return  new WaitUntil(() => Input.GetKeyUp(KeyCode.Q));
        yield return wait;
        yield return ReadLine("Spider Ants Can Walk Up Walls and Swing From Webs, \nPress SPACE BAR to Dismount From Webs");
        yield return  new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
        yield return wait;
        yield return ReadLine("Left Click to Attack");
        yield return  new WaitUntil(() => Input.GetKeyUp(KeyCode.Mouse0));
        yield return wait;
    }

    public IEnumerator ReadLine(string line){
        //time after each character
        var charWait = new WaitForSeconds(0.01f);
        //how to get to the next line
        var lineWait = new WaitUntil( () => Input.GetKeyDown(KeyCode.Tab));
        text.text = "";
        foreach(char letter in line){
            text.text += letter;
            yield return charWait;
        }
    }
}
