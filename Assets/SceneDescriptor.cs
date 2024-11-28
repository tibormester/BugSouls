using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDescriptor : MonoBehaviour{
    public GameObject player; // Reference to the player object
    public CameraController cam; // Reference to the camera controller
    public List<GraphEdge> exits; // Exits connecting to other scenes

    public event Action<Transform> PlayerEntered; //When a player enters, broadcast this event so AI scripts can update and track the new transform

    private void Start(){
        // Subscribe to player's death event to reload the current scene 
        if (player.TryGetComponent(out Health playerHealth)){
            playerHealth.DeathEvent += ReloadCurrentScene;
        }

        // Subscribe to collision events for each exit
        foreach (var exit in exits){
            exit.SubscribeToCollision(OnExitEntered);
        }
    }

    private void ReloadCurrentScene(){
        //Since this is additive, it should reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //I want to cache the initial game state on enter and restore it here
    }

    public void OnExitEntered(GraphEdge edge, GameObject collidingObject){
        if (collidingObject != player) return; // Ensure it's the player
        if (edge.locked && !AreAllEnemiesCleared()) return; // Check if exit is locked

        // Transition to the next scene
        //SceneManager.sceneLoaded += MoveToScene; Dont need if we use a coroutine and wait until loading is done
        StartCoroutine(LoadSceneAsync(edge));
    }

    private IEnumerator LoadSceneAsync(GraphEdge edge){
        // Begin asynchronous loading of the next scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(edge.sceneName, LoadSceneMode.Additive);
        yield return new WaitUntil( () => asyncLoad.isDone);

        // Move the player to the next scene and set it to active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(edge.sceneName));
        SceneDescriptor nextSceneDescriptor = FindObjectOfType<SceneDescriptor>();
        nextSceneDescriptor?.RecievePlayer(player, edge.entranceLocation.position, edge.entranceLocation.rotation);
        //Remove the link to the player (so we dont accidentally destroy it)
        player = null;
        //Stop this scene from processing
        PauseScene();
    }

    public void RecievePlayer(GameObject transientPlayer, Vector3 position, Quaternion rotation){
        // Replace existing player with the incoming player
        
        SceneManager.MoveGameObjectToScene(player, SceneManager.GetActiveScene());//This should be the active scene
        if (player != null){
            Destroy(player);
        }
        transientPlayer.transform.position = position;
        transientPlayer.transform.rotation = rotation;
        player = transientPlayer;
        // Adjust the camera to target the new player
        cam.target = player.transform;
        PlayerEntered?.Invoke(player.transform);//Let all AI's know we got a new player transform in town
        //Start the scene if it was paused (if it wasn't this shouldnt change anything)
        PauseScene(true);
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

[System.Serializable]
public struct GraphEdge
{
    public Collider collider; // Collider for detecting player interaction
    public string sceneName; // Name of the connected scene
    public bool locked; // Is this exit locked?
    public Transform entranceLocation; //A position and rotation in the next scene's local coordinates where the player should be

    public void SubscribeToCollision(System.Action<GraphEdge, GameObject> callback){
        var triggerHandler = collider.gameObject.AddComponent<TriggerHandler>();
        GraphEdge self = this;
        triggerHandler.onExitEnteredEvent += (collidingObject) => callback(self, collidingObject);
    }
}

// A class to expose the unity's collider OnTriggerEnter event to C# events 
public class TriggerHandler : MonoBehaviour{
    public event Action<GameObject> onExitEnteredEvent; //C# event that wraps OnTriggerEnter into OnExitEnteredEvent(GameObject player)
    private void OnEnable() {
        //Do setup here to reduce work done in editor
        Collider collider = GetComponent<Collider>();
        collider.excludeLayers = LayerMask.GetMask(new string[]{"Everything"});
        collider.includeLayers = LayerMask.GetMask(new string[]{"Player"});
        collider.isTrigger = true;
    }
    //Wraps Unity's OnTriggerEnter call to our C# event Action<Player> on ExitEnteredEvent
    private void OnTriggerEnter(Collider other){
        //Should be safe since the player layer should only ever have the player with their rigidbody
        onExitEnteredEvent?.Invoke(other.attachedRigidbody.gameObject);
    }
}
