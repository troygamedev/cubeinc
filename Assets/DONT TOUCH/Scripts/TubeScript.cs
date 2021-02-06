using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Written by Ajay Liu, 2019

public class TubeScript : MonoBehaviour
{
    
    public GameObject launchObject;
    public int maxNumberOfObjectsInGame;

    public Vector3 targetPos;
    public Vector3 launchedObjectRotation;

    public float launchHeight = 25;
    public float gravity = 18f;

    GameController game;

    List<GameObject> spawnedObjects = new List<GameObject>();
    List<int> moveCountWhenDenied = new List<int>(); // Keeps track of when the button press is denied due to too many objects spawned already

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            StartCoroutine(Launch());
        }
    }

    void Awake(){
        Physics.gravity = Vector3.up * gravity;

    }
    void Start(){
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }


    public IEnumerator Launch() {
        //Check how many spawned objects are not disintegrated yet
        int spawnedObjectsStillAlive = 0;
        foreach(GameObject obj in spawnedObjects){
            if (!obj.GetComponentInChildren<MoveableScript>().isFall)
                spawnedObjectsStillAlive++;
        }

        if(spawnedObjectsStillAlive < maxNumberOfObjectsInGame){
            GameObject ins = Instantiate(launchObject, transform.position, Quaternion.identity, game.transform.root);

            PlayerScript.restrictAllInput = true;
            PlayerScript.allowRotate = false;
            game.allowRewind = false;

            MoveableScript moveable = ins.GetComponentInChildren<MoveableScript>();

            moveable.moveCountAtSpawnTime = PlayerScript.moveCount;

            Rigidbody rbody = ins.GetComponent<Rigidbody>();
            rbody.isKinematic = false;
            rbody.useGravity = true;
            CalculationResults calc = CalculateLaunchData(ins);
            rbody.velocity = calc.velocity;


            DisintegrationScript destroy = ins.GetComponentInChildren<DisintegrationScript>();
            destroy.destroyOnObstructedLanding = true;

            spawnedObjects.Add(ins);


            game.moveables.Add(moveable);

            yield return new WaitForSeconds(calc.time);


            if (ins != null) {
                destroy.destroyOnObstructedLanding = false;
                rbody.isKinematic = true;
                rbody.useGravity = false;

                ins.transform.position = targetPos;
                ins.transform.eulerAngles = launchedObjectRotation;
            }

            PlayerScript.restrictAllInput = false;
            PlayerScript.allowRotate = true;
            game.allowRewind = true;

        } else {
            print("TOO MANY OBJECTS IN SCENE - ABOVE SET QUOTA");
            moveCountWhenDenied.Add(PlayerScript.moveCount);
        }         
    }

    /* Velocity Calculations by Sebastian Lague's code  https://github.com/SebLague/Kinematic-Equation-Problems */
    CalculationResults CalculateLaunchData(GameObject obj) {
        float displacementY = targetPos.y - obj.transform.position.y;
        Vector3 displacementXZ = new Vector3(targetPos.x - obj.transform.position.x, 0, targetPos.z - obj.transform.position.z);
        float time = Mathf.Sqrt(-2 * launchHeight / gravity) + Mathf.Sqrt(2 * (displacementY - launchHeight) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * launchHeight);
        Vector3 velocityXZ = displacementXZ / time;

        return new CalculationResults(velocityXZ + velocityY * -Mathf.Sign(gravity), time);
    }

    struct CalculationResults {
        public readonly Vector3 velocity;
        public readonly float time;
        public CalculationResults(Vector3 initialVelocity, float timeToTarget) {
            this.velocity = initialVelocity;
            this.time = timeToTarget;
        }
    };
}
