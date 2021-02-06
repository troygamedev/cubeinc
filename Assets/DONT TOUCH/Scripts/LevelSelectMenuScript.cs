using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Written by Ajay Liu, 2019

public class LevelSelectMenuScript : MonoBehaviour
{    
    public GameObject canvas;
    int levelSetIndex = 0;
    public GameObject[] levelSets;

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("Highest Unlocked Scene"))
            PlayerPrefs.SetInt("Highest Unlocked Scene", 2);

        CheckIfButtonsShouldBeActive();

        LoadLevelSet(levelSetIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject currentLevelSet;

    void LoadLevelSet(int setIndex){
        if (currentLevelSet != null)
            Destroy(currentLevelSet);
        currentLevelSet = Instantiate(levelSets[setIndex], canvas.transform);
    }

    public Button previousButton, nextButton;

    public void OnPreviousButtonPress(){
        levelSetIndex--;
        LoadLevelSet(levelSetIndex);
        CheckIfButtonsShouldBeActive();
    }

    public void OnNextButtonPress() {
        levelSetIndex++;
        LoadLevelSet(levelSetIndex);
        CheckIfButtonsShouldBeActive();
    }

    void CheckIfButtonsShouldBeActive(){
        if (levelSetIndex == 0)
            previousButton.gameObject.SetActive(false);
        else
            previousButton.gameObject.SetActive(true);

        if (levelSetIndex == levelSets.Length - 1)
            nextButton.gameObject.SetActive(false);
        else
            nextButton.gameObject.SetActive(true);
    }

    public void HomeButton() {
        SceneManager.LoadScene(0);
    }
}
