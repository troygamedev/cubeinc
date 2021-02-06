using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisintegrationScript : MonoBehaviour
{

    public bool destroyOnObstructedLanding = false;
    public float disintegrationTime;
    public bool doneDisintegrating = true;

    Renderer[] renderers;
    Collider[] colliders;

    // Start is called before the first frame update
    void Start()
    {
        renderers = transform.parent.GetComponentsInChildren<Renderer>();
        colliders = transform.parent.GetComponentsInChildren<Collider>();
    }

    void OnTriggerEnter(Collider other){
        if (destroyOnObstructedLanding && other.name == "Bottom"){
            StartCoroutine(Disintegrate());
            transform.parent.GetComponentInChildren<MoveableScript>().isFall = true;            
        }
    }

    void SetCollidersEnabled(bool on) {
        foreach (Collider c in colliders) {
            c.enabled = on;
        }
    }

    public IEnumerator Disintegrate(){
        doneDisintegrating = false;
        float step = 64f;

        Transform moveBody = transform.parent.GetChild(0);
        moveBody.eulerAngles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        SetCollidersEnabled(false);

        for (int i = 0; i < step; i++) {
            bool isTransparent = false;

            foreach (Renderer r in renderers){
                if (r.material.color.a > disintegrationTime / step)
                    r.material.color -= new Color(0f, 0f, 0f, disintegrationTime / step);
                else{
                    r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, 0f);
                    isTransparent = true;
                }
            }
            transform.parent.position += Vector3.up * (disintegrationTime / step);

            if (isTransparent) {                
                break;
            }

            yield return new WaitForSeconds(disintegrationTime / step);
        }

        doneDisintegrating = true;

        //transform.root.gameObject.SetActive(false);
    }

    public void Recover(Vector3 rotation, int colorIndex) {
        doneDisintegrating = true;
        StopCoroutine("Disintegrate");

        transform.parent.gameObject.GetComponentInChildren<ColorController>().SetColor(colorIndex);

        foreach (Renderer rend in transform.parent.gameObject.GetComponentsInChildren<Renderer>()) {
            rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1f);
        }

        StopCoroutine("Disintegrate");

        Transform moveBody = transform.parent.GetChild(0);
        moveBody.eulerAngles = rotation;
        SetCollidersEnabled(true);
    }
}
