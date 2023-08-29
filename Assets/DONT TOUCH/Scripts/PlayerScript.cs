using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/* Written by Ajay Liu, 2019 */

public class PlayerScript : MonoBehaviour {

    public static int moveCount = 0;
    

    public bool isKeyboard;


    public static ColorController playerColorCon;

    [HideInInspector] public Color defaultColor;

    [HideInInspector]public static MeshRenderer playerRenderer;
    Transform playerBody;
    Rigidbody rbody;
    GameObject cam;
    Transform mainCameraTransform;
    [HideInInspector]public Camera mainCamera;
    CameraScript cameraScript;
    DetermineDirectionScript directionScript;
    GameController game;

    public static bool restrictAllInput = false;

	// Use this for initialization
	void Start () {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        playerColliders = GetComponentsInChildren<BoxCollider>();

        isFall = false;
        allowForward = allowBack = allowLeft = allowRight = true;
        restrictAllInput = false;


        moveCount = 0;
        game.UpdateMoveCountText();

        playerBody = transform.GetChild(0);
        rbody = GetComponent<Rigidbody>();

        cam = transform.parent.GetChild(1).gameObject;
        cameraScript = cam.gameObject.GetComponent<CameraScript>();
        mainCameraTransform = cam.transform.GetChild(0);
        mainCamera = mainCameraTransform.GetComponent<Camera>();

        directionScript = cam.GetComponent<DetermineDirectionScript>();
        playerRenderer = GetComponentInChildren<MeshRenderer>();
        playerColorCon = GetComponentInChildren<ColorController>();

        //isKeyboard = PlayerPrefs.GetInt("isKeyboard") == 1;
        isKeyboard = false;
    }

    Vector2 fingerDown;
    Vector2 fingerUp;

    bool isMultiFinger = false;
    bool oneFingerReleased = false;

    public float SWIPE_THRESHOLD;

    // Update is called once per frame
    void Update() {
        if(!restrictAllInput && !game.isWin){
            KeyboardControl();

            if (!isKeyboard) {
                UpdateFingers();
            }
        }

        UpdateMoveQueue();
        Move_Update();
        Rotate_Update();
        if(isBlockBelow)
            CheckBlockFall_Update();
    }


    #region input    


    void KeyboardControl(){

        //KEYBOARD
        if (Input.GetKeyDown(KeyCode.W)) {
            moveQueue.Add(new MoveQueueItem(Direction.Forward, Mathf.RoundToInt(playerBody.transform.position.y), false));
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            moveQueue.Add(new MoveQueueItem(Direction.Left, Mathf.RoundToInt(playerBody.transform.position.y), false));
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            moveQueue.Add(new MoveQueueItem(Direction.Back, Mathf.RoundToInt(playerBody.transform.position.y), false));
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            moveQueue.Add(new MoveQueueItem(Direction.Right, Mathf.RoundToInt(playerBody.transform.position.y), false));
        }
        
        

        if(Input.GetKey(KeyCode.LeftArrow)){
            SetCamPanSpeed(300f);
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            SetCamPanSpeed(-300f);
        } else {
            SetCamPanSpeed(0);
        }
    }

    Vector2 touch1Prev, touch2Prev, posChange1, posChange2;
    float prevDistance;
    public float zoomSpeed;

    public float panThreshold;

    

    void MultiFinger(){

        /////////////// PANNING //////////////////////
        
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);
        
        if (touch1.phase == TouchPhase.Began)
            touch1Prev = touch1.position;
        if (touch2.phase == TouchPhase.Began)
            touch2Prev = touch2.position;

