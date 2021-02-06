using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class ColorController : MonoBehaviour {
    Renderer rend;

    void Start(){
        rend = GetComponent<Renderer>();
        SetColor(thisColorIndex);
    }

    public static Color[] colors = {Color.black, Color.red, Color.green, Color.cyan, Color.magenta};
    public int thisColorIndex;

    public void SetColor(int newIndex){
        if(tag == "Bullet") {
            rend = GetComponent<Renderer>();
        }
        rend.material.color = colors[newIndex];
        thisColorIndex = newIndex;
    }
}
