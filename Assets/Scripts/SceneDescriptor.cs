using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneDescriptor : MonoBehaviour{
    public GameObject player; // Reference to the player object
    public CameraController cam; // Reference to the camera controller
    public List<GraphEdge> exits; // Exits connecting to other scenes

    //When we restart a scene, remeber how we entered and where we entered from
    public GraphEdge prevEdge;
    public GameObject cachedPlayer;

    public event Action<Transform> PlayerEntered; //When a player enters, broadcast this event so AI scripts can update and track the new transform

    private void Start(){
        StartCoroutine(DelayedStart());
        canvas = gameObject.scene.GetRootGameObjects()
                .Select(go => go.GetComponent<Canvas>()) //Find the screen descriptor components
                .FirstOrDefault(desc => desc != null);// return the first one that isnt null
    }
    private IEnumerator DelayedStart(){
        yield return null;
        
        if (cachedPlayer == null && player != null){
            //normally this is done during the transition, so if not then we are probably running this scene in the editor
            //This will never run in build i think
            cachedPlayer = (GameObject)Instantiate(player, gameObject.scene);
            cachedPlayer.SetActive(false);
            player.GetComponent<Health>().DeathEvent += ReloadCurrentScene;
        }
        if(prevEdge.sceneName == ""){
            prevEdge.entranceLocation = transform;
            prevEdge.sceneName = gameObject.scene.name;
        }  
        foreach (var edge in exits){
            edge.SetupColliders();
        }
        PlayerEntered?.Invoke(player.transform);
    }

    private void ReloadCurrentScene(){
        StartCoroutine(ReloadCoroutine());
    }
    private Canvas canvas;
    private IEnumerator ReloadCoroutine(){

        canvas.GetComponent<Text>().text = "\n\nYou Went Splat!";

        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));
        StartCoroutine(LoadSceneAsync(prevEdge, true));
    }

    public void OnExitEntered(GraphEdge edge, GameObject collidingObject){
        if (collidingObject != player) return; // Ensure it's the player
        if (edge.locked && !AreAllEnemiesCleared()) return; // Check if exit is locked
        if(collidingObject.GetComponent<Health>().currentHealth <= 0f){
            return;//Bug where corpse gets pushed through a door, (could be a feature)
        }
        if(edge.sceneName == gameObject.scene.name){
            player.transform.position = edge.entranceLocation.transform.position;
            player.transform.rotation = edge.entranceLocation.transform.rotation;
        }else{
            StartCoroutine(LoadSceneAsync(edge));
        }
    }

    private IEnumerator LoadSceneAsync(GraphEdge edge, bool reload = false){
        // UnSubscribe to player's death event to reload the current scene 
        if (player.TryGetComponent(out Health playerHealth)){
            playerHealth.DeathEvent -= ReloadCurrentScene;
        }
        //Check if the scene needs to be loaded
        Scene nextScene = SceneManager.GetSceneByName(edge.sceneName);
        if (!nextScene.IsValid() || reload){//Of course unity has to be special and doesn't want to just return null or include better documentation
            // Begin asynchronous loading of the next scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(edge.sceneName, LoadSceneMode.Additive);
            yield return new WaitUntil( () => asyncLoad.isDone);
            //Because scene management in unity is painful, we need this loop to make sure we get the new scene when restarting
            for (int i = 0; i < SceneManager.sceneCount; i++){
                Scene scene = SceneManager.GetSceneAt(i);
                if(scene.name == edge.sceneName && scene != gameObject.scene){
                    nextScene = scene;
                }
            }
        }
        
        PauseScene();

        SceneDescriptor nextSceneDescriptor = nextScene.GetRootGameObjects()
                .Select(go => go.GetComponent<SceneDescriptor>()) //Find the screen descriptor components
                .FirstOrDefault(desc => desc != null);// return the first one that isnt null

        nextSceneDescriptor.RecievePlayer(reload ? cachedPlayer : player, edge);
        //Set the lighting to be from the next scene now that it is loaded and stuff
        SceneManager.SetActiveScene(nextScene);
        
        //Remove the link to the player (so we dont accidentally destroy it)
        player.GetComponent<Health>().DeathEvent -= ReloadCurrentScene;
        player = null;
        
        if(reload){
            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(gameObject.scene);
            yield return new WaitUntil( () => asyncLoad.isDone); //Does this even get caled?
            Debug.LogWarning("Restarted abd unloaded original:" + gameObject.scene.name);
        } else{ //If we reload the scene gets destroyed anyways
            Destroy(cachedPlayer);//No need to store this anymore
            cachedPlayer = null;
        }
    }

    public void RecievePlayer(GameObject transientPlayer, GraphEdge edge, bool reload = false){
        Vector3 position = edge.entranceLocation.position;
        Quaternion rotation = edge.entranceLocation.rotation;
        prevEdge = edge;

        SceneManager.MoveGameObjectToScene(transientPlayer, gameObject.scene);

        //If there is an exisitng player remove it to make room for the new player
        if (player != null && player != transientPlayer){
            Destroy(player);
        }
        //Put the new player at the right position
        transientPlayer.transform.position = position;
        transientPlayer.transform.rotation = rotation;

        player = transientPlayer;
        player.SetActive(true);
        // Adjust the camera to target the new player
        cam.target = player.transform;
        PlayerEntered?.Invoke(player.transform);//Let all AI's know we got a new player transform in town
        //Move the health bar reference to the current scene's canvas
        //Ik this is very messy but the deadline is in 4 days
        HealthBar hbar = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponentInChildren<HealthBar>()).FirstOrDefault(desc => desc != null);
        Health health = transientPlayer.GetComponent<Health>();
        health.health = hbar;
        health.health.setMaxHealth(health.maxHealth);
        health.health.setHealth(health.currentHealth);
        //Create an inactive copy of the player and store it incase we need to restart the next scene
        if (cachedPlayer != null && cachedPlayer.gameObject.scene == gameObject.scene){
            Destroy(cachedPlayer); 
        }
        cachedPlayer = (GameObject)Instantiate(transientPlayer, gameObject.scene);
        
        //Add an event handler to the current player so we can reload the scene on death
        health.DeathEvent += ReloadCurrentScene;
        
        //Start the scene if it was paused (if it wasn't this shouldnt change anything)
        PauseScene(true);
        cachedPlayer.SetActive(false); //Set the savepoint player to inactive
    }

    private bool AreAllEnemiesCleared(){
        // Check if all enemies in the scene are cleared
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1){
            Debug.LogError("Enemy layer not found");
            return false;
        }
        return FindObjectsOfType<Transform>().All(t => t.gameObject.layer != enemyLayer);
    }
    //Pause a scene on enter and exit
    public void PauseScene(bool unPause = false){
        foreach (var obj in gameObject.scene.GetRootGameObjects()){
            obj.SetActive(unPause);
        }
        if(unPause){
            cam.gameObject.tag = "MainCamera";
        } 
    }
}

