using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class ReflectorScript : MonoBehaviour {

    float rotation;

    public int colorIndexThatDoesNotChangeBulletColor;
    ColorController thisColorCon;

    // Use this for initialization
    void Start () {

        thisColorCon = GetComponent<ColorController>();
    }
	
    const float COLLIDER_SIZE = 0.4f;

    void OnTriggerEnter(Collider other){
        if(other.tag == "Bullet"){
            Vector3 directionToGo = Vector3.Reflect(other.transform.forward, transform.up + transform.forward);
            other.transform.forward = new Vector3(directionToGo.x, 0, directionToGo.z);
            other.transform.eulerAngles = new Vector3(other.transform.eulerAngles.x, Mathf.Round(other.transform.eulerAngles.y / 90) * 90, other.transform.eulerAngles.z);
            other.transform.position = new Vector3(Mathf.Round(transform.parent.position.x), other.transform.position.y, Mathf.Round(transform.parent.position.z));

            //dont change bullet's color if the reflector is default color
            ColorController otherColorCon = other.GetComponent<ColorController>();
            if (colorIndexThatDoesNotChangeBulletColor != thisColorCon.thisColorIndex)
                 otherColorCon.SetColor(thisColorCon.thisColorIndex);
            
        }        
    }    
}
