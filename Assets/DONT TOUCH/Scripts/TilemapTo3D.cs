using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/* Written by Ajay Liu, 2019 */

public class TilemapTo3D : MonoBehaviour {

    public GameObject redTile, blueTile, greenTile, defaultTile, collapseTile;
    public Transform spawningParent;

    Tilemap startTilemap;
    TilemapRenderer rend;
    BoundsInt area;

    GameController game;

    // Use this for initialization
    void Awake () {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();


        //CONVERT TILEMAP TO 3D
        startTilemap = GetComponent<Tilemap>();
        rend = GetComponent<TilemapRenderer>();
        rend.enabled = false;

        MapTilemap(startTilemap, true, spawningParent);   
    }

    public void MapTilemap(Tilemap tilemap, bool isEnabled, Transform parent){
        tilemap.CompressBounds();

        for (int n = tilemap.cellBounds.xMin; n < tilemap.cellBounds.xMax; n++) {
            for (int p = tilemap.cellBounds.yMin; p < tilemap.cellBounds.yMax; p++) {
                int tilemapHeight = Mathf.RoundToInt(tilemap.transform.position.y + 0.5f);
                Vector3Int localPlace = new Vector3Int(n, p, 0);
                Vector3 place = tilemap.CellToWorld(localPlace);
                if (tilemap.HasTile(localPlace)) {
                    Vector3 adjustedPos = GameController.RoundedPos(place + (Vector3.right - Vector3.forward- Vector3.up) * 0.5f);
                    SpawnTile(adjustedPos, tilemap.GetSprite(localPlace), parent, isEnabled);
                }
            }
        }
    }

    public void SpawnTile(Vector3 pos, Sprite s, Transform spawnParent, bool isEnabled){
        Quaternion rot = Quaternion.identity;
        if (!game.allowedLocations.ContainsKey(pos))
            game.allowedLocations.Add(pos, isEnabled);
        switch (s.name){
            case "whiteBox":
                Instantiate(defaultTile, pos + Vector3.up * 0.5f, rot, spawnParent);
                break;
            case "redBox":
                Instantiate(redTile, pos + Vector3.up * 0.5f, rot, spawnParent);
                break;
            case "blueBox":
                Instantiate(blueTile, pos + Vector3.up * 0.5f, rot, spawnParent);
                break;
            case "greenBox":
                Instantiate(greenTile, pos + Vector3.up * 0.5f, rot, spawnParent);
                break;
            case "yellow100":
                Instantiate(collapseTile, pos, rot, spawnParent);
                break;
            default:
                print("ERROR");
                break;
        }                       
    }

    void Update(){
        
    }
}
