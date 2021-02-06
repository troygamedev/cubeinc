using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class Rotate : MonoBehaviour {

    public float rotateSpeed;

    void Update () {
        transform.Rotate(transform.up, rotateSpeed * Time.deltaTime);      
    }
}
