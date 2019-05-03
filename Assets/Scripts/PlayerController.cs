using System.Collections;
using UnityEngine;
using Tobii.Gaming;

public class PlayerController : MonoBehaviour
{
    #region variables
    private Light lt;
    private Light camLt;
    private Rigidbody rb;
    private Collider cl;
    private Vector3 lookAtPos;
    private Vector2 cursorPos;

    [Header("Model and light objects")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Transform[] raycastPosition = null;
    [SerializeField] private Transform raycastClimb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform hipPosition;
    [SerializeField] private Transform headPosition;
    [SerializeField] private Transform armPosition;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float rayCastLength = 0.1f;
    [SerializeField] private float maxClimbHeight = 1.0f;
    [SerializeField] private float maxClimbLength = 1.0f;
    [SerializeField] private float jumpLength = 3;
    [SerializeField] private float jumpLengthSpeed = 1;
    [SerializeField] private bool isGrounded = false;
    private int jumpDirection = 0;

    [Header("Movement")]
    [SerializeField] private float walkTime;
    [SerializeField] private float runTimeMinus;
    [SerializeField] private float deadZoneValue = 0.3f;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float ladderSpeed = 0.5f;
    [SerializeField] private float grabSpeed = 0.5f;
    [SerializeField] private float changingLineSpeed = 0.1f;

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

    [Header("Light")]
    [SerializeField] private float bodyRotationDeadZone = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float lightSensitivity = 1;
    [SerializeField] private float lightSpeed = 1;
    [SerializeField] private Transform flashlight;
   // [SerializeField] private Transform cameraLight;
    [SerializeField] private Transform lamp;
    [SerializeField] private Light pointLight;
    [SerializeField] public float rangeDim; 
    public bool lightOn = true;

    [Header("State")]
    private bool isHidden = false;
    private bool isAlive = true;
    [SerializeField]private bool isClimbingLadder = false;
    private bool isClimbing = false;
    private bool isGrabbing = false;
    private bool isMoving = false;
    [SerializeField] public bool isJumping = false;
    private bool isChangingSpatialLine = false;
    private bool isTouchingBox = false;
    private bool walkRoutine = false;
    private bool StoppedHMove = false;
    private bool stopMove = false;
    private bool isFalling = false;
    private bool hasPlayedHeart = false;

    private CameraBlock currentCameraBlock = null;
    SpatialRoom currentSpatialRoom = null;
    SpatialSas currentSpatialSas = null;
    SpatialLine currentSpatialLine = null;
    private Rigidbody objectGrabbed = null;
    [Header("Debug")]
    //[SerializeField] private bool ignoreIsGroundedOneTime = false;

    private float objectGrabbedWidth = 0;

    enum LookDirection { Left, Right, Front, Back};
    LookDirection currentLookDirection = LookDirection.Right;

    enum InputMode { PC, Pad};
    InputMode inputMode = InputMode.Pad;

    #endregion

    #region StartUpdate

    private void Start()
    {
        isAlive = true;
        cl = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        lt = flashlight.GetComponent<Light>();
        //camLt = cameraLight.GetComponent<Light>();
        lt.type = LightType.Spot;
        ClosedEyes(false);
        Cursor.visible = false;
        moveSpeed = walkSpeed;

        cursorPos = Input.mousePosition;
        lastPosition = transform.position;
        localModelPosition = modelTransform.localPosition;
    }

    private void Update()
    {
        if (!isAlive || GameManager.instance.GetIsPaused() || isClimbing ) return;
        LightAim();
        GroundedCheck();
        FallingCheck();
        MoveInputUpdate();
    }

    private void FixedUpdate()
    {
        if (!isAlive || GameManager.instance.GetIsPaused() || isClimbing) return;

        velocity = (transform.position - lastPosition).magnitude;
        yVelocity = (transform.position.y - lastPosition.y);

        lastPosition = transform.position;


        if (isClimbingLadder)
        {
            if(vMove > 0 || vMove < 0) animator.SetFloat("ClimbSpeed", Mathf.Abs(vMove));
            else animator.SetFloat("ClimbSpeed", 0);
        }

        if (isClimbingLadder) return;
        ClimbCheck();
        if (isJumping || stopMove || isFalling) return;
        Move();

        BodyRotation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(lookAtPos, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.left);
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.right);
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.forward);
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.back);

        Gizmos.color = Color.green;
        if (Application.isPlaying)
        {
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
            GameManager.instance.camHandler.SetNewZ(currentCameraBlock.room.newZ);
            GameManager.instance.camHandler.SetNewOffset(currentCameraBlock.room.newOffset);
            SetLightRange(currentCameraBlock.room.newLightRange);
            return;
        }

        SpatialSas spatialSas = other.GetComponent<SpatialSas>();
        if (spatialSas != null)
        {
            currentSpatialSas = spatialSas;
            currentSpatialLine = spatialSas.spatialLine;
            isChangingSpatialLine = true;
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
                    }
                }
            }
            return;
        }

        else if(other.CompareTag("Hideout"))
        {
            isHidden = true;
            PlayHeart();
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
        else if(other.CompareTag("Grabbable"))
        {
            GrabbableBox gb = other.GetComponentInParent<GrabbableBox>();
            gb.setIsPlayerInGrabZone(true);
        }
        else if(other.CompareTag("DetectZone"))
        {
            Enemy e = other.GetComponentInParent<Enemy>();
            e.DetectPlayer(true);
        }
        else if(other.CompareTag("Finish"))
        {
            AkSoundEngine.PostEvent(GameManager.instance.ChaseAmbStop, GameManager.instance.gameObject);
            UIManager.instance.FadeInEnd();
        }
        else if (other.CompareTag("UpLadder") || other.CompareTag("DownLadder"))
        {
            if (!isClimbingLadder)
            {
                StartClimbLadder(other.transform.position);
                if (!other.GetComponentInParent<Ladder>().isReusable) other.isTrigger = false;
            }
            else StopClimbLadder();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!stopMove && !isClimbing && !isClimbingLadder && !animator.GetBool("IsJumping"))
        {
            if (other.CompareTag("JumpZoneRight") && hMove > 0)
            {
                jumpDirection = 1;
                Jump();
            }
            else if (other.CompareTag("JumpZoneLeft") && hMove < 0)
            {
                jumpDirection = 0;
                Jump();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CameraBlock>() != null)
        {
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
                    }
                }
            }
            return;
        }

        if (other.CompareTag("Hideout"))
        {
            isHidden = false;
            StopHeart();
            return;
        }

        if (other.CompareTag("Grabbable"))
        {
            GrabbableBox gb = other.GetComponentInParent<GrabbableBox>();
            gb.setIsPlayerInGrabZone(false);
            return;
        }

        if (other.CompareTag("DetectZone"))
        {
            Enemy e = other.GetComponentInParent<Enemy>();
            e.DetectPlayer(false);
            return;
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

    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetLookAtWeight(1);
        Vector3 headLookAt = lookAtPos;
        if (currentLookDirection == LookDirection.Left)
        {
            headLookAt = rotatePointAround(headLookAt, headPosition.position, Vector3.up, 90);
        }
        else
        {
            headLookAt = rotatePointAround(headLookAt, headPosition.position, Vector3.up, -90);
        }
        animator.SetLookAtPosition(headLookAt);

        /*animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        Vector3 armPos;
        if (currentLookDirection == LookDirection.Left)
        {
            armPos = hipPosition.position + new Vector3(0.05f, 1.0f, 0.1f) + Vector3.ClampMagnitude(lookAtPos - new Vector3(0.05f, 0.75f, 0.1f) - hipPosition.position, 0.25f);
            armPos = rotatePointAround(armPos, hipPosition.position, Vector3.up, -90);
        }
        else
        {
            armPos = hipPosition.position + new Vector3(-0.05f, 1.0f, -0.1f) + Vector3.ClampMagnitude(lookAtPos - new Vector3(-0.05f, 0.75f, -0.1f) - hipPosition.position, 0.25f);
            armPos = rotatePointAround(armPos, hipPosition.position, Vector3.up, 90);
        }
        animator.SetIKPosition(AvatarIKGoal.LeftHand, armPos);*/
        //animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.LookRotation(headLookAt - armPos) * Quaternion.Euler(45, -30, 0));
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

    private void LightAim()
    {
        if(!TobiiAPI.IsConnected)
        {
            UIManager.instance.DisableControlPanel(true);
        }
        else
        {
            UIManager.instance.DisableControlPanel(false);
        }
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
        /*Vector3 lightAimVec = lookAtPos - GameManager.instance.mainCamera.transform.position;
        Vector3 cameraPosBack = GameManager.instance.mainCamera.transform.position;
        cameraPosBack.z = lookAtPos.z;
        Vector3 cameraPosPlayer = GameManager.instance.mainCamera.transform.position;
        cameraPosPlayer.z = transform.position.z + 2;
        float vecRate = (cameraPosPlayer - GameManager.instance.mainCamera.transform.position).magnitude / (cameraPosBack - GameManager.instance.mainCamera.transform.position).magnitude;
        lookAtPos = GameManager.instance.mainCamera.transform.position + lightAimVec * vecRate;*/
        /* ---  --- */

        //cameraLight.rotation = Quaternion.Slerp(cameraLight.rotation, Quaternion.LookRotation(lookAtPos - cameraLight.position), Time.fixedDeltaTime * lightSpeed * 100);
        flashlight.rotation = Quaternion.Slerp(flashlight.rotation, Quaternion.LookRotation(lookAtPos - flashlight.position), Time.fixedDeltaTime * lightSpeed * 100);
    }

    // APPELER LORSQUE LE JOUEUR FERME LES YEUX
    private void ClosedEyes(bool isClosed)
    {
        //camLt.enabled = !isClosed;
        lt.enabled = !isClosed;
        //pointLight.enabled = !isClosed;
        lightOn = !isClosed;

        if(isClosed)
        {
            PlayHeart();
        }
        else if(!isHidden)
        {
            StopHeart();
        }
    }

    #endregion

    #region System
    public void Reset()
    {
        isHidden = false;
        SetIsAlive(true);
        isGrabbing = false;
        isClimbing = false;
        isClimbingLadder = false;
        isTouchingBox = false;
        animator.SetBool("OnLadder", false);
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("Climb", false);
        animator.SetBool("IsFalling", false);
        animator.SetBool("HasLanded", false);
        rb.useGravity = true;
        rb.isKinematic = false;
        StoppedHMove = false;
        stopMove = false;
        ResetVelocity();
    }
    #endregion

    #region Ladder

    public void StartClimbLadder(Vector3 v)
    {
        StoppedHMove = false;
        transform.position = v;
        modelTransform.eulerAngles = new Vector3(0, 0, 0);
        SetIsClimbingLadder(true);
        ResetVelocity();
        animator.SetBool("OnLadder", true);
    }

    public void StopClimbLadder()
    {
        animator.SetFloat("ClimbSpeed", 1);
        StoppedHMove = false;
        modelTransform.eulerAngles = new Vector3(0, 90, 0);
        SetIsClimbingLadder(false);
        animator.SetBool("OnLadder", false);
        Vector3 newPosition = transform.position;
        transform.position = newPosition;
    }

    private IEnumerator LadderPush()
    {
        float count = 0;
        while (count < 1f)
        {
            Vector3 lMovement = new Vector3(0, GameManager.instance.controls.GetAxisRaw("Move Vertical"), 0);
            if ((lMovement.y > 0) || lMovement.y < 0)
            {
                Vector3 move = transform.position + lMovement;
                transform.position = Vector3.Lerp(transform.position, move, Time.deltaTime / ladderSpeed);
                count += Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    #endregion

    #region Movement

    private void MoveInputUpdate()
    {
        hMove = GameManager.instance.controls.GetAxis("Move Horizontal");
        vMove = GameManager.instance.controls.GetAxis("Move Vertical");
    }

    private void Move()
    {
        if (isJumping) return;
        isMoving = false;
        Vector3 lMovement = Vector3.zero;
        Vector3 camMove = Vector3.zero;

        if (hMove < deadZoneValue && hMove > -deadZoneValue) hMove = 0;
        if (!isClimbingLadder && !isHidden)
        {
            if (Mathf.Abs(hMove) > 0)
                lMovement += HorizontalMove(hMove);
            else if (_horizontalAccDecLerpValue != 0)
                lMovement += HorizontalSlowDown();
        }

        if (!isGrabbing && !isClimbingLadder)
        {
            if (!isChangingSpatialLine)
            {
                if (currentSpatialSas == null)
                {
                    if (vMove > deadZoneValue)
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
                                    break;
                                }
                            }
                        }
                    }
                    else if (vMove < -deadZoneValue)
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
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        
            /*if (vMove !=0)
                lMovement += VerticalMove(vMove);
            else if (_verticalAccDecLerpValue != 0)
                lMovement += VerticalSlowDown();*/
        }

        if (lMovement != Vector3.zero || isChangingSpatialLine)
        {
            isMoving = true;
            if (isClimbingLadder) lMovement *= 0;// ladderSpeed; //moveSpeed = ladderSpeed;
            else if (GameManager.instance.controls.GetAxisRaw("Sprint") != 0 && inverse==1)
            {
                animator.SetBool("IsRunning", true);
                moveSpeed = runSpeed;
            }
            else
            {
                animator.SetBool("IsRunning", false);
                moveSpeed = walkSpeed;
            }

            if (isGrabbing)
            {
                lMovement *= grabSpeed;
            }

            if (!isClimbingLadder)
            {
                if (isHidden)
                {
                    if (currentSpatialLine.begin.position.z > transform.position.z)
                    {
                        lMovement.x = Mathf.Clamp(((currentSpatialLine.end.position.x - currentSpatialLine.begin.position.x) / 2 + currentSpatialLine.begin.position.x) - transform.position.x, -1, 1);
                        lMovement.x *= changingLineSpeed;
                    }
                    else
                    {
                        lMovement.x = 0;
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
                    lMovement.z = (transform.position.z - currentSpatialLine.begin.position.z) > 0 ? -1 : 1;
                    lMovement.z *= changingLineSpeed;
                    Vector3 transformPosition = transform.position + lMovement;
                    if (lMovement.z > 0)
                    {
                        if (transformPosition.z > currentSpatialLine.begin.position.z)
                        {
                            lMovement.z = currentSpatialLine.begin.position.z - transform.position.z;
                        }
                    }
                    else
                    {
                        if (transformPosition.z < currentSpatialLine.begin.position.z)
                        {
                            lMovement.z = currentSpatialLine.begin.position.z - transform.position.z;
                        }
                    }

                    if (Mathf.Abs(transform.position.z - currentSpatialLine.begin.position.z) < 0.001f)
                    {
                        isChangingSpatialLine = false;
                    }
                }
            }

            rb.MovePosition(transform.position + lMovement);

            if (isGrabbing)
            {
                int direction = (transform.position.x - objectGrabbed.position.x) > 0 ? -1 : 1;
                objectGrabbed.MovePosition(transform.position + new Vector3(objectGrabbedWidth * direction, 0, 0));
            }

            /*Vector3 camPos = GameManager.instance.mainCamera.transform.position;
            lMovement.z = 0;
            GameManager.instance.MoveCamera(camPos + (lMovement*50));*/
        }
        if (Mathf.Abs(lMovement.x) < 0.01f && Mathf.Abs(lMovement.z) < 0.01f)
        {
            isMoving = false;
        }
        animator.SetBool("IsMoving", isMoving);
    }

    public void PlaySoundWalk()
    {
        if (isClimbingLadder) return;
        AkSoundEngine.PostEvent("Play_Placeholder_Footsteps_Concrete_Walk", gameObject);
    }

    public void PlaySoundRun()
    {
        if (isClimbingLadder) return;
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

        if (isClimbingLadder)
        {
            lMovement = new Vector3(0, Mathf.Abs(lYmovValue), 0);
        }
        else
        {
            lMovement = new Vector3(0, 0, Mathf.Abs(lYmovValue));
        }
        
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
        if (isClimbingLadder) return;
        Quaternion save = lt.transform.rotation;
        float speed = 0.1f;
        if (vMove > 0 && isChangingSpatialLine)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 0, 0)), speed);
            inverse = 1;
            currentLookDirection = LookDirection.Front;
        }
        else if (vMove < 0 && isChangingSpatialLine)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), speed);
            inverse = -1;
            currentLookDirection = LookDirection.Back;
        }
        else
        {
            if ((lookAtPos.x - transform.position.x > -bodyRotationDeadZone) || (currentLookDirection == LookDirection.Left && lookAtPos.x - transform.position.x > bodyRotationDeadZone))
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 90, 0)), speed / 2.0f);
                currentLookDirection = LookDirection.Right;
                if (hMove > 0) inverse = 1;
                else inverse = -1;
            }
            else
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 270, 0)), speed / 2.0f);
                currentLookDirection = LookDirection.Left;
                if (hMove < 0) inverse = 1;
                else inverse = -1;
            }
        }
        lt.transform.rotation = save;
        animator.SetFloat("Inverse", inverse);
    }


    // Bouge les pieds du joueur à la position donnée
    public void moveTo(Vector3 positionToMove)
    {
        positionToMove.y += cl.bounds.center.y - cl.bounds.min.y;
        transform.position = positionToMove;
    }

    #endregion

    #region Climb

    private void ClimbCheck()
    {
        Vector3 climbDirection = Vector3.zero;
        Vector3 angle = Vector3.zero;

        if (hMove > 0)
        {
            climbDirection = Vector3.right;
            angle = new Vector3(0, 90, 0);
        }
        else if (hMove < 0)
        {
            climbDirection = Vector3.left;
            angle = new Vector3(0, -90, 0);
        }
        else if (vMove > 0)
        {
            climbDirection = Vector3.forward;
            angle = new Vector3(0, 0, 0);
        }
        else if (vMove < 0)
        {
            climbDirection = Vector3.back;
            angle = new Vector3(0, -180, 0);
        }

        if (isJumping)
        {
            if (jumpDirection == 0)
            {
                climbDirection = Vector3.left;
                angle = new Vector3(0, -90, 0);
            }
            else if (jumpDirection == 1)
            {
                climbDirection = Vector3.right;
                angle = new Vector3(0, 90, 0);
            }
        }

        //print(climbDirection);

        RaycastHit hitInfo;
        if (Physics.Raycast(raycastClimb.position, climbDirection, out hitInfo, maxClimbLength, GameManager.instance.GetClimbLayer()))
        {
            if (hitInfo.collider.bounds.max.y - cl.bounds.max.y < maxClimbHeight)
            {
                modelTransform.localEulerAngles = angle;
                isClimbing = true;
                rb.isKinematic = true;
                animator.SetBool("IsFalling", false);
                animator.SetBool("Climb", true);
            }
        }
    }

    public void StopClimb()
    {
        Vector3 newPos = hipPosition.position;
        newPos.y -= 0.3f;
        transform.position = newPos;
        modelTransform.localPosition = localModelPosition;
        lastPosition = transform.position;

        rb.isKinematic = false;
        //isAlive = true;
        isClimbing = false;
        isGrounded = true;
        animator.SetBool("Climb", false);
        animator.SetBool("IsMoving", false);
        velocity = 0;
        animator.SetBool("IsFalling", false);
        animator.SetBool("HasLanded", false);
    }

    #endregion

    #region Jump
    private void GroundedCheck()
    {
        if (!isGrounded) isFalling = true;
        isGrounded = false;
        /*if (ignoreIsGroundedOneTime)
        {
            ignoreIsGroundedOneTime = false;
            return;
        }*/
        RaycastHit hitInfo;
        foreach (Transform t in raycastPosition)
        {
            if (Physics.Raycast(t.position, -Vector3.up, out hitInfo, rayCastLength))
            {
                //print("raycasthit: "+hitInfo.collider.name);
                if (!hitInfo.collider.isTrigger)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        if (!isFalling) return;
        if (isGrounded)
        {
            isJumping = false;
            isFalling = false;
            stopMove = false;
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("HasLanded", true);
        }
    }

    private void FallingCheck()
    {
        if(!isClimbing && !isClimbingLadder)
        {
            if (yVelocity < 0)
            {
                animator.SetBool("IsFalling", true);
            }
            else
            {
                animator.SetBool("IsFalling", false);
            }
        }
    }

    public void RecordModelPosition()
    {
        worldModelPosition = transform.position + modelTransform.localPosition;
    }
    
    public void LandOnGround()
    {
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsFalling", false);
        animator.SetBool("HasLanded", false);
    }

    public void JumpStart()
    {
        isJumping = true;
        rb.velocity = Vector3.zero;
        float direction = jumpLength;
        if (jumpDirection == 0) direction *= -1;
        Vector3 jumpVector = new Vector3(direction, jumpForce);
        rb.AddForce(jumpVector, ForceMode.VelocityChange);
       // ignoreIsGroundedOneTime = true;
    }

    public void Jump()
    {
        GroundedCheck();
        if (!isGrounded) return;
        stopMove = true;
        rb.velocity = Vector3.zero;
        Vector3 angle=Vector3.zero;
        if (jumpDirection == 0)
        {
            angle = new Vector3(0, -90, 0);
        }
        else if (jumpDirection == 1)
        {
            angle = new Vector3(0, 90, 0);
        }
        modelTransform.localEulerAngles = angle;
        animator.SetTrigger("Jump");
        animator.SetBool("IsJumping",true);

    }

    public void ResetVelocity()
    {
        rb.velocity = new Vector3(0, 0, 0);
    }

    #endregion

    #region GetSet
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
        return lt;
    }

    public void SetLightRange(float newRange)
    {
        lt.range = newRange;
    }

    public void SetIsClimbingLadder(bool b)
    {
        isClimbingLadder = b;
        if (b) rb.useGravity = false;
        else rb.useGravity = true;
    }

    public bool GetIsClimbingLadder()
    {
        return isClimbingLadder;
    }

    public void SetIsGrabbing(bool b, Rigidbody obj, float objWidth)
    {
        isGrabbing = b;
        objectGrabbed = obj;
        objectGrabbedWidth = objWidth;
    }

    public bool GetIsGrabbing()
    {
        return isGrabbing;
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
 