[Serializable]
public struct GraphEdge
{
    public List<Collider> colliders; // Collider for detecting player interaction
    public string sceneName; // Name of the connected scene
    public bool locked; // Is this exit locked?
    public Transform entranceLocation; //A position and rotation in the next scene's local coordinates where the player should be

    public void SetupColliders(){
        foreach (Collider collider in colliders){
            var triggerHandler = collider.gameObject.AddComponent<TriggerHandler>();
            triggerHandler.edge = this;
        }
    }
}

// A class to expose the unity's collider OnTriggerEnter event to C# events 
public class TriggerHandler : MonoBehaviour{
    public event Action<GameObject> onExitEnteredEvent; //C# event that wraps OnTriggerEnter into OnExitEnteredEvent(GameObject player)
    private SceneDescriptor sceneDescriptor;
    public GraphEdge edge;
    private void OnEnable() {
        //Do setup here to reduce work done in editor
        Collider collider = GetComponent<Collider>();
        collider.includeLayers = LayerMask.GetMask(new string[]{"Player"});
        collider.isTrigger = true;

        sceneDescriptor = gameObject.scene.GetRootGameObjects()
                .Select(go => go.GetComponent<SceneDescriptor>()) 
                .FirstOrDefault(desc => desc != null);
        
    }
    //Wraps Unity's OnTriggerEnter call to our C# event Action<Player> on ExitEnteredEvent
    //Changed to not use events because then i have to unsubscribe to them
    private void OnTriggerEnter(Collider other){
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")){//Kinda uneccessary since we check if its the player later
            sceneDescriptor.OnExitEntered(edge, other.attachedRigidbody.gameObject);
        }
    }
}
