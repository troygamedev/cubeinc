using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class CollapseScript : MonoBehaviour {

    GameController game;
    Rigidbody rbody;
    AddToAllowedLocations allowed;
    Vector3 originalPos;
    int moveWhenFell;

    void Start(){
        rbody = GetComponent<Rigidbody>();
        rbody.useGravity = false;
        rbody.isKinematic = true;
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        allowed = GetComponent<AddToAllowedLocations>();
        originalPos = transform.position;
    }

    void OnTriggerExit(Collider other){
        if(other.CompareTag("Player Collider")){
            if(!PlayerScript.currentlyRewinding) {
                Vector3 roundPos = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z)) + Vector3.up * allowed.yOffset;
                if (!game.allowedLocations.ContainsKey(roundPos)) {
                    game.allowedLocations.Add(roundPos, false);
                } else {
                    game.allowedLocations[roundPos] = false;
                }
                rbody.useGravity = true;
                rbody.isKinematic = false;
                moveWhenFell = PlayerScript.moveCount;
            }            
        }
    }

    void path_OnRewind() {
        if (PlayerScript.moveCount == moveWhenFell+1) {
            transform.position = originalPos;
            rbody.useGravity = false;
            rbody.isKinematic = true;

            Vector3 roundPos = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z)) + Vector3.up * allowed.yOffset;
            if (!game.allowedLocations.ContainsKey(roundPos)) {
                game.allowedLocations.Add(roundPos, true);
            } else {
                game.allowedLocations[roundPos] = true;
            }
        }
    }
}
