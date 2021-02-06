using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class BulletScript : MonoBehaviour {

    public float speed;
    const float TIME_UNTIL_DESTROY = 3f;

    ColorController colorCon;

    void Start(){
        colorCon = GetComponent<ColorController>();

        InvokeRepeating("CheckIfOutOfBounds", 0f, 10f);
    }


    void Update () {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

    void OnTriggerEnter(Collider other){
        switch (other.tag){
            case "Goal Block":
                other.transform.parent.GetComponent<ColorController>().SetColor(colorCon.thisColorIndex);
                Destroy(this.gameObject);
                break;
            case "Player Collider":
                PlayerScript.playerColorCon.SetColor(colorCon.thisColorIndex);
                Destroy(this.gameObject);
                break;            
            case "Moveable Collider":
                if(other.transform.parent.GetChild(0).tag == "Moveable Box"){
                    other.transform.parent.GetComponentInChildren<ColorController>().SetColor(colorCon.thisColorIndex);
                    Destroy(this.gameObject);
                }
                break;
            case "Spike":
                other.GetComponent<ColorController>().SetColor(colorCon.thisColorIndex);
                Destroy(this.gameObject);
                break;
        }

        if(other.gameObject.layer == 9) {
            Destroy(this.gameObject);   
        }
    }

    float OUTOFBOUNDSDISTANCE = 30f;

    void CheckIfOutOfBounds(){
        if(Vector3.Distance(transform.position, Vector3.zero) > OUTOFBOUNDSDISTANCE) {
            Destroy(this.gameObject);
        }
    }
}
