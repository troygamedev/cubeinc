using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class DetectorScript : MonoBehaviour {


	// Use this for initialization
	void Start () {
    }

    // Update is called once per frame
    void Update () {
	}

    void OnTriggerEnter(Collider other){        
        if (other.gameObject.layer == 9) { //non passable objects layer
            if(other.gameObject.tag == "Gate"){
                if (other.gameObject.GetComponent<ColorController>().thisColorIndex != PlayerScript.playerColorCon.thisColorIndex){
                    if (name == "Front") {                        
                        PlayerScript.allowForward = false;
                    }
                    if (name == "Back") {
                        PlayerScript.allowBack = false;
                    }
                    if (name == "Left") {
                        PlayerScript.allowLeft = false;
                    }
                    if (name == "Right") {
                        PlayerScript.allowRight = false;
                    }
                }
            } else {
                StartCoroutine(SetAllowDirection(gameObject.name, false));
            }
        }
    }

    void OnTriggerExit(Collider other){        
        if (other.gameObject.layer == 9) {
            StartCoroutine(SetAllowDirection(gameObject.name, true));
        }
    }

    IEnumerator SetAllowDirection(string name, bool on) {

        if (!PlayerScript.currentlyRewinding) {
            PlayerScript.restrictAllInput = true;

            yield return new WaitUntil(() => PlayerScript.allowRotate);

            if (on)
                yield return new WaitForSeconds(0.001f);

            PlayerScript.restrictAllInput = false;

            if (name == "Front") {
                PlayerScript.allowForward = on;
            }
            if (name == "Back") {
                PlayerScript.allowBack = on;
            }
            if (name == "Left") {
                PlayerScript.allowLeft = on;
            }
            if (name == "Right") {
                PlayerScript.allowRight = on;
            }
        }


    }
}
