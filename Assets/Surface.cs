using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    // Start is called before the first frame update
    public Status MakeStatus(GameObject obj){
        
        return new SlidingStatus(obj);
    }
    public Status GetStatus(List<Status> statuses){
        foreach (Status status in statuses){
            if (status is SlidingStatus){
                return status;
            }
        }
        return null;
    }
}
