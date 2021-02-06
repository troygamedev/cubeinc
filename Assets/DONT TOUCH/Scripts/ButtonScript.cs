using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public enum ButtonMode{
    toggle, activateOnly, deactivateOnly, hold
}

public class ButtonScript : MonoBehaviour {

    public PathMapTo3D pathBlueprint;
    public bool isOneUse;
    public ButtonMode mode;
    public bool isColorSpecific = false;

    bool isUsed = false;

    public GameObject launchTube;
    TubeScript tube;

    ColorController colorCon;
	// Use this for initialization
	void Start () {
        colorCon = GetComponentInChildren<ColorController>();
        if(launchTube != null)
            tube = launchTube.GetComponent<TubeScript>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    Collider colliderCurrentlyOnButton;

    void OnTriggerEnter(Collider other){
        CheckCollider(other);
    }

    void CheckCollider(Collider other) {
        if (!isUsed) {
            if (other.CompareTag("Player Collider") || other.CompareTag("Moveable Collider")) {
                colliderCurrentlyOnButton = other;
                if (!isColorSpecific || (other.CompareTag("Player Collider") && isColorSpecific && PlayerScript.playerColorCon.thisColorIndex == colorCon.thisColorIndex) || (other.CompareTag("Moveable Collider") && isColorSpecific && other.transform.parent.GetComponentInChildren<ColorController>().thisColorIndex == colorCon.thisColorIndex)) {
                    if (pathBlueprint != null) {
                        switch (mode) {
                            case ButtonMode.toggle:
                                pathBlueprint.SetPathActive(!pathBlueprint.currentlyEnabled, PlayerScript.currentlyRewinding || PlayerScript.justFell, false);
                                break;
                            case ButtonMode.activateOnly:
                                pathBlueprint.SetPathActive(true, PlayerScript.currentlyRewinding || PlayerScript.justFell, false);
                                break;
                            case ButtonMode.deactivateOnly:
                                pathBlueprint.SetPathActive(false, PlayerScript.currentlyRewinding || PlayerScript.justFell, false);
                                break;
                            case ButtonMode.hold:
                                pathBlueprint.SetPathActive(true, PlayerScript.currentlyRewinding || PlayerScript.justFell, false);
                                break;
                            default:
                                break;
                        }
                    }

                    if (launchTube != null) {
                        if(!PlayerScript.currentlyRewinding)
                            tube.StartCoroutine("Launch");
                    }
                }
                if (isOneUse) {
                    isUsed = true;
                }
            }
        }
    }

    void OnTriggerExit(){
        if(mode == ButtonMode.hold && pathBlueprint != null){
            pathBlueprint.SetPathActive(false, PlayerScript.currentlyRewinding || PlayerScript.justFell, false);
        }
        colliderCurrentlyOnButton = null;
    }


}
