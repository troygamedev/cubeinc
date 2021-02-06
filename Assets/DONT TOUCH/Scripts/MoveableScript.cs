using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Written by Ajay Liu, 2019 */

public class MoveableScript : MonoBehaviour {

    Rigidbody rbody;
    [HideInInspector]public DisintegrationScript disintegration;

    [HideInInspector]public bool isFall = false;

    public ColorController colorCon;
    GameController game;

    PlayerScript play;

    Collider[] colliders;

    // Use this for initialization
    void Start() {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        
        play = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponent<PlayerScript>();
        rbody = transform.parent.GetComponentInChildren<Rigidbody>();
        disintegration = transform.parent.GetComponentInChildren<DisintegrationScript>();
        colorCon = GetComponent<ColorController>();
        colliders = GetComponentsInChildren<Collider>();
        if (PlayerScript.moveCount == 0)
            moveCountAtSpawnTime = -2;
    }

    // Update is called once per frame
    void Update () {
        Move_Update();
        if (isBlockBelow)
            CheckBlockFall_Update();

    }

    [HideInInspector]public bool stillMoving = false;


    Vector3 positionAtFall;
    Vector3 eulersAtFall;
    int colorIndexAtFall;

    bool isGonnaFall = false;

    void Pre_OutOfBounds(){
        isGonnaFall = true;
        eulersAtFall = transform.eulerAngles;
        colorIndexAtFall = colorCon.thisColorIndex;
        positionAtFall = GameController.RoundedPos(transform.parent.position);
    }

    void OutOfBounds() {

        isFall = true;
        rbody.isKinematic = false;
        rbody.useGravity = true;

        disintegration.StartCoroutine("Disintegrate");
    }

    public struct MoveHistoryItem {
        public Direction direction;
        public int colorIndex;
        public int yRotation;
        public int height;
        public MoveHistoryItem(Direction direction, int colorIndex, int yRotation, int height) {
            this.direction = direction;
            this.colorIndex = colorIndex;
            this.yRotation = yRotation;
            this.height = height;
        }
    }


    public Dictionary<int, MoveHistoryItem> moveHistory = new Dictionary<int, MoveHistoryItem>();

    [HideInInspector] public int moveCountAtSpawnTime = -2;

    public void Rewind(bool playerFell) {
        if(moveCountAtSpawnTime == PlayerScript.moveCount) {
            isFall = true;
            rbody.isKinematic = false;            
            disintegration.StartCoroutine("Disintegrate");
        } else {
            if (moveHistory.Count > 0) {
                if (playerFell) {
                    if (moveHistory.ContainsKey(PlayerScript.moveCount)) {
                        MoveHistoryItem move;
                        moveHistory.TryGetValue(PlayerScript.moveCount, out move);

                        moveHistory.Remove(PlayerScript.moveCount);
                        int inversedDirectionValue = (int)move.direction * -1;


                        if (isFall) {
                            //transform.root.gameObject.SetActive(true);
                            StartCoroutine(RecoverFromFall());
                        } else {
                            transform.eulerAngles = new Vector3(0, move.yRotation, 90);
                            StartCoroutine(MoveToDirection((Direction)inversedDirectionValue, move.height, true));
                            colorCon.SetColor(move.colorIndex);
                        }

                    }
                } else {
                    if (moveHistory.ContainsKey(PlayerScript.moveCount - 1)) {

                        MoveHistoryItem move;
                        moveHistory.TryGetValue(PlayerScript.moveCount - 1, out move);

                        moveHistory.Remove(PlayerScript.moveCount - 1);
                        int inversedDirectionValue = (int)move.direction * -1;


                        if (isFall) {
                            //transform.root.gameObject.SetActive(true);
                            StartCoroutine(RecoverFromFall());
                        } else {
                            transform.eulerAngles = new Vector3(0, move.yRotation, 90);
                            StartCoroutine(MoveToDirection((Direction)inversedDirectionValue, move.height, true));
                            colorCon.SetColor(move.colorIndex);
                        }

                    }
                }
            }
        }        
    }

    IEnumerator RecoverFromFall(){
        stillMoving = true;

        rbody.isKinematic = true;
        rbody.useGravity = false;
        transform.parent.position = positionAtFall;

        yield return new WaitUntil(() => PlayerScript.allowRotate);        

        disintegration.Recover(eulersAtFall, colorIndexAtFall);

        transform.parent.position = positionAtFall;


        //colorCon.SetColor(colorIndexAtFall);
        //transform.eulerAngles = eulersAtFall;

        stillMoving = false;
        isFall = false;
    }

    public static float MOVETIME = 0.1f;

    Vector3 targetPos;
    bool moveToTargetPosition = false;

    public float speed;
    
