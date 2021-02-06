using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class SpikeScript : MonoBehaviour {

    PlayerScript play;
    ColorController colorCon;

	// Use this for initialization
	void Start () {
        play = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerScript>();
        colorCon = GetComponent<ColorController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player Collider")){
           if(PlayerScript.playerColorCon.thisColorIndex == colorCon.thisColorIndex){
                play.OutOfBounds();
           }
        }
    }
}