        if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved){
            posChange1 = touch1.position - touch1Prev;
            touch1Prev = touch1.position;
            posChange2 = touch2.position - touch2Prev;
            touch2Prev = touch2.position;
            SetCamPanSpeed((posChange1.x + posChange2.x) / 2 * PlayerPrefs.GetFloat("Camera Pan Speed"));            
        } else {
            SetCamPanSpeed(0f);
        }



        ///////////////////// ZOOMING ///////////////////////

        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        float touchDeltaMag = (touch1.position - touch2.position).magnitude;

        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;


        mainCamera.fieldOfView += deltaMagnitudeDiff * zoomSpeed;

        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, 20f, 90f);
    }

    void UpdateFingers() {
        //2 FINGERS
        if (Input.touchCount == 2) {
            isMultiFinger = true;
            MultiFinger();
        } else if (Input.touchCount == 1) {
            // 1 FINGER
            Touch touch = Input.GetTouch(0);

            if (!isMultiFinger) {
                switch (touch.phase) {
                    case TouchPhase.Began:
                        fingerUp = touch.position;
                        fingerDown = touch.position;                        
                        break;

                    case TouchPhase.Ended:
                        fingerUp = touch.position;
                        if(!game.touchIsInRewindZone){
                            DetermineDirection();
                        }
                        break;
                }
            }                       
        } else if (Input.touchCount == 0) {
            if (oneFingerReleased) {
                oneFingerReleased = false;
            }
            isMultiFinger = false;
            SetCamPanSpeed(0);
        }
    }

    public float panSpeed;

    void SetCamPanSpeed(float amount){
        cameraScript.speedY = amount * panSpeed;
    }

    #endregion

    #region movement
    public float speed;
    public static bool allowRotate = true;

    void DetermineDirection() {
        float vertical = fingerUp.y - fingerDown.y;
        float horizontal = fingerUp.x - fingerDown.x;
        if (Mathf.Abs(vertical) > SWIPE_THRESHOLD || Mathf.Abs(horizontal) > SWIPE_THRESHOLD) {
            moveQueue.Add(new MoveQueueItem(directionScript.GetDirection(horizontal, vertical), Mathf.RoundToInt(playerBody.transform.position.y), false));                           
        }              
    }

    public bool movePlayerToTargetPos = false;
    public Vector3 targetPos;

    void Move_Update(){
        if (movePlayerToTargetPos){
            // Move position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.position, targetPos) < 0.001f) {
                movePlayerToTargetPos = false;
            }
        }        
    }

    public bool rotateToTargetRotation = false;
    Quaternion targetRotation;
    public float rotationSpeed;
    float rotationTimer = 0f;
    void Rotate_Update(){
        if (rotateToTargetRotation) {
            rotationTimer += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Slerp(Quaternion.identity, targetRotation, rotationTimer);

            if(Vector3.Distance(transform.eulerAngles, targetRotation.eulerAngles) < 0.01f){
                rotateToTargetRotation = false;
                rotationTimer = 0f;
            }
        }
    }


    IEnumerator SpinCubeAroundAxis(Vector3 axis, float angle){
        Transform targetRotationTransform = transform;
        targetRotationTransform.rotation = Quaternion.identity;
        targetRotationTransform.Rotate(axis, angle, Space.Self);
        targetRotation = targetRotationTransform.rotation;
        rotateToTargetRotation = true;

        yield return new WaitUntil(() => !rotateToTargetRotation);

        transform.eulerAngles = targetRotation.eulerAngles;
    }

    public static bool allowForward = true, allowBack = true, allowLeft = true, allowRight = true;

    public struct MoveQueueItem {
        public Direction direction;
        public int yPos;
        public bool isRewind;
        public MoveQueueItem(Direction direction, int yPos, bool isRewind){
            this.direction = direction;
            this.yPos = yPos;
            this.isRewind = isRewind;
        }
    }


    public static List<MoveQueueItem> moveQueue = new List<MoveQueueItem>();

    bool isDeleted = false; //one time execution of deleting movequeue items before rewind after falling

    Direction fallDirection;
    int fallColorIndex;

    void UpdateMoveQueue(){

        if (isFall) {
            if (!isDeleted) {
                restrictAllInput = true;
                isDeleted = true;
                for(int i = 0; i < moveQueue.Count; i++) {
                    if (!moveQueue[i].isRewind) {
                        moveQueue.Remove(moveQueue[i]);
                    } else {
                        //break;
                    }
                }
            }
        } else {
            isDeleted = false;
        }

        if (moveQueue.Count > 0 && allowRotate){            
            if (!moveQueue[0].isRewind) {
                moveHistory.Insert(0, new MoveHistoryItem(moveQueue[0].direction, moveQueue[0].yPos, playerColorCon.thisColorIndex));
            }

            StartCoroutine(MoveInDirection(moveQueue[0].direction, moveQueue[0].yPos, moveQueue[0].isRewind));
            moveQueue.RemoveAt(0);
        }
    }

    bool isBlockBelow = false;
    [HideInInspector] public bool justTeleported;

    public static bool currentlyRewinding = false;

    public int lowestBlockHeight = -30;

    public static bool moveableInMotion = false;

    IEnumerator MoveInDirection(Direction direction, int height, bool isRewindMove){

        for (int i = 0; i < game.moveables.Count; i++) {
            if (game.moveables[i].stillMoving) {
                yield return new WaitForSeconds(0.1f);
            }
        }

        SetPlayerColliderActive(true);
        justFell = false;
        currentlyRewinding = isRewindMove;

        allowRotate = false;
        
        bool movementBlocked = false;

        bool spinCubeAnimation = true;

        Vector3 roundedPos = GameController.RoundedPos(new Vector3(playerBody.transform.position.x, height, playerBody.transform.position.z));

        detectTeleports = !isRewindMove;

        
        switch (direction) {
            case Direction.Forward:
                if (allowForward || isRewindMove) {
                    targetPos = roundedPos + Vector3.forward;
                    if(spinCubeAnimation)
                        StartCoroutine(SpinCubeAroundAxis(Vector3.right, 90f));
                } else
                    movementBlocked = true;
                break;
            case Direction.Back:
                if (allowBack || isRewindMove) {
                    targetPos = roundedPos - Vector3.forward;
                    if (spinCubeAnimation)
                        StartCoroutine(SpinCubeAroundAxis(Vector3.right, -90f));
                } else
                    movementBlocked = true;
                break;
            case Direction.Left:
                if (allowLeft || isRewindMove) {
                    targetPos = roundedPos - Vector3.right;
                    if (spinCubeAnimation)
                        StartCoroutine(SpinCubeAroundAxis(Vector3.forward, 90f));
                } else
                    movementBlocked = true;
                break;
            case Direction.Right:
                if (allowRight || isRewindMove) {
                    targetPos = roundedPos + Vector3.right;
                    if (spinCubeAnimation)
                        StartCoroutine(SpinCubeAroundAxis(Vector3.forward, -90f));
                } else
                    movementBlocked = true;
                break;
        }

       
        if (!movementBlocked || isRewindMove) {
            movePlayerToTargetPos = true;

            bool floorIsEnabled;

            if (!game.allowedLocations.TryGetValue(targetPos - Vector3.up, out floorIsEnabled) || !floorIsEnabled) {
                OutOfBounds();
                bool isABlockBelow = false;
                for (int i = (int)targetPos.y - 1; i >= lowestBlockHeight; i--) {
                    Vector3 locationBelow = new Vector3(targetPos.x, i, targetPos.z);
                    if (game.allowedLocations.TryGetValue(locationBelow, out floorIsEnabled) && floorIsEnabled) {
                        FallTo(i + 1);
                        moveHistory.RemoveAt(0);
                        moveHistory.Insert(0, new MoveHistoryItem(direction, Mathf.RoundToInt(positionAtFall.y), playerColorCon.thisColorIndex));
                        isABlockBelow = true;
                        break;
                    }
                }
                if(!isABlockBelow)
                    Invoke("RevertFallForRewind", 1f);

            }

            


            yield return new WaitUntil(() => !movePlayerToTargetPos);
            yield return new WaitUntil(() => !rotateToTargetRotation);
    
            if (isRewindMove) {
                allowRotate = true;
            } else {
                justRewinded = false;
                /*
                for (int i = 0; i < game.moveables.Count; i++) {
                    yield return new WaitUntil(() => !game.moveables[i].stillMoving);
                }
                */
            }




            playerBody.position = new Vector3(targetPos.x, height, targetPos.z);
            
            

            moveCount = isRewindMove ? moveCount - 1 : moveCount + 1;

            game.UpdateMoveCountText();            
        } else {
            moveHistory.RemoveAt(0);
        }

        

        currentlyRewinding = false;        

        allowRotate = true;

        
    }

    int heightToFallTo;

    void CheckBlockFall_Update(){
        if (Mathf.Abs(playerBody.transform.position.y - heightToFallTo) <= 0.1f) {
            isBlockBelow = false;

            restrictAllInput = false;
            rbody.isKinematic = true;
            rbody.useGravity = false;
            allowRotate = allowForward = allowBack = allowLeft = allowRight = true;           
            isFall = false;

            playerBody.position = new Vector3(playerBody.position.x, heightToFallTo, playerBody.position.z);
            SetPlayerColliderActive(true);
            StartCoroutine(resetMoveablesDestroyOnObstructedLandingDelayed());
        }
    }

    public void setMoveablesDestroyOnObstructedLanding(){
        StopCoroutine(resetMoveablesDestroyOnObstructedLandingDelayed());
        for (int i = 0; i < game.moveables.Count; i++) {
            game.moveables[i].disintegration.destroyOnObstructedLanding = true;
        }
    }

    public IEnumerator resetMoveablesDestroyOnObstructedLandingDelayed(){
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < game.moveables.Count; i++) {
            game.moveables[i].disintegration.destroyOnObstructedLanding = false;
        }

        allowRotate = true;
        restrictAllInput = false;

    }

    void FallTo(int heightToFall){
        heightToFallTo = heightToFall;
        isBlockBelow = true;
        setMoveablesDestroyOnObstructedLanding();
    }

    struct MoveHistoryItem {
        public Direction direction;
        public int yPos;
        public int colorIndex;
        public MoveHistoryItem (Direction direction, int yPos, int colorIndex){
            this.direction = direction;
            this.yPos = yPos;
            this.colorIndex = colorIndex;
        }
    }

    List<MoveHistoryItem> moveHistory = new List<MoveHistoryItem>();

    public static bool rewindDone = true;

    public static bool detectTeleports = true;

    public bool justRewinded = false;

    public static bool justFell = false;

    public Dictionary<int, Vector3Int> teleportHistory = new Dictionary<int, Vector3Int>();

    public void Rewind(){
        if (moveHistory.Count > 0) {
            if (teleportHistory.ContainsKey(moveCount)){
                //special
                transform.position = teleportHistory[moveCount];
                teleportHistory.Remove(moveCount);
            }
            playerColorCon.SetColor(moveHistory[0].colorIndex);
            int inversedDirectionValue = (int)moveHistory[0].direction * -1;
            StartCoroutine(MoveInDirection((Direction)inversedDirectionValue, moveHistory[0].yPos, true));
            if (moveHistory[0].yPos > transform.position.y)
                PlayerScript.allowForward = PlayerScript.allowBack = PlayerScript.allowLeft = PlayerScript.allowRight = true;

            moveHistory.RemoveAt(0);
            justRewinded = true;
        }
    }


    [HideInInspector] public static bool isFall = false;
    #endregion

    #region Game Status

    Vector3 positionAtFall;

    BoxCollider[] playerColliders;

    public void OutOfBounds() {
        positionAtFall = GameController.RoundedPos(transform.position);

        restrictAllInput = true;
        rbody.isKinematic = false;
        rbody.useGravity = true;
        allowForward = allowBack = allowLeft = allowRight = false;
        
        allowRotate = false;
        isFall = true;
        justFell = true;
        SetPlayerColliderActive(false);
        
    }
    
    void RevertFall(){
        rbody.isKinematic = true;
        rbody.useGravity = false;
        allowForward = allowBack = allowLeft = allowRight = true;
        allowRotate = true;
        isFall = false;
        SetPlayerColliderActive(true);
    }

    public void RevertFallForRewind(){
        if (isFall) {
            RevertFall();
            transform.position = positionAtFall;
            playerBody.position = positionAtFall;
            SetPlayerColliderActive(false);
            moveCount--;
            game.UpdateMoveCountText();
            moveHistory.RemoveAt(0);
            restrictAllInput = false;
        }
        
    }

        

    public void SetPlayerColliderActive(bool wantActive){
        foreach (BoxCollider col in playerColliders)
            col.enabled = wantActive;
    }

    #endregion


    public static Vector3Int posAtPause;
    /*
    void OnApplicationQuit() {
        posAtPause = GameController.RoundedVectorInt(transform.position);
    }
    */
    void Save(){

        SaveScript.infoToSave.playerPosition = new int[] { posAtPause.x, posAtPause.y, posAtPause.z };
        print("SAVE" + new Vector3(SaveScript.infoToSave.playerPosition[0], SaveScript.infoToSave.playerPosition[1], SaveScript.infoToSave.playerPosition[2]));

        SaveScript.infoToSave.playerCurrentColorIndex = playerColorCon.thisColorIndex;
        SaveScript.infoToSave.playerIsFall = isFall;
        
        int[] historyDirections = new int[moveHistory.Count];
        int[] historyHeights = new int[moveHistory.Count];
        int[] historyColorIndexes = new int[moveHistory.Count];

        for (int i = 0; i < moveHistory.Count; i++) {
            historyDirections[i] = (int)moveHistory[i].direction;
            historyHeights[i] = moveHistory[i].yPos;
            historyColorIndexes[i] = moveHistory[i].colorIndex;
        }
        SaveScript.infoToSave.playerMoveHistory_Directions = historyDirections;
        SaveScript.infoToSave.playerMoveHistory_heights = historyHeights;
        SaveScript.infoToSave.playerMoveHistory_playerColorIndexes = historyColorIndexes;

        SaveScript.infoToSave.moveCount = moveCount;

        SaveScript.infoToSave.teleportHistoryKeys = new int[teleportHistory.Keys.Count];
        SaveScript.infoToSave.teleportHistoryValues = new int[teleportHistory.Values.Count * 3];

        int keyCounter = 0;
        int valueCounter = 0;
        foreach (int keyIndex in teleportHistory.Keys){
            SaveScript.infoToSave.teleportHistoryKeys[keyCounter] = keyIndex;
            keyCounter++;

            SaveScript.infoToSave.teleportHistoryValues[valueCounter] = teleportHistory[keyIndex].x;
            valueCounter++;
            SaveScript.infoToSave.teleportHistoryValues[valueCounter] = teleportHistory[keyIndex].y;
            valueCounter++;
            SaveScript.infoToSave.teleportHistoryValues[valueCounter] = teleportHistory[keyIndex].z;
            valueCounter++;
        }
    }

    void Load(SaveInfo info) {


        moveCount = info.moveCount;
        game.UpdateMoveCountText();

        transform.position = new Vector3(info.playerPosition[0], info.playerPosition[1], info.playerPosition[2]);
        print(info.playerPosition[0]);
        isFall = info.playerIsFall;
        if (!info.playerIsFall) {
            RevertFall();
        }

        playerColorCon.SetColor(info.playerCurrentColorIndex);

        moveHistory.Clear();
        for (int i = 0; i < info.playerMoveHistory_Directions.Length; i++) {
            moveHistory.Add(new MoveHistoryItem((Direction)info.playerMoveHistory_Directions[i], info.playerMoveHistory_heights[i], info.playerMoveHistory_playerColorIndexes[i]));            
        }

        teleportHistory.Clear();
        for (int i = 0; i < info.teleportHistoryKeys.Length; i++) {
            Vector3Int v = new Vector3Int(info.teleportHistoryValues[i * 3], info.teleportHistoryValues[i * 3 + 1], info.teleportHistoryValues[i * 3 + 2]);
            teleportHistory.Add(info.teleportHistoryKeys[i], v);
        }
    }
}
