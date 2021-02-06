using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class PaintScript : MonoBehaviour {

    ColorController colorCon;

    void Start(){
        colorCon = GetComponent<ColorController>();
    }

    void OnTriggerEnter(Collider other) {

        switch (other.tag) {
            case "Player Collider":
                if (!PlayerScript.isFall) {
                    if (PlayerScript.playerColorCon.thisColorIndex != colorCon.thisColorIndex)
                        PlayerScript.playerColorCon.SetColor(colorCon.thisColorIndex);                    
                }
                break;
            case "Moveable Collider":
                if (!other.transform.parent.GetComponentInChildren<MoveableScript>().isFall)
                    other.transform.parent.GetComponentInChildren<ColorController>().SetColor(colorCon.thisColorIndex);
                break;
        }
        
    }
}
