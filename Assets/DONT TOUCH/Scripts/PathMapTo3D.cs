using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/* Written by Ajay Liu, 2019 */

public class PathMapTo3D : MonoBehaviour {

    public bool startEnabled = false;
    [HideInInspector] public bool currentlyEnabled = true;

    //public GameObject redTile, blueTile, greenTile, defaultTile;
    public Transform spawnParent;


    TilemapTo3D tile;
    GameController game;
    Tilemap secretTilemap;
    TilemapRenderer rend;
    BoundsInt area;

    List<Transform> childrenTransforms;

    void Awake(){
        childrenTransforms = new List<Transform>();        
    }

    // Use this for initialization
    void Start() {
        //CONVERT TILEMAP TO 3D
        secretTilemap = GetComponent<Tilemap>();
        rend = GetComponent<TilemapRenderer>();
        rend.enabled = false;

        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        tile = GameObject.FindGameObjectWithTag("Level Creator").GetComponent<TilemapTo3D>();

        tile.MapTilemap(secretTilemap, startEnabled, spawnParent);

        
        foreach(Transform t in spawnParent.GetComponentsInChildren<Transform>()) {
            if(t != spawnParent){
                childrenTransforms.Add(t);                
            }            
        }
        

        currentlyEnabled = !startEnabled;
        SetPathActive(startEnabled, false, true);
        currentlyEnabled = startEnabled;
    }

    //Vector2 (moveCountAtOccurance, 0 or 1 (resulted in active))
    public Dictionary<int, bool> activeHistory = new Dictionary<int, bool>();

    public void SetPathActive(bool wantActive, bool isRewind, bool isInit){
        if (wantActive != currentlyEnabled){
            foreach (Transform t in childrenTransforms) {
                Vector3 roundPos = GameController.RoundedPos(t.position - Vector3.up * 0.5f);
                if(game.allowedLocations.ContainsKey(roundPos)){
                    game.allowedLocations.Remove(roundPos);
                    game.allowedLocations.Add(roundPos, wantActive);
                }                                    
            }
            spawnParent.gameObject.SetActive(wantActive);
            currentlyEnabled = wantActive;

            if (!isRewind && !isInit) {
                activeHistory.Add(PlayerScript.moveCount, wantActive);
            }

        }
    }

    void path_OnRewind() {
        if(!PlayerScript.justFell)
            StartCoroutine(OnRewindDelayed());
    }

    IEnumerator OnRewindDelayed(){
        yield return new WaitForSecondsRealtime(0.1f);
        if (activeHistory.ContainsKey(PlayerScript.moveCount - 1)) {
            SetPathActive(!activeHistory[PlayerScript.moveCount - 1], true, false);
            activeHistory.Remove(PlayerScript.moveCount - 1);
        }
    }

    
    
    void Save(){
        SaveScript.infoToSave.pathHistories = "";
        SaveScript.infoToSave.pathHistories += name + ":";
        int i = 0;
        foreach(int keyIndex in activeHistory.Keys){
            SaveScript.infoToSave.pathHistories += keyIndex + "," + (activeHistory[keyIndex] ? "t" : "f") + (i == activeHistory.Keys.Count-1? "" : ";");
            i++;
        }
        SaveScript.infoToSave.pathHistories += "&";
        print(SaveScript.infoToSave.pathHistories);
    }

    void Load(SaveInfo saveInfo) {
        string[] pathsSplit = saveInfo.pathHistories.Split('&');
        foreach(string eachPathInfo in pathsSplit){
            string[] pathNameSplit = eachPathInfo.Split(':');
            if (pathNameSplit[0] == name) {
                activeHistory.Clear();
                string[] historyItems = pathNameSplit[1].Split(';');
                foreach (string item in historyItems) {
                    string[] keyAndVal = item.Split(',');
                    activeHistory.Add(int.Parse(keyAndVal[0]), keyAndVal[1] == "t");
                }
                foreach (int keyIndex in activeHistory.Keys) {
                    print(keyIndex + ": " + activeHistory[keyIndex] + ", ");
                }
                break;
            }
            
        }
        
    }
    
}
