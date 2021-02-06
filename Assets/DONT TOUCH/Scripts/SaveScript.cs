using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SaveInfo {
    public string pathHistories;
    public int[] playerPosition;
    public int playerCurrentColorIndex;
    public bool playerIsFall;
    public int moveCount;
    public int[] playerMoveHistory_Directions;
    public int[] playerMoveHistory_heights;
    public int[] playerMoveHistory_playerColorIndexes;
    public int[] teleportHistoryKeys;
    public int[] teleportHistoryValues;

    //"x_y_z(location), 0 or 1 (isFall), 0 (colorIndex), 0 (yRotation), 0 (moveCountAtSpawnTime) movehistory (repeats until ;) -> 0 (moveCount), 1 (directions), 0 (colorIndex), 0 (yRotation), 0 (height)&
    //Ex: 3-0-2,1,2,90,5,1,3,90,0,0&4,1,2,90,0;
    public string moveablesInfo; 

    public SaveInfo() {
        //default initializations
    }
}

public static class SaveController
{

    public static void SaveGame (int buildIndex, SaveInfo info){
        string path = Application.persistentDataPath + buildIndex + ".lol";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, info);
        stream.Close();
    }

    public static SaveInfo LoadGame(int buildIndex){
        string path = Application.persistentDataPath + buildIndex + ".lol";
        if (File.Exists(path)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SaveInfo outData = formatter.Deserialize(stream) as SaveInfo;
            stream.Close();
            return outData;
        } else {
            Debug.Log("NO SAVE FILE EXISTS");
            return null;
        }
    }

    public static void Delete(int buildIndex) {
        string path = Application.persistentDataPath + buildIndex + ".lol";
        if (File.Exists(path))
            File.Delete(path);
    }
}

public class SaveScript : MonoBehaviour {
    GameController game;

    void Start(){
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    public static SaveInfo infoToSave = new SaveInfo();

    public void OnSave(){
        print("save");
        gameObject.BroadcastMessage("Save");
        SaveController.SaveGame(GameController.currentSceneIndex, infoToSave);
        StartCoroutine(game.SaveAndLoadCooldown(true));
    }

    public void OnLoad() {
        print("load");
        SaveInfo info = SaveController.LoadGame(GameController.currentSceneIndex);
        if (info == null)
            print("NOT FOUND");
        else {
            gameObject.BroadcastMessage("Load", info);
            StartCoroutine(game.SaveAndLoadCooldown(false));
        }
        
    }

    


}
