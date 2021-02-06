/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this Code Monkey project
    I hope you find it useful in your own projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class ScreenshotHandler : MonoBehaviour {

    private static ScreenshotHandler instance;

    private Camera myCamera;
    private bool takeScreenshotOnNextFrame;

    private void Awake() {
        instance = this;
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        myCamera = gameObject.GetComponent<Camera>();
    }

    const int SCREENSHOT_DIMENSION = 512;

    GameController game;

    void Start() {
        if(/*game.isDevelopmentMode*/false)
            TakeScreenshot(SCREENSHOT_DIMENSION, SCREENSHOT_DIMENSION);

    }

    private void OnPostRender() {

        if (takeScreenshotOnNextFrame) {
            takeScreenshotOnNextFrame = false;
            RenderTexture renderTexture = myCamera.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);
            
            renderResult.ReadPixels(rect, 0, 0);
            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/Resources/Textures/" + SceneManager.GetActiveScene().buildIndex
                + ".png", byteArray);
            Debug.Log("Saved CameraScreenshot.png");
            

            RenderTexture.ReleaseTemporary(renderTexture);
            myCamera.targetTexture = null;

            myCamera.fieldOfView = game.startZoomDegrees;

        }
    }

    private void TakeScreenshot(int width, int height) {
        myCamera.fieldOfView = game.previewScreenshotZoomInPercentage;
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenshotOnNextFrame = true;
    }

    public static void TakeScreenshot_Static(int width, int height) {
        instance.TakeScreenshot(width, height);
    }
}