    void Move_Update(){
        if(moveToTargetPosition){
            // Move position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.parent.position = Vector3.MoveTowards(transform.parent.position, targetPos, step);

            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.parent.position, targetPos) < 0.001f) {
                moveToTargetPosition = false;
            }
        }
    }

    int lowest = -40;

    IEnumerator MoveToDirection(Direction dir, int height, bool isRewind) {

        if (isRewind)
            yield return new WaitUntil(() => PlayerScript.allowRotate); 

        stillMoving = true;
        Vector3 roundedPos = GameController.RoundedPos(transform.parent.position);

        switch (dir) {
            case Direction.Forward:
                targetPos = roundedPos + Vector3.forward;
                break;
            case Direction.Back:
                targetPos = roundedPos - Vector3.forward;
                break;
            case Direction.Left:
                targetPos = roundedPos - Vector3.right;
                break;
            case Direction.Right:
                targetPos = roundedPos + Vector3.right;
                break;
        }
        targetPos = new Vector3(targetPos.x, height, targetPos.z);

        transform.parent.position = new Vector3(transform.parent.position.x, height, transform.parent.position.z);

        Vector3 roundedTargetPos = GameController.RoundedPos(targetPos);

        moveToTargetPosition = true;

        bool floorIsEnabled;

        MoveHistoryItem move = new MoveHistoryItem(dir, colorCon.thisColorIndex, (int)transform.eulerAngles.y, (int)roundedTargetPos.y);

        if (!game.allowedLocations.TryGetValue(roundedTargetPos - Vector3.up, out floorIsEnabled) || !floorIsEnabled) {
            Pre_OutOfBounds();
        }

        if (!isRewind) {
            if (!moveHistory.ContainsKey(PlayerScript.moveCount))
                moveHistory.Add(PlayerScript.moveCount, move);
            else {
                print("BIG BUG");
                moveHistory.Remove(PlayerScript.moveCount);
                moveHistory.Add(PlayerScript.moveCount, new MoveHistoryItem(dir, colorCon.thisColorIndex, (int)transform.eulerAngles.y, (int)roundedTargetPos.y));            
            }
        }

        yield return new WaitUntil(() => !moveToTargetPosition);
        yield return new WaitUntil(()=> PlayerScript.allowRotate);

        if (isGonnaFall) {
            isBlockBelow = false;
            for (int i = (int)targetPos.y - 1; i >= lowest; i--) {
                Vector3 locationBelow = new Vector3(targetPos.x, i, targetPos.z);
                if (game.allowedLocations.TryGetValue(locationBelow, out floorIsEnabled) && floorIsEnabled) {
                    isBlockBelow = true;
                    FallTo(i + 1);
                    moveHistory.Remove(PlayerScript.moveCount-1);
                    moveHistory.Add(PlayerScript.moveCount-1, new MoveHistoryItem(dir, colorCon.thisColorIndex, (int)transform.eulerAngles.y, (int)positionAtFall.y));
                    //yield return new WaitWhile(()=> stillMoving);
                    break;
                }
            }

            if(!isBlockBelow)
                OutOfBounds();
        }


        transform.parent.position = targetPos;

        //yield return new WaitUntil(() => disintegration.doneDisintegrating);
        if (!disintegration.doneDisintegrating) {
            disintegration.StartCoroutine("Disintegrate");
        }

        isGonnaFall = false;
        stillMoving = false;

    }

    int heightToFallTo;
    bool isBlockBelow;

    void CheckBlockFall_Update() {
        if (Mathf.Abs(transform.parent.position.y - heightToFallTo) <= 0.1f) {
            isBlockBelow = false;

            rbody.isKinematic = true;
            rbody.useGravity = false;
            stillMoving = false;            

            transform.parent.position = new Vector3(transform.parent.position.x, heightToFallTo, transform.parent.position.z);
            //GetComponentInChildren<BoxCollider>().enabled = true;
            SetCollidersEnabled(true);

            StartCoroutine(play.resetMoveablesDestroyOnObstructedLandingDelayed());
            
            
        }
    }

    

    void FallTo(int heightToFall) {
        PlayerScript.moveQueue.Clear();
        PlayerScript.allowRotate = false;
        PlayerScript.restrictAllInput = true;

        heightToFallTo = heightToFall;
        rbody.useGravity = true;
        rbody.isKinematic = false;
        stillMoving = true;
        //GetComponentInChildren<BoxCollider>().enabled = false;
        SetCollidersEnabled(false);
        play.setMoveablesDestroyOnObstructedLanding();
    }

    void SetCollidersEnabled(bool on) {
        foreach(Collider c in colliders) {
            c.enabled = on;
        }
    }

    void OnTriggerEnter(Collider other){
        if(!stillMoving){
            if ((other.tag == "Player Collider" || other.tag == "Moveable Collider")) {

                switch (other.name) {
                    case "Front":
                        StartCoroutine(MoveToDirection(Direction.Forward, (int)transform.parent.position.y, false));
                        break;
                    case "Back":
                        StartCoroutine(MoveToDirection(Direction.Back, (int)transform.parent.position.y, false));
                        break;
                    case "Left":
                        StartCoroutine(MoveToDirection(Direction.Left, (int)transform.parent.position.y, false));
                        break;
                    case "Right":
                        StartCoroutine(MoveToDirection(Direction.Right, (int)transform.parent.position.y, false));
                        break;
                    default:
                        break;
                }
            }
        }

       
    }
}
