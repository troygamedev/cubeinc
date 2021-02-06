using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* Written by Ajay Liu, 2019 */

public class GameController : MonoBehaviour {

    [HideInInspector] public Dictionary<Vector3, bool> allowedLocations = new Dictionary<Vector3, bool>();

    [HideInInspector]public bool isDevelopmentMode = false;

    public GameObject player;
    [HideInInspector]public PlayerScript play;
    public Vector3 spawnPos;

    public float startZoomDegrees = 90;

    public Text moveCountText;

    public List<MoveableScript> moveables = new List<MoveableScript>();

    public int previewScreenshotZoomInPercentage;

    public GameObject nextLevelButton;

    SaveScript save;

    public int playerSpawnColorIndex = 0;

    [HideInInspector] public bool isWin = false;

	// Use this for initialization
	void Awake () {
        SpawnPlayer();
    }

    public static int currentSceneIndex;

    public Text levelNameText;

    void Start(){

        Time.timeScale = 1;

        save = transform.root.GetComponent<SaveScript>();


        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        levelNameText.text = "Level " + (currentSceneIndex-1);

        pausePanel.SetActive(false);
        warningPanel.SetActive(false);
        nextLevelButton.SetActive(false);

        PlayerScript.allowRotate = true;


        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality Level"));

        foreach (MoveableScript move in FindObjectsOfType<MoveableScript>()) {
            moveables.Add(move);
        }

        Camera.main.fieldOfView = startZoomDegrees;

        StartCoroutine(frameAfterStart());
    }

