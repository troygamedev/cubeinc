using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class SpinnerScript : MonoBehaviour {

    public bool isClockwise;

	// Use this for initialization
	void Start () {
		
	}
	

	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other){
        if(other.CompareTag("Prism Center")){
            other.transform.eulerAngles = new Vector3(0, isClockwise ? other.transform.eulerAngles.y + 90 : other.transform.eulerAngles.y - 90, 90);
        }
    }
}
