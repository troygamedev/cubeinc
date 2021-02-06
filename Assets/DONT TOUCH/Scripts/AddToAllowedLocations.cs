using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class AddToAllowedLocations : MonoBehaviour {

    public float yOffset;

    GameController game;

    public bool walkable;

	// Use this for initialization
	void Start () {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        Vector3 pos = GameController.RoundedPos(transform.position + Vector3.up * yOffset);
        if(!game.allowedLocations.ContainsKey(pos)){
            game.allowedLocations.Add(pos, walkable);
        }
    }
}
