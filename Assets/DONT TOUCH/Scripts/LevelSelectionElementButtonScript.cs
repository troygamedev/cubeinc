using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class LevelSelectionElementButtonScript : MonoBehaviour
{
    public int sceneIndex;

    RawImage img;
    GameObject padlockObject;

    int starCount;

    void Start() {

        starImgs = transform.GetChild(2).GetComponentsInChildren<Image>();

        padlockObject = transform.GetChild(1).gameObject;
        padlockObject.SetActive(false);

        img = GetComponent<RawImage>();
        Texture2D texture = Resources.Load<Texture2D>("Textures/" + sceneIndex.ToString());
        img.texture = texture;

        if (sceneIndex > PlayerPrefs.GetInt("Highest Unlocked Scene")){
            LockLevel();
        }

        if(!PlayerPrefs.HasKey("Stars" + sceneIndex)) {
            PlayerPrefs.SetInt("Stars" + sceneIndex, 0);
        }

        starCount = PlayerPrefs.GetInt("Stars" + sceneIndex);
        UpdateStarCount(starCount);

    }



    bool isLocked = false;

    public void OnPress(){
        if (!isLocked) {
            PlayerScript.isFall = false;
            PlayerScript.allowRotate = PlayerScript.allowForward = PlayerScript.allowBack = PlayerScript.allowLeft = PlayerScript.allowRight = true;
            PlayerScript.restrictAllInput = false;
            Time.timeScale = 1;
            SceneManager.LoadScene(sceneIndex);
        }
    }

    public Color lockColor;

    void LockLevel(){
        isLocked = true;
        img.color = lockColor;
        padlockObject.SetActive(true);
        foreach(Image im in starImgs) {
            im.transform.gameObject.SetActive(false);
        }
    }

    Image[] starImgs;
    void UpdateStarCount(int n) {
        for (int i = n-1; i >= 0; i--) {
            starImgs[i].color = Color.yellow;
        }
    }
}
