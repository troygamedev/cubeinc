using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class CameraScript : MonoBehaviour {

    Transform playerTransform;
    public float speedY;
    public float smoothingDuration;

    // Use this for initialization
    void Start () {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0);       
    }

    public float clampPanSpeed;

	// Update is called once per frame
	void Update () {
        if(speedY != 0){
            speedY = Mathf.Clamp(speedY, -clampPanSpeed, clampPanSpeed);
            transform.Rotate(0, speedY * Time.deltaTime, 0);
            
        }
    }

    void LateUpdate(){
        if(transform.position != playerTransform.position)
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, smoothingDuration);  
    }
}
