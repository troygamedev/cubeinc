using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* Written by Ajay Liu, 2019 */
public class ApplyColorToChildren : MonoBehaviour {



    public Color color;

	// Use this for initialization
	void Awake () {
        SetColor(color);        
	}

    public void SetColor(Color color){
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer m in meshes) {
            if (m.tag != "Star")
                m.material.SetColor("_Color", color);
        }
    }
}
