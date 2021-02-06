using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public enum Direction{ Back = -2, Left = -1, Right = 1, Forward = 2 }

public class DetermineDirectionScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Vector2 RotateVector(Vector2 v, float angle) {
        float radian = angle * Mathf.Deg2Rad;
        float _x = v.x * Mathf.Cos(radian) - v.y * Mathf.Sin(radian);
        float _y = v.x * Mathf.Sin(radian) + v.y * Mathf.Cos(radian);
        return new Vector2(_x, _y);
    }

    public Direction GetDirection(float horizontal, float vertical){
        Vector2 point = new Vector2(horizontal, vertical).normalized;

        float angle;    
        angle = 360f - transform.eulerAngles.y;
       
        point = RotateVector(point, angle);

        if (Mathf.Abs(point.x) > Mathf.Abs(point.y)) {
            if (point.x > 0) {
                return Direction.Right;
            } else {
                return Direction.Left;
            }
        } else {
            if (point.y > 0) {
                return Direction.Forward;
            } else {
                return Direction.Back;
            }
        }

        
    }
}
