using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class TeleportScript : MonoBehaviour {

    Transform playerTransform;
    PlayerScript play;
    public GameObject targetTeleport;
    TeleportScript targetScript;
    Transform targetTransform;
    [HideInInspector] public bool allowTeleport = true;
    
    void Start() {
        targetScript = targetTeleport.GetComponent<TeleportScript>();
        targetTransform = targetTeleport.transform.GetChild(0);
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0);
        play = playerTransform.GetComponent<PlayerScript>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player Collider") && !PlayerScript.isFall) {
            if (allowTeleport)
                StartCoroutine(Teleport());
        }
    }

    void OnTriggerExit(Collider other) {
        if(!PlayerScript.isFall)
            allowTeleport = true;
    }
    
    IEnumerator Teleport() {
        int moveCountAtTrigger = PlayerScript.moveCount;
        yield return new WaitForSeconds(0.1f);
        if (PlayerScript.detectTeleports){
            play.justTeleported = true;
            PlayerScript.detectTeleports = false;
            play.teleportHistory.Add(moveCountAtTrigger + 1, GameController.RoundedVectorInt(transform.GetChild(0).position));
            yield return new WaitUntil(() => PlayerScript.allowRotate);
            targetScript.allowTeleport = false;
            playerTransform.position = targetTransform.position;
        }
        
    }
}
