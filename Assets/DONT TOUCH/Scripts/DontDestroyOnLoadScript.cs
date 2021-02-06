using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        if (GameObject.FindGameObjectsWithTag("Music").Length > 1) {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
