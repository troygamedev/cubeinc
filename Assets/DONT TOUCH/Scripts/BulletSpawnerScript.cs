using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class BulletSpawnerScript : MonoBehaviour {

    public GameObject bulletPrefab;
    public float bulletsPerSecond;

    public int colorToFireIndex;

	// Use this for initialization
	void Start () {
        
        InvokeRepeating("FireBullet", 0, bulletsPerSecond / 60f);
    }

    // Update is called once per frame
    void Update () {

    }

    void FireBullet(){
        GameObject ins = Instantiate(bulletPrefab, transform.position + transform.forward * 0.6f, transform.rotation);
        ins.GetComponent<ColorController>().SetColor(colorToFireIndex);
    }
}
