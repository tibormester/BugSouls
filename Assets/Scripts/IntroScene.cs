using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Mouse0)){
            SceneManager.LoadSceneAsync("TutorialScene", LoadSceneMode.Single);
        }
    }
}
