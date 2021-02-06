using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class GoalBlockScript : MonoBehaviour {

    [HideInInspector]public Color thisColor;

    ColorController colorCon;
    GameController game;

	// Use this for initialization
	void Start () {
        colorCon = transform.parent.GetComponent<ColorController>();
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other){
        if (other.CompareTag("Center Collider")){            
            if (colorCon.thisColorIndex == PlayerScript.playerColorCon.thisColorIndex){
                game.Win();
            }
        }
    }
}