    IEnumerator frameAfterStart(){
        yield return new WaitForEndOfFrame();
        PlayerScript.playerColorCon.SetColor(playerSpawnColorIndex);

    }


    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Restart();
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            LoadNextLevel();
        }
    }

    void SpawnPlayer(){
        play = Instantiate(player, spawnPos, Quaternion.identity, transform.root).GetComponentInChildren<PlayerScript>();

    }

    bool allowPause = true;

    public void Win(){
        isWin = true;
        PlayerScript.restrictAllInput = true;
        PlayerScript.moveQueue.Clear();
        allowRewind = false;
        allowPause = false;

        if(!PlayerPrefs.HasKey("Highest Unlocked Scene"))
            PlayerPrefs.SetInt("Highest Unlocked Scene", 0);

        PlayerPrefs.SetInt("Highest Unlocked Scene", Mathf.Max(PlayerPrefs.GetInt("Highest Unlocked Scene"), currentSceneIndex + 1));

        Text[] starTexts = starParent.GetComponentsInChildren<Text>();
        starTexts[0].text = "> " + bestNumMoves[currentSceneIndex - 2].y.ToString();
        starTexts[1].text = "≤ " + bestNumMoves[currentSceneIndex - 2].y.ToString();
        starTexts[2].text = "≤ " + bestNumMoves[currentSceneIndex - 2].x.ToString();

        completePanel.SetActive(true);
        numMovesUsedCompleted.text = "Moves Used: " + (PlayerScript.moveCount+1).ToString();

        int stars = 3;

        if(PlayerScript.moveCount + 1 > bestNumMoves[currentSceneIndex - 2].x) {
            starParent.transform.GetChild(2).GetComponent<Image>().color = Color.white;
            stars = 2;
        }
        if (PlayerScript.moveCount+1 > bestNumMoves[currentSceneIndex - 2].y) {
            starParent.transform.GetChild(1).GetComponent<Image>().color = Color.white;
            stars = 1;
        }

        PlayerPrefs.SetInt("Stars" + currentSceneIndex, Mathf.Max(stars, PlayerPrefs.GetInt("Stars" + currentSceneIndex)));

        if(currentSceneIndex != SceneManager.sceneCountInBuildSettings-1)
            nextLevelButton.SetActive(true);
    }

    public GameObject pausePanel, warningPanel, saveLoadCooldownPanel;

    public void PauseButton(){
        if (allowPause) {
            pausePanel.SetActive(true);
            PlayerScript.posAtPause = RoundedVectorInt(play.transform.position);
            PlayerScript.restrictAllInput = true;
            Time.timeScale = 0;
        }        
    }

    public void ResumeButton(){
        pausePanel.SetActive(false);
        PlayerScript.restrictAllInput = false;
        Time.timeScale = 1;
    }

    /*
    void OnApplicationQuit(){
        save.OnSave();        
    }
    */

    public void SaveButton(){
        save.OnSave();
    }

    public void LoadButton() {
        save.OnLoad();
    }

    public void OnYesButton() {
        Restart();
    }

    public void OnNoButton() {
        warningPanel.SetActive(false);
    }

    public void RestartButtonPress(){
        warningPanel.SetActive(true);
    }

    public void Restart() {
        PlayerScript.isFall = false;
        PlayerScript.allowRotate = PlayerScript.allowForward = PlayerScript.allowBack = PlayerScript.allowLeft = PlayerScript.allowRight = true;
        PlayerScript.restrictAllInput = false;
        ResumeButton();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);    
    }

    public void LoadNextLevel(){
        PlayerScript.isFall = false;
        PlayerScript.allowRotate = PlayerScript.allowForward = PlayerScript.allowBack = PlayerScript.allowLeft = PlayerScript.allowRight = true;
        PlayerScript.restrictAllInput = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void HomeButton() {
        //SaveButton();
        SceneManager.LoadScene(0);
    }

    public bool allowRewind = true;

    public void RewindButton(){
        if (allowRewind)
            StartCoroutine(Rewind());        
    }

    public void SkipButton(){
        if(SceneManager.GetActiveScene().buildIndex + 1 <= SceneManager.sceneCountInBuildSettings){
            PlayerPrefs.SetInt("Highest Unlocked Scene", SceneManager.GetActiveScene().buildIndex + 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        
    }

    
    IEnumerator Rewind() {


        allowRewind = false;
        PlayerScript.restrictAllInput = true;

        bool playerFell = false;

        transform.root.BroadcastMessage("path_OnRewind");

        yield return new WaitUntil(() => PlayerScript.allowRotate);

        if (PlayerScript.isFall) {
            playerFell = true;
            play.RevertFallForRewind();
        } else {
            play.Rewind();
        }

        for (int i = 0; i < moveables.Count; i++) {
            yield return new WaitUntil(() => !moveables[i].stillMoving);
            moveables[i].Rewind(playerFell);
        }

        yield return new WaitForSeconds(0.2f);

        PlayerScript.restrictAllInput = false;
        allowRewind = true;
    }

    public int maxMoveCount;

    public void UpdateMoveCountText(){
        moveCountText.text = PlayerScript.moveCount.ToString();
        maxMoveCount = Mathf.Max(maxMoveCount, PlayerScript.moveCount);
    }

    public static Vector3 RoundedPos(Vector3 v){
        return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }
    public static Vector3Int RoundedVectorInt(Vector3 v) {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    [HideInInspector]public bool touchIsInRewindZone = false;

    public void TouchEnterRewindButton() {
        touchIsInRewindZone = true;
    }

    public void TouchExitRewindButton(){
        touchIsInRewindZone = false;
    }

    void Save(){

        /* SAVE MOVEABLES */
        //"0 (Moveable Prefab [0 = Moveable Box, 1 = Reflector] ),x_y_z(location), 0 (yRotation), 0 or 1 (isFall), 0 (colorIndex), 0 (moveCountAtSpawnTime) movehistory (repeats until ;): 0 (moveCount), 1 (directions), 0 (colorIndex), 0 (yRotation), 0 (height)&
        SaveScript.infoToSave.moveablesInfo = "";
        for (int i = 0; i < moveables.Count; i++) {

            switch (moveables[i].tag) {
                case "Moveable Box":
                    SaveScript.infoToSave.moveablesInfo += "0,";
                    break;
                case "Prism Center":
                    SaveScript.infoToSave.moveablesInfo += "1,";
                    break;
            }

            SaveScript.infoToSave.moveablesInfo += 
                moveables[i].transform.position.x.ToString() + "_" + moveables[i].transform.position.y.ToString() + "_" + moveables[i].transform.position.z.ToString() + ",";
            SaveScript.infoToSave.moveablesInfo += ((int)moveables[i].transform.eulerAngles.y).ToString() + ",";
            SaveScript.infoToSave.moveablesInfo += moveables[i].isFall ? "1," : "0,";
            SaveScript.infoToSave.moveablesInfo += moveables[i].colorCon.thisColorIndex.ToString() + ",";
            SaveScript.infoToSave.moveablesInfo += moveables[i].moveCountAtSpawnTime;
            if (moveables[i].moveHistory.Count != 0)
                SaveScript.infoToSave.moveablesInfo += ":";

            //MoveHistory
            int counter = 0;
            foreach (int j in moveables[i].moveHistory.Keys) {
                counter++;
                SaveScript.infoToSave.moveablesInfo += j.ToString() + ",";
                SaveScript.infoToSave.moveablesInfo += ((int)moveables[i].moveHistory[j].direction).ToString() + ",";
                SaveScript.infoToSave.moveablesInfo += moveables[i].moveHistory[j].colorIndex + ",";
                SaveScript.infoToSave.moveablesInfo += moveables[i].moveHistory[j].yRotation + ",";
                SaveScript.infoToSave.moveablesInfo += moveables[i].moveHistory[j].height;
                if(counter != moveables[i].moveHistory.Keys.Count)
                    SaveScript.infoToSave.moveablesInfo += "&";
            }

            if(i != moveables.Count - 1)
                SaveScript.infoToSave.moveablesInfo += ";";
        }
        SaveScript.infoToSave.moveablesInfo = SaveScript.infoToSave.moveablesInfo.Trim();
    }

    [SerializeField] GameObject moveableBoxPrefab, reflectorPrefab;


    void Load(SaveInfo saveInfo) {

        //print(saveInfo.moveablesInfo);

        //DESTROY ALL OLD MOVEABLES AND SPAWN LOADED ONES
        foreach (MoveableScript move in moveables) {
            Destroy(move.transform.parent.gameObject);
        }

        moveables.Clear();

        /* SAVE MOVEABLES */
        //"0 (Moveable Prefab [0 = Moveable Box, 1 = Reflector] ),x_y_z(location), 0 (yRotation), 0 or 1 (isFall), 0 (colorIndex), 0 (moveCountAtSpawnTime) movehistory (repeats until ;) : 0 (moveCount), 1 (directions), 0 (colorIndex), 0 (yRotation), 0 (height)&
        if (saveInfo.moveablesInfo != "") {


            string[] moveablesArr = saveInfo.moveablesInfo.Split(';');

            foreach (string s in moveablesArr) {
                string[] infoAndHistory = s.Split(':'); // splits s into info at index 0 and history at index 1 
                string[] csv = infoAndHistory[0].Split(',');

                /* DECODING THE COMMA SEPARATED VALUES */
                GameObject prefabToSpawn = moveableBoxPrefab;

                switch(csv[0]){
                    case "0":
                        prefabToSpawn = moveableBoxPrefab;
                        break;
                    case "1":
                        prefabToSpawn = reflectorPrefab;
                        break;
                }

                Vector3 locationToSpawn;
                string[] location = csv[1].Split('_');
                locationToSpawn = new Vector3(int.Parse(location[0]), int.Parse(location[1]), int.Parse(location[2]));

                Vector3 rotationToSpawn = new Vector3(0, int.Parse(csv[2]), prefabToSpawn == reflectorPrefab? 90:0);

                bool isFallAtSpawn = csv[3] == "0";

                int colorIndexAtSpawn = int.Parse(csv[4]);

                int moveCountAtSpawn = int.Parse(csv[5]);

                GameObject ins = Instantiate(prefabToSpawn, locationToSpawn, Quaternion.identity, transform.root);
                ins.transform.GetChild(0).eulerAngles = rotationToSpawn;

                MoveableScript insMoveable = ins.GetComponentInChildren<MoveableScript>();

                if (infoAndHistory.Length > 1){
                    string[] histories = infoAndHistory[1].Split('&');
                    for (int i  = 0; i < histories.Length; i++){
                        string[] historyCsv = histories[i].Split(',');
                        //0(moveCount), 1(directions), 0(colorIndex), 0(yRotation), 0(height)
                        insMoveable.moveHistory.Add(int.Parse(historyCsv[0]), new MoveableScript.MoveHistoryItem((Direction)int.Parse(historyCsv[1]), int.Parse(historyCsv[2]), int.Parse(historyCsv[3]), int.Parse(historyCsv[4])));
                    }
                }

                moveables.Add(insMoveable);
            }           
        }
    }

    public float saveWaitDuration = 1f;
    public Text saveLoadText;

    public IEnumerator SaveAndLoadCooldown(bool isSave) {
        Time.timeScale = 1;
        PlayerScript.moveQueue.Clear();
        pausePanel.SetActive(false);
        saveLoadCooldownPanel.SetActive(true);
        saveLoadText.text = isSave ? "Saving..." : "Loading...";
        yield return new WaitForSecondsRealtime(saveWaitDuration);
        saveLoadCooldownPanel.SetActive(false);
        PlayerScript.restrictAllInput = false;
    }


    public GameObject completePanel;
    public Text numMovesUsedCompleted;
    public GameObject starParent;

    //x is 3 stars, y is 2 stars
    public List<Vector2> bestNumMoves = new List<Vector2>();

}
