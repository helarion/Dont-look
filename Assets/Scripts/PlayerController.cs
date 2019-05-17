using Rewired;
using System.Collections;
using UnityEngine;
using Tobii.Gaming;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class PlayerController : MonoBehaviour
{
    #region variables

    #region SavedVariables
    [Header("Models & saved objects")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Transform raycastClimb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform hipPosition;
    [SerializeField] private Transform headPosition;
    [SerializeField] private Transform armPosition;

    private CameraBlock currentCameraBlock = null;
    private AudioRoom currentAudioRoom = null;
    [HideInInspector] public SpatialRoom currentSpatialRoom = null;
    SpatialSas currentSpatialSas = null;
    SpatialLine currentSpatialLine = null;
    private Rigidbody objectGrabbed = null;
    private float objectGrabbedWidth = 0;
    private Rigidbody rb;
    private Collider cl;
    private Vector3 lookAtPos;
    private Vector2 cursorPos;
    Vector3 lMovement;

    #endregion

    #region JumpVariables

   [Header("Jump")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float rayCastLength = 0.1f;
    [SerializeField] private float maxClimbHeight = 1.0f;
    [SerializeField] private float maxClimbLength = 1.0f;
    [SerializeField] private float jumpLength = 3;
    [SerializeField] private float jumpLengthSpeed = 1;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private GroundDetector groundDetector;
    private int jumpDirection = 0;

    #endregion

    #region MovementVariables

    [Header("Movement")]
    [SerializeField] private float walkTime;
    [SerializeField] private float runTimeMinus;
    [SerializeField] private float deadZoneValue = 0.3f;
    [SerializeField] private float changeLineDeadZoneValue = 0.9f;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float ladderSpeed = 0.5f;
    [SerializeField] private float grabSpeed = 0.5f;
    private bool isChangingSpatialLine = false;
    private bool walkRoutine = false;
    private bool StoppedHMove = false;
    private bool stopMove = false;
    private bool isMoving = false;
    private bool isRunning = false;

    private Vector3 _verticalLastMovement = Vector3.zero;
    private Vector3 localModelPosition;
    private Vector3 worldModelPosition;
    private Vector3 climbPosition;
    private Vector3 lastPosition;
    public float velocity;
    public float yVelocity;

    private float moveSpeed;
    private float vMove;
    private float hMove;
    private int inverse = 1;
    private int changingLineDirection = 1;

    #endregion

    #region AccelerationDecelerationVariables

    [Header("Acceleration Decceleration Curves")]
    [SerializeField] private AnimationCurve _horizontalAccelerationCurve;
    [SerializeField] private AnimationCurve _verticalAccelerationCurve;
    [SerializeField] private AnimationCurve _horizontalDecelerationCurve;
    [SerializeField] private AnimationCurve _verticalDecelerationCurve;
    [SerializeField] private float _horizontalAccSpeed = 1;
    [SerializeField] private float _horizontalDecSpeed = 1;
    [Range(-1, 1)]
    private float _horizontalAccDecLerpValue;
    private Vector3 _horizontalLastMovement = Vector3.zero;
    [SerializeField] private float _verticalAccSpeed = 1;
    [SerializeField] private float _verticalDecSpeed = 1;
    [Range(-1, 1)]
    private float _verticalAccDecLerpValue;

    #endregion

    #region LightVariables
    [Header("Light")]
    [SerializeField] private float bodyRotationDeadZone = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float lightSensitivity = 1;
    [SerializeField] private float lightSpeed = 1;
    [SerializeField] private Transform flashlightTransform;
    [SerializeField] private Light pointLight;
    [SerializeField] private int flickerPercentage = 10;
    [SerializeField] private int flickeringFrequency = 1;
    [SerializeField] public float rangeDim;
    [SerializeField] public Animator flashlightAnimator;
    [SerializeField] private float lightTransitionSpeed=0.1f;
    [SerializeField] private float normalCameraFOV;
    [SerializeField] private float zoomCameraFOV;

    [SerializeField] private float concentratedLightRangeBonus;
    [SerializeField] private float concentratedLightIntensity;
    [SerializeField] private Color concentratedLightColor;
    [SerializeField] private float concentratedLightAngle;
    [SerializeField] private float normalLightIntensity;
    [SerializeField] private float normalLightRange;
    [SerializeField] private Color normalLightColor;
    [SerializeField] private float normalLightAngle;
    private bool isConcentrating=false;
    public bool lightOn = true;
    private Light flashlight;
    #endregion

    #region StatesVariables
    [Header("State")]
    private bool isHidden = false;
    private bool isAlive = true;

    private bool isFalling = false;
    private bool hasPlayedHeart = false;
    private bool needsCentering = false;
    private bool isInElevator = false;
    private int controler = -1;
    #endregion

    //Vector3 headLookAt;

    enum LookDirection { Left, Right, Front, Back};
    LookDirection currentLookDirection = LookDirection.Right;

    public enum InputMode { PC, Pad};
    InputMode inputMode = InputMode.Pad;

    #endregion

    #region StartUpdate

    private void Start()
    {
        isAlive = true;
        cl = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        flashlight = flashlightTransform.GetComponent<Light>();
        flashlight.type = LightType.Spot;
        ClosedEyes(false);
        Cursor.visible = false;
        moveSpeed = walkSpeed;
        cursorPos = Input.mousePosition;
        lastPosition = transform.position;
        localModelPosition = modelTransform.localPosition;

        //headLookAt = rotatePointAround(transform.position + Vector3.right, headPosition.position, Vector3.up, -90);
        StartFlickering();
    }

    private void Update()
    {
        CheckTrackerConnected();
        if (!isAlive || GameManager.instance.GetIsPaused()) return;
        LightAim();
        LightMode();
        GroundedCheck();
        FallingCheck();
        MoveInputUpdate();
    }

    private void FixedUpdate()
    {
        if (!isAlive || GameManager.instance.GetIsPaused()) return;

        velocity = (transform.position - lastPosition).magnitude;
        yVelocity = (transform.position.y - lastPosition.y);

        lastPosition = transform.position;

        if (stopMove || isFalling) return;
        Move();

        BodyRotation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, lookAtPos);
        Gizmos.color = Color.green;
        if (Application.isPlaying)
        {
            //print("spatialline begin"+currentSpatialLine.begin.position);
            Gizmos.DrawLine(currentSpatialLine.begin.position, currentSpatialLine.end.position);
        }
        Gizmos.color = Color.white;
    }

    #endregion

    #region TriggersCollision

    private void OnTriggerEnter(Collider other)
    {
        CameraBlock cameraBlock = other.GetComponent<CameraBlock>();
        if (cameraBlock != null)
        {
            currentCameraBlock = cameraBlock;
            CameraBlockChanges();
            return;
        }

        SpatialSas spatialSas = other.GetComponent<SpatialSas>();
        if (spatialSas != null)
        {
            currentSpatialSas = spatialSas;
            currentSpatialLine = spatialSas.spatialLine;
            isChangingSpatialLine = true;
            changingLineDirection = 1;
            return;
        }

        SpatialRoom spatialRoom = other.GetComponent<SpatialRoom>();
        if (spatialRoom != null)
        {
            currentSpatialRoom = spatialRoom;
            if (currentSpatialSas == null)
            {
                float zOffset = Mathf.Infinity;
                foreach (SpatialLine sl in currentSpatialRoom._spatialLines)
                {
                    if (Mathf.Abs(sl.begin.position.z - transform.position.z) < zOffset)
                    {
                        zOffset = Mathf.Abs(sl.begin.position.z - transform.position.z);
                        currentSpatialLine = sl;
                        isChangingSpatialLine = true;
                        changingLineDirection = 1;
                    }
                }
            }
            return;
        }

        else if(other.CompareTag("Hideout"))
        {
            isHidden = true;
        }
        else if (other.CompareTag("Killzone"))
        {
            BipedeBehavior b = other.GetComponentInParent<BipedeBehavior>();
            if(b!=null)
            {
                if(!b.isLooked) GameManager.instance.Death();
            }
            else   GameManager.instance.Death();
        }
        else if(other.CompareTag("DetectZone"))
        {
            Enemy e = other.GetComponentInParent<Enemy>();
            e.DetectPlayer(true);
        }
        else if(other.CompareTag("Finish"))
        {
            GameManager.instance.camHandler.DestroyTarget();
            UIManager.instance.FadeInEnd();
        }
        else if (other.CompareTag("Elevator"))
        {
            Elevator elevator = other.GetComponent<Elevator>();
            if(elevator!=null)
            {
                elevator.isPlayerOnBoard = true;
                elevator.StartMoving();
                animator.SetBool("IsMoving", false);
                isInElevator = true;
                print("elevator starts moving");
            }
        }
        else if (other.CompareTag("NeedsCentering"))
        {
            needsCentering = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CameraBlock camBlock = other.GetComponent<CameraBlock>();
        if(camBlock != null)
        {
            if(currentCameraBlock == camBlock)
            currentCameraBlock = null;
            return;
        }
        
        if (other.GetComponent<SpatialSas>() != null)
        {
            currentSpatialSas = null;
            float zOffset = Mathf.Infinity;
            float currentZ = currentSpatialLine.begin.position.z;
            float currentXBegin = currentSpatialLine.begin.position.x;
            float currentXEnd = currentSpatialLine.end.position.x;
            foreach (SpatialLine sl in currentSpatialRoom._spatialLines)
            {
                if (sl.begin.position.x < currentXEnd && sl.end.position.x > currentXBegin)
                {
                    if (Mathf.Abs(sl.begin.position.z - currentZ) < zOffset)
                    {
                        zOffset = Mathf.Abs(sl.begin.position.z - currentZ);
                        currentSpatialLine = sl;
                        isChangingSpatialLine = true;
                        changingLineDirection = 1;
                    }
                }
            }
            return;
        }

        if (other.CompareTag("Hideout"))
        {
            isHidden = false;
        }
        else if(other.CompareTag("NeedsCentering"))
        {
            needsCentering = false;
        }
        else if (other.CompareTag("Elevator"))
        {
            Elevator elevator = other.GetComponent<Elevator>();
            if (elevator != null)
            {
                isInElevator = false;
                elevator.isPlayerOnBoard = false;
            }
        }

        else if (other.CompareTag("DetectZone"))
        {
            Enemy e = other.GetComponentInParent<Enemy>();
            e.DetectPlayer(false);
        }
    }

    Vector3 rotatePointAround(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
    {
        GameObject gameObject = new GameObject();
        gameObject.transform.position = point;
        gameObject.transform.RotateAround(pivot, axis, angle);
        Vector3 result = gameObject.transform.position;
        Destroy(gameObject);
        return result;
    }

    #endregion

    #region audio

    private void PlayHeart()
    {
        if (!hasPlayedHeart)
        {
            hasPlayedHeart = true;
            AkSoundEngine.PostEvent(GameManager.instance.HeartPlay, GameManager.instance.gameObject);
        }
    }

    private void StopHeart()
    {
        if (hasPlayedHeart)
        {
            hasPlayedHeart = false;
            AkSoundEngine.PostEvent(GameManager.instance.HeartStop, GameManager.instance.gameObject);
        }
    }

    #endregion

    #region Light

    private void CheckTrackerConnected()
    {
        if (!TobiiAPI.IsConnected)
        {
            UIManager.instance.DisableControlPanel(true);
        }
        else
        {
            UIManager.instance.DisableControlPanel(false);
        }
    }

    private void LightMode()
    {
        if(GameManager.instance.controls.GetButton("Concentrate"))
        {
            isConcentrating = true;

            flashlight.range = Mathf.Lerp(flashlight.range,normalLightRange+concentratedLightRangeBonus,lightTransitionSpeed);
            flashlight.intensity = Mathf.Lerp(flashlight.intensity,concentratedLightIntensity,lightTransitionSpeed);
            flashlight.color = Color.Lerp(flashlight.color, concentratedLightColor, lightTransitionSpeed);
            pointLight.color = Color.Lerp(pointLight.color, concentratedLightColor, lightTransitionSpeed);
            flashlight.spotAngle = Mathf.Lerp(flashlight.spotAngle, normalLightAngle/concentratedLightAngle, lightTransitionSpeed);
            GameManager.instance.mainCamera.fieldOfView = Mathf.Lerp(GameManager.instance.mainCamera.fieldOfView, zoomCameraFOV, lightTransitionSpeed);
            GameManager.instance.camHandler.Zoom(true);
            flashlightAnimator.enabled = false;
        }
        else
        {
            isConcentrating = false;
            flashlight.range = Mathf.Lerp(flashlight.range, normalLightRange, lightTransitionSpeed);
            flashlight.intensity = Mathf.Lerp(flashlight.intensity, normalLightIntensity, lightTransitionSpeed);
            flashlight.color = Color.Lerp(flashlight.color, normalLightColor, lightTransitionSpeed);
            pointLight.color = Color.Lerp(pointLight.color, normalLightColor, lightTransitionSpeed);
            flashlight.spotAngle = Mathf.Lerp(flashlight.spotAngle, normalLightAngle, lightTransitionSpeed);
            GameManager.instance.mainCamera.fieldOfView = Mathf.Lerp(GameManager.instance.mainCamera.fieldOfView, normalCameraFOV, lightTransitionSpeed);
            GameManager.instance.camHandler.Zoom(false);
            if (lightOn) flashlightAnimator.enabled = true;
        }
    }

    private void LightAim()
    {
        // LIGHT AIM CONTROL
        if (GameManager.instance.GetIsTrackerEnabled() && TobiiAPI.IsConnected) // EYE TRACKER OPTION
        {
            if (!TobiiAPI.GetGazePoint().IsRecent())
            {
                ClosedEyes(true);
            }
            else
            {
                ClosedEyes(false);
            }

            cursorPos = TobiiAPI.GetGazePoint().Screen;
        }
        else
        {
            // CHECK DU BOUTON POUR FERMER LES YEUX SI L'EYE TRACKER N'EST PAS ACTIVÉ
            if (GameManager.instance.controls.GetAxis("LightOff")!=0)
            {
                ClosedEyes(true);
            }
            else
            {
                ClosedEyes(false);
            }

            float xLight = GameManager.instance.controls.GetAxis("Light Horizontal");
            float yLight = GameManager.instance.controls.GetAxis("Light Vertical");
            
            if(Input.GetAxis("Mouse X")!=0 || Input.GetAxis("Mouse Y")!=0)
            {
                cursorPos = Input.mousePosition;
                inputMode = InputMode.PC;
            }
            else if (Mathf.Abs(xLight) > 0.1f || Mathf.Abs(yLight) > 0.1f)
            {
                cursorPos.x = ((xLight * lightSensitivity + 1) / 2) * GameManager.instance.mainCamera.pixelWidth;
                cursorPos.y = ((yLight * lightSensitivity + 1) / 2) * GameManager.instance.mainCamera.pixelHeight;
                inputMode = InputMode.Pad;
            }
            else if (inputMode == InputMode.Pad)
            {
                cursorPos = GameManager.instance.mainCamera.WorldToScreenPoint(transform.position);
            }
        }

        RaycastHit hit;
        Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(cursorPos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetLookLayer()))
        {
            lookAtPos = hit.point;
        }

        /* --- Code pour vérouiller le z de la lampe --- */
        /*
        Vector3 lightAimVec = lookAtPos - GameManager.instance.mainCamera.transform.position;
        Vector3 cameraPosBack = GameManager.instance.mainCamera.transform.position;
        cameraPosBack.z = lookAtPos.z;
        Vector3 cameraPosPlayer = GameManager.instance.mainCamera.transform.position;
        cameraPosPlayer.z = transform.position.z + 2;
        float vecRate = (cameraPosPlayer - GameManager.instance.mainCamera.transform.position).magnitude / (cameraPosBack - GameManager.instance.mainCamera.transform.position).magnitude;
        lookAtPos = GameManager.instance.mainCamera.transform.position + lightAimVec * vecRate;
        */
        /* ---  --- */

        //cameraLight.rotation = Quaternion.Slerp(cameraLight.rotation, Quaternion.LookRotation(lookAtPos - cameraLight.position), Time.fixedDeltaTime * lightSpeed * 100);


        flashlightTransform.rotation = Quaternion.Slerp(flashlightTransform.rotation, Quaternion.LookRotation(lookAtPos - flashlightTransform.position), Time.deltaTime * lightSpeed * 100);
    }

    // APPELER LORSQUE LE JOUEUR FERME LES YEUX
    private void ClosedEyes(bool isClosed)
    {
        flashlight.enabled = !isClosed;
        pointLight.enabled = !isClosed;
        lightOn = !isClosed;

        if(isClosed)
        {
            GameManager.instance.PlayHeart();
            flashlightAnimator.enabled = false;
        }
        else
        {
            GameManager.instance.StopHeart();
            flashlightAnimator.enabled = true;
        }
    }

    public void StartFlickering()
    {
        StartCoroutine(RandomFlickerCoroutine());
    }

    private void RandomFlicker()
    {
        int rand = Random.Range(0, 100);
        if (rand <= flickerPercentage)
        {
            flashlightAnimator.SetTrigger("Flicker1");
            //print("Flickers");
        }
    }

    IEnumerator RandomFlickerCoroutine()
    {
        while (true)
        {
            RandomFlicker();
            yield return new WaitForSeconds(flickeringFrequency);
        }
        //yield return null;
    }

    #endregion

    #region System
    public void Reset()
    {
        isHidden = false;
        SetIsAlive(true);
        isConcentrating = false;
        isInElevator = false;
        isRunning = false;
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsFalling", false);
        animator.SetBool("HasLanded", false);
        StoppedHMove = false;
        stopMove = false;
        ResetVelocity();
    }

    private void CameraBlockChanges()
    {
        GameManager.instance.SetContrePlongeeAngle(currentCameraBlock.room.newContrePlongeeAngle);
        GameManager.instance.SetContrePlongeeHauteur(currentCameraBlock.room.newContrePlongeeHauteur);
        GameManager.instance.camHandler.SetNewZ(currentCameraBlock.room.newZ);
        GameManager.instance.camHandler.SetNewOffset(currentCameraBlock.room.newOffset);
        SetLightRange(currentCameraBlock.room.newLightRange);
        GameManager.instance.SetDutchAngle(currentCameraBlock.room.newDutchAngle);
        SetLightAngle(currentCameraBlock.room.newLightAngle);
        //print("enter cameraBLock");
        if (currentCameraBlock.updatedDecals.Length > 0)
        {
            //print("must change decal");
            int i = 0;
            foreach (DecalProjectorComponent d in currentCameraBlock.updatedDecals)
            {
                if (inputMode == InputMode.PC)
                {
                    // print("input pc");
                    d.m_Material = currentCameraBlock.keyboardMaterials[i];
                }
                else
                {
                    //print("input manette");
                    d.m_Material = currentCameraBlock.gamePadMaterials[i];
                }
                d.enabled = false;
                d.enabled = true;
                //print("mat changé");
                i++;
            }
        }
    }
    #endregion

    #region Movement

    private void MoveInputUpdate()
    {
        hMove = GameManager.instance.controls.GetAxis("Move Horizontal");
        vMove = GameManager.instance.controls.GetAxis("Move Vertical");

        Controller controller = GameManager.instance.controls.controllers.GetLastActiveController();
        if (controller != null)
        {
            switch (controller.type)
            {
                case ControllerType.Keyboard:
                    inputMode = InputMode.PC;
                    break;
                case ControllerType.Joystick:
                    inputMode = InputMode.Pad;
                    break;
                case ControllerType.Mouse:
                    inputMode = InputMode.PC;
                    break;
            }
        }
    }

    private void Move()
    {
        isMoving = false;
        lMovement = Vector3.zero;
        Vector3 camMove = Vector3.zero;

        if (hMove < deadZoneValue && hMove > -deadZoneValue && inputMode == InputMode.Pad) hMove = 0;
        if (!needsCentering)
        {
            if (Mathf.Abs(hMove) > 0)
                lMovement += HorizontalMove(hMove);
            else if (_horizontalAccDecLerpValue != 0)
                lMovement += HorizontalSlowDown();
        }

        if (currentSpatialSas == null)
        {
            if ((vMove >= changeLineDeadZoneValue || (vMove > 0 && inputMode == InputMode.PC)) && !(isChangingSpatialLine && changingLineDirection == 1))
            {
                for (int i = 0; i < currentSpatialRoom._spatialLines.Count; i++)
                {
                    SpatialLine sl = currentSpatialRoom._spatialLines[i];
                    if (sl.begin.position.z > currentSpatialLine.begin.position.z)
                    {
                        if (transform.position.x >= sl.begin.position.x && transform.position.x <= sl.end.position.x)
                        {
                            currentSpatialLine = sl;
                            isChangingSpatialLine = true;
                            changingLineDirection = 1;
                            break;
                        }
                    }
                }
            }
            else if ((vMove <= -changeLineDeadZoneValue || (vMove < 0 && inputMode == InputMode.PC)) && !(isChangingSpatialLine && changingLineDirection == -1))
            {
                for (int i = currentSpatialRoom._spatialLines.Count - 1; i >= 0; i--)
                {
                    SpatialLine sl = currentSpatialRoom._spatialLines[i];
                    if (sl.begin.position.z < currentSpatialLine.begin.position.z)
                    {
                        if (transform.position.x >= sl.begin.position.x && transform.position.x <= sl.end.position.x)
                        {
                            currentSpatialLine = sl;
                            isChangingSpatialLine = true;
                            changingLineDirection = -1;
                            break;
                        }
                    }
                }
            }
        }

        if (lMovement != Vector3.zero || isChangingSpatialLine)
        {
            isMoving = true;
            if (GameManager.instance.controls.GetAxisRaw("Sprint") != 0)// && inverse==1)
            {
                isRunning = true;
                animator.SetBool("IsRunning", true);
                moveSpeed = runSpeed;
            }
            else
            {
                isRunning = false;
                animator.SetBool("IsRunning", false);
                moveSpeed = walkSpeed;
            }

            if (needsCentering && currentSpatialLine.begin.position.z > transform.position.z)
            {
                float targetPosX = (currentSpatialLine.end.position.x + currentSpatialLine.begin.position.x) / 2;
                int horizontalDirection = (transform.position.x - targetPosX) > 0 ? -1 : 1;
                Vector3 horizontalMove = HorizontalMove(horizontalDirection);
                if (horizontalDirection == -1)
                {
                    if (horizontalMove.x > 0.0f)
                    {
                        horizontalMove.x = 0.0f;
                    }
                }
                else
                {
                    if (horizontalMove.x < 0.0f)
                    {
                        horizontalMove.x = 0.0f;
                    }
                }
                lMovement += horizontalMove;
                Vector3 transformPosition = transform.position + lMovement;
                if (lMovement.x > 0.0f)
                {
                    if (transformPosition.x > targetPosX)
                    {
                        print("je tp >0");
                        lMovement.x = targetPosX - transform.position.x;
                    }
                }
                else if (lMovement.x < 0.0f)
                {
                    if (transformPosition.x < targetPosX)
                    {
                        print("je tp <0");
                        lMovement.x = targetPosX - transform.position.x;
                    }
                }
            }
            else if (currentSpatialSas == null)
            {
                Vector3 transformPosition = transform.position + lMovement;
                if (transformPosition.x < currentSpatialLine.begin.position.x)
                {
                    lMovement.x = currentSpatialLine.begin.position.x - transform.position.x;
                }
                else if (transformPosition.x > currentSpatialLine.end.position.x)
                {
                    lMovement.x = currentSpatialLine.end.position.x - transform.position.x;
                }
            }

            if (isChangingSpatialLine)
            {
                /*lMovement.z = (transform.position.z - currentSpatialLine.begin.position.z) > 0 ? -1 : 1;
                lMovement.z *= moveSpeed;*/

                int verticalDirection = (transform.position.z - currentSpatialLine.begin.position.z) > 0 ? -1 : 1;
                Vector3 verticalMove = VerticalMove(verticalDirection);
                if (verticalDirection == -1)
                {
                    if (verticalMove.z > 0.0f)
                    {
                        verticalMove.z = 0.0f;
                    }
                }
                else
                {
                    if (verticalMove.z < 0.0f)
                    {
                        verticalMove.z = 0.0f;
                    }
                }
                lMovement += verticalMove;
                Vector3 transformPosition = transform.position + lMovement;
                if (lMovement.z > 0.0f)
                print("movementz"+lMovement.z);
                {
                    if (transformPosition.z > currentSpatialLine.begin.position.z)
                    {
                        //print("je tp >0");
                        lMovement.z = currentSpatialLine.begin.position.z - transform.position.z;
                    }
                }
                else if (lMovement.z < 0.0f)
                {
                    if (transformPosition.z < currentSpatialLine.begin.position.z)
                    {
                        //print("je tp <0");
                        lMovement.z = currentSpatialLine.begin.position.z - transform.position.z;
                    }
                }

                if (Mathf.Abs(transform.position.z - currentSpatialLine.begin.position.z) < 0.001f)
                {
                    isChangingSpatialLine = false;
                }
            }
            else if(_horizontalAccDecLerpValue != 0)
            {
                lMovement += VerticalSlowDown();
            }

            rb.MovePosition(transform.position + lMovement);

            /*if (Mathf.Abs(hMove) < 0.01f && Mathf.Abs(vMove) < 0.01f)
            {
                isMoving = false;
            }*/

            
        }
        if (Mathf.Abs(lMovement.x) < 0.01f && Mathf.Abs(lMovement.z) < 0.01f)
        {
            isMoving = false;
        }
        animator.SetBool("IsMoving", isMoving);

    }

    public void PlaySoundWalk()
    {
        AkSoundEngine.PostEvent("Play_Placeholder_Footsteps_Concrete_Walk", gameObject);
    }

    public void PlaySoundRun()
    {
        AkSoundEngine.PostEvent("Play_Placeholder_Footsteps_Concrete_Run", gameObject);
    }

    private Vector3 HorizontalMove(float lXmovValue)
    {
        _horizontalAccDecLerpValue += Time.fixedDeltaTime * _horizontalAccSpeed * Mathf.Sign(lXmovValue);
        _horizontalAccDecLerpValue = Mathf.Clamp(_horizontalAccDecLerpValue, -1, 1);

        Vector3 lMovement = new Vector3(Mathf.Abs(lXmovValue), 0, 0);
        lMovement = lMovement.normalized * moveSpeed * Time.fixedDeltaTime * Mathf.Sign(_horizontalAccDecLerpValue);

        _horizontalLastMovement = lMovement;

        lMovement *= _horizontalAccelerationCurve.Evaluate(Mathf.Abs(_horizontalAccDecLerpValue));

        return lMovement;
    }

    private Vector3 VerticalMove(float lYmovValue)
    {
        _verticalAccDecLerpValue += Time.fixedDeltaTime * _verticalAccSpeed * Mathf.Sign(lYmovValue);
        _verticalAccDecLerpValue = Mathf.Clamp(_verticalAccDecLerpValue, -1, 1);

        Vector3 lMovement;
        lMovement = new Vector3(0, 0, Mathf.Abs(lYmovValue));
        lMovement = lMovement.normalized * moveSpeed * Time.fixedDeltaTime * Mathf.Sign(_verticalAccDecLerpValue);
        _verticalLastMovement = lMovement;
        lMovement *= _verticalAccelerationCurve.Evaluate(Mathf.Abs(_verticalAccDecLerpValue));

        return lMovement;
    }

    private Vector3 HorizontalSlowDown()
    {
        float pastLerp = _horizontalAccDecLerpValue;
        _horizontalAccDecLerpValue -= Time.fixedDeltaTime * _horizontalDecSpeed * Mathf.Sign(_horizontalAccDecLerpValue);
        if (Mathf.Sign(pastLerp) != Mathf.Sign(_horizontalAccDecLerpValue))
        {
            _horizontalAccDecLerpValue = 0;
        }

        Vector3 lMovement = _horizontalLastMovement;
        lMovement *= _horizontalDecelerationCurve.Evaluate(Mathf.Abs(_horizontalAccDecLerpValue));
        //print("SLOW = " + lMovement.x + " lastMovement = " + _horizontalLastMovement.x + " and lerp = " + _horizontalDecelerationCurve.Evaluate(Mathf.Abs(_horizontalAccDecLerpValue)) + " lerp = " + _horizontalAccDecLerpValue);

        return lMovement;
    }

    private Vector3 VerticalSlowDown()
    {
        float pastLerp = _verticalAccDecLerpValue;
        _verticalAccDecLerpValue -= Time.fixedDeltaTime * _verticalDecSpeed * Mathf.Sign(_verticalAccDecLerpValue);
        if (Mathf.Sign(pastLerp) != Mathf.Sign(_verticalAccDecLerpValue))
        {
            _verticalAccDecLerpValue = 0;
        }

        Vector3 lMovement = _verticalLastMovement;
        lMovement *= _verticalDecelerationCurve.Evaluate(Mathf.Abs(_verticalAccDecLerpValue));

        return lMovement;
    }

    private void BodyRotation()
    {
        Quaternion save = flashlight.transform.rotation;
        float speed = 0.1f;
        if (lMovement.z > 0 && isChangingSpatialLine)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 0, 0)), speed);
            inverse = 1;
            currentLookDirection = LookDirection.Front;
        }
        else if (lMovement.z < 0 && isChangingSpatialLine)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), speed);
            inverse = -1;
            currentLookDirection = LookDirection.Back;
        }
        else
        {
            if ((lookAtPos.x - transform.position.x > -bodyRotationDeadZone) || (currentLookDirection == LookDirection.Left && lookAtPos.x - transform.position.x > bodyRotationDeadZone))
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 89, 0)), speed / 2.0f);
                currentLookDirection = LookDirection.Right;
                if (hMove > 0) inverse = 1;
                else inverse = -1;
            }
            else
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 271, 0)), speed / 2.0f);
                currentLookDirection = LookDirection.Left;
                if (hMove < 0) inverse = 1;
                else inverse = -1;
            }
        }
        flashlight.transform.rotation = save;
        animator.SetFloat("Inverse", inverse);
    }


    // Bouge les pieds du joueur à la position donnée
    public void moveTo(Vector3 positionToMove)
    {
        positionToMove.y += cl.bounds.center.y - cl.bounds.min.y;
        transform.position = positionToMove;
    }

    #endregion

    #region Ground & Fall
    private void GroundedCheck()
    {
        if (!isGrounded) isFalling = true;
        isGrounded = false;
        isGrounded = groundDetector.GetIsGrounded();
        if (!isFalling) return;
        if (isGrounded)
        {
            isFalling = false;
            stopMove = false;
            animator.SetBool("IsFalling", false);
            animator.SetBool("HasLanded", true);
        }
    }

    private void FallingCheck()
    {
        if(yVelocity < 0 && !isInElevator &&!stopMove)
        {
            animator.SetBool("IsFalling", true);
        }
        else
        {
            animator.SetBool("IsFalling", false);
        }
    }

    public void RecordModelPosition()
    {
        worldModelPosition = transform.position + modelTransform.localPosition;
    }
    
    public void LandOnGround()
    {
        animator.SetBool("IsFalling", false);
        animator.SetBool("HasLanded", false);
    }

    public void ResetVelocity()
    {
        rb.velocity = new Vector3(0, 0, 0);
    }

    #endregion

    #region GetSet
    public void StopMove()
    {
        stopMove = true;
    }

    public void ResumeMove()
    {
        stopMove = false;
    }

    // RENVOIE LE POINT DANS LE MONDE QUE LE JOUEUR VISE
    public Vector3 GetLookAt()
    {
        return lookAtPos;
    }

    public Vector2 GetCursorPos()
    {
        return cursorPos;
    }

    // RENVOIE LA POSITION ACTUELLE DU JOUEUR
    public Vector3 GetPlayerPos()
    {
        return transform.position;
    }

    public bool GetConcentration()
    {
        return isConcentrating;
    }

    public InputMode GetInputMode()
    {
        return inputMode;
    }

    public bool GetIsRunning()
    {
        return isRunning;
    }

    // Renvoie la position du curseur sur l'écran (souris ou eye tracker) dans l'intervalle [-1;1]

    public Vector2 getCursorPosNormalized()
    {
        Vector2 normalizedCursorPos = GameManager.instance.mainCamera.ScreenToViewportPoint(cursorPos);

        normalizedCursorPos.x = Mathf.Clamp(normalizedCursorPos.x, 0.0f, 1.0f) * 2.0f - 1.0f;
        normalizedCursorPos.y = Mathf.Clamp(normalizedCursorPos.y, 0.0f, 1.0f) * 2.0f - 1.0f;

        return normalizedCursorPos;
    }

    // ACTIVE OU DESACTIVE LES CONTROLES DU JOUEUR LORSQU IL EST VIVANT OU MORT
    public void SetIsAlive(bool b)
    {
        isAlive = b;
    }

    // RENVOIE LA VALEUR VRAI OU FAUX DE SI LE JOUEUR EST VIVANT OU NON
    public bool getIsAlive()
    {
        return isAlive;
    }

    public Light getLight()
    {
        return flashlight;
    }

    public void SetCurrentAudioRoom(AudioRoom ar)
    {
        currentAudioRoom = ar;
    }

    public void SetIsInElevator(bool b)
    {
        isInElevator = b;
    }

    public void SetLightAngle(float newAngle)
    {
        normalLightAngle = newAngle;
    }

    public AudioRoom GetCurrentAudioRoom()
    {
        return currentAudioRoom;
    }

    public void SetLightRange(float newRange)
    {
        normalLightRange = newRange;
    }

    public CameraBlock getCameraBlock()
    {
        return currentCameraBlock;
    }

    public bool GetIsHidden()
    {
        return isHidden;
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }
    #endregion
}
 