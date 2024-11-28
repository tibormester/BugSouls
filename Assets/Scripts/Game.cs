using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Store the game state in this object
//Make it singleton like that there is only ever a single active one

public class Game : ScriptableObject{

    public List<string> sceneNames;
    public int sceneIndex = 0;

    public GameObject player;
    public GameObject camera;


    private static Game activeGame; 

    public static Game getGame(){
        if (activeGame != null){
            return activeGame;
        } else{
            Game newGame = new Game();
            activeGame = newGame;
            return newGame;
        }
    }

    public void Awake(){
        if(activeGame == null){
            activeGame = this;
        } else if (activeGame != this){
            Debug.LogError("Some other game is loaded and we are trying to load this game");
        }
        SceneManager.sceneLoaded +=OnSceneLoaded;//Register event handler so we can transition asynch
    }
    public void TransitionScene(string sceneName){

        //Load the next scene asynch
        
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode){
        //Move the player data to the next scene
        
        //Set this scene to active

        //Unload the last scene
    }
    
}
