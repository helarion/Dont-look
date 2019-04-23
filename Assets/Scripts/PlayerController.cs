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

    [Header("Jump")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float rayCastLength = 0.1f;
    [SerializeField] private float maxClimbHeight = 1.0f;
    [SerializeField] private float maxClimbLength = 1.0f;
    [SerializeField] private float jumpLength = 3;
    [SerializeField] private float jumpLengthSpeed = 1;
    private bool isGrounded = false;
    private int jumpDirection = 0;

    [Header("Movement")]
    [SerializeField] private float walkTime;
    [SerializeField] private float runTimeMinus;
    [SerializeField] private float deadZoneValue = 0.3f;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float ladderSpeed = 0.5f;
    [SerializeField] private float grabSpeed = 0.5f;

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
    [SerializeField] private float sizeSpeed = 5;
    [SerializeField] private float stickSpeed = 3;
    [SerializeField] private float lightSpeed = 1;
    [SerializeField] private Transform flashlight;
    [SerializeField] private Transform cameraLight;
    [SerializeField] private Light pointLight;
    [SerializeField] public float rangeDim; 
    public bool lightOn = true;

    [Header("State")]
    private bool isHidden = false;
    private bool isAlive = true;
    private bool isClimbingLadder = false;
    [SerializeField] private bool hasReachedTop = false;
    private bool hasReachedBottom = false;
    private bool isClimbing = false;
    private bool isGrabbing = false;
    private bool isMoving = false;
    public bool isJumping = false;

    [Header("Debug")]
    [SerializeField] private bool ignoreIsGroundedOneTime = false;

    private CameraBlock currentCameraBlock = null;

    SpatialRoom currentSpatialRoom = null;

    private Rigidbody objectGrabbed = null;
    private float objectGrabbedWidth = 0;
    private bool isTouchingBox = false;

    private bool walkRoutine = false;
    private bool StoppedHMove = false;

    enum LookDirection { Left, Right, Front, Back};
    LookDirection currentLookDirection = LookDirection.Right;

    #endregion

    #region StartUpdateOn

    private void Start()
    {
        isAlive = true;
        cl = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        lt = flashlight.GetComponent<Light>();
        camLt = cameraLight.GetComponent<Light>();
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
        JumpLadderHandler();
    }

    private void FixedUpdate()
    {
        if (!isAlive || GameManager.instance.GetIsPaused() || isClimbing) return;

        velocity = (transform.position - lastPosition).magnitude;
        yVelocity = (transform.position.y - lastPosition.y);

        lastPosition = transform.position;

        if (isJumping) return;
        Move();
        ClimbCheck();
        BodyRotation();
        if (!isClimbingLadder) return;
        if ((!hasReachedTop && vMove > 0) || !hasReachedBottom && vMove < 0) animator.SetFloat("ClimbSpeed", Mathf.Abs(vMove));
        else animator.SetFloat("ClimbSpeed", 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(lookAtPos, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.left);
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.right);
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.forward);
        Gizmos.DrawLine(raycastClimb.position, raycastClimb.position + Vector3.back);
    }

    private void OnTriggerEnter(Collider other)
    {
        CameraBlock cameraBlock = other.GetComponent<CameraBlock>();
        if (cameraBlock != null)
        {
            currentCameraBlock = cameraBlock;
            GameManager.instance.mainCamera.GetComponent<CameraHandler>().SetNewZ(currentCameraBlock.newZ);
            return;
        }

        SpatialRoom spatialRoom = other.GetComponent<SpatialRoom>();
        if (spatialRoom != null)
        {
            currentSpatialRoom = spatialRoom;
            return;
        }

        else if(other.CompareTag("Hideout"))
        {
            isHidden = true;
        }
        else if (other.CompareTag("Killzone"))
        {
            GameManager.instance.Death();
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
        else if (other.CompareTag("JumpZone"))
        {

            if (other.transform.position.x < transform.position.x) jumpDirection = 1;
            else if (other.transform.position.x > transform.position.x) jumpDirection = -1;
            else jumpDirection = 0;
            //print("direction:" + jumpDirection);
            Jump();
        }
        else if(other.CompareTag("UpLadder") || other.CompareTag("DownLadder"))
        {
            if (!isClimbingLadder)
            {
                StartClimbLadder(other.transform.position);
                if (!other.GetComponentInParent<Ladder>().isReusable) Destroy(other.gameObject);
            }
            else StopClimbLadder(1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CameraBlock>() != null)
        {
            currentCameraBlock = null;
        }
        else if (other.CompareTag("Hideout"))
        {
            isHidden = false;
        }
        else if (other.CompareTag("Grabbable"))
        {
            GrabbableBox gb = other.GetComponentInParent<GrabbableBox>();
            gb.setIsPlayerInGrabZone(false);
        }
        else if (other.CompareTag("DetectZone"))
        {
            Enemy e = other.GetComponentInParent<Enemy>();
            e.DetectPlayer(false);
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
            if (GameManager.instance.controls.GetAxis("Light")!=0)
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
            }

            cursorPos.x += xLight * stickSpeed * 100 * Time.fixedDeltaTime;
            cursorPos.y += yLight * stickSpeed * 100 * Time.fixedDeltaTime;
        }
        RaycastHit hit;
        Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(cursorPos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetLookLayer()))
        {
            lookAtPos = hit.point;
        }
        cameraLight.rotation = Quaternion.Slerp(cameraLight.rotation, Quaternion.LookRotation(lookAtPos - cameraLight.position), Time.fixedDeltaTime * lightSpeed * 100);
        flashlight.rotation = Quaternion.Slerp(flashlight.rotation, Quaternion.LookRotation(lookAtPos - flashlight.position), Time.fixedDeltaTime * lightSpeed * 100);
    }

    // APPELER LORSQUE LE JOUEUR FERME LES YEUX
    private void ClosedEyes(bool isClosed)
    {
        camLt.enabled = !isClosed;
        lt.enabled = !isClosed;
        pointLight.enabled = !isClosed;
        lightOn = !isClosed;
    }

    #endregion

    #region System
    public void Reset()
    {
        isHidden = false;
        SetIsAlive(false);
        SetHasReachedTop(false);
        SetHasReachedBottom(false);
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
        rb.useGravity = true;
        rb.isKinematic = false;
        StoppedHMove = false;
        ResetVelocity();
    }
    #endregion

    #region Ladder

    private void JumpLadderHandler()
    {
        if (!isClimbingLadder || (!hasReachedBottom && !hasReachedTop)) return;
        if (hMove !=0)
        {
            if (!StoppedHMove) return;
            int direction=0;
            if (hMove > 0) direction = 1;
            else if(hMove < 0) direction = -1;
            StopClimbLadder(direction);
            print(direction);
        }
        else
        {
            StoppedHMove = true;
        }
    }

    public void StartClimbLadder(Vector3 v)
    {
        StoppedHMove = false;
        transform.position = v;
        modelTransform.eulerAngles = new Vector3(0, 0, 0);
        SetIsClimbing(true);
        ResetVelocity();
        animator.SetBool("OnLadder", true);
    }

    public void StopClimbLadder(int direction)
    {
        animator.SetFloat("ClimbSpeed", 1);
        StoppedHMove = false;
        modelTransform.eulerAngles = new Vector3(0, 90, 0);
        SetIsClimbing(false);
        SetHasReachedTop(false);
        SetHasReachedBottom(false);
        animator.SetBool("OnLadder", false);
        Vector3 newPosition = transform.position;
        newPosition.z -= 3;
        newPosition.x += 2*direction;
        transform.position = newPosition;
    }

    private IEnumerator LadderPush()
    {
        float count = 0;
        while (count < 1f)
        {
            Vector3 lMovement = new Vector3(0, GameManager.instance.controls.GetAxisRaw("Move Vertical"), 0);
            if ((lMovement.y > 0 && !hasReachedTop) || lMovement.y < 0 && !hasReachedBottom)
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

    private void Move()
    {
        isMoving = false;
        Vector3 lMovement = Vector3.zero;
        Vector3 camMove = Vector3.zero;

        hMove = GameManager.instance.controls.GetAxis("Move Horizontal");
        if (hMove < deadZoneValue && hMove > -deadZoneValue) hMove = 0;
        if(!isClimbingLadder)
        {
            if (Mathf.Abs(hMove) > 0)
                lMovement += HorizontalMove(hMove);
            else if (_horizontalAccDecLerpValue != 0)
                lMovement += HorizontalSlowDown();
        }

        vMove = GameManager.instance.controls.GetAxis("Move Vertical");
        if (vMove < deadZoneValue && vMove > -deadZoneValue) vMove = 0;
        if (!isGrabbing)
        {
            if (vMove !=0)
                lMovement += VerticalMove(vMove);
            else if (_verticalAccDecLerpValue != 0)
                lMovement += VerticalSlowDown();
        }

        /*Vector3 lPoint;
        if (isClimbingLadder)
        {
           lPoint = new Vector3(transform.position.x + lMovement.x, transform.position.y + lMovement.y,0);
        }
        else
        {
            lPoint = new Vector3(transform.position.x + lMovement.x, 0, transform.position.z + lMovement.y);
        }*/

        if (lMovement != Vector3.zero)
        {
            isMoving = true;
            if (isClimbingLadder) lMovement *= 0;// ladderSpeed; //moveSpeed = ladderSpeed;
            else if (GameManager.instance.controls.GetAxisRaw("Sprint") != 0)
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
                int direction = (transform.position.x - objectGrabbed.position.x) > 0 ? -1 : 1;
                objectGrabbed.MovePosition(transform.position + lMovement + new Vector3(objectGrabbedWidth * direction, 0, 0));
            }
            rb.MovePosition(transform.position + lMovement);
            /*Vector3 camPos = GameManager.instance.mainCamera.transform.position;
            lMovement.z = 0;
            GameManager.instance.MoveCamera(camPos + (lMovement*50));*/
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
        if (vMove > 0)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 0, 0)), speed);
            inverse = 1;
            currentLookDirection = LookDirection.Front;
        }
        else if (vMove < 0)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), speed);
            inverse = -1;
            currentLookDirection = LookDirection.Back;
        }
        else
        {
            if ((lookAtPos.x - transform.position.x < -0.25f) || (currentLookDirection == LookDirection.Left && lookAtPos.x - transform.position.x < 0.25f))
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 270, 0)), speed / 2.0f);
                currentLookDirection = LookDirection.Right;
                if (hMove > 0) inverse = -1;
                else inverse = 1;
            }
            else
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 90, 0)), speed / 2.0f);
                currentLookDirection = LookDirection.Left;
                if (hMove < 0) inverse = -1;
                else inverse = 1;
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

    #region Jump
    private void GroundedCheck()
    {
        isGrounded = false;
        if (ignoreIsGroundedOneTime)
        {
            ignoreIsGroundedOneTime = false;
            return;
        }
        RaycastHit hitInfo;
        foreach (Transform t in raycastPosition)
        {
            if (Physics.Raycast(t.position, -Vector3.up, out hitInfo, rayCastLength))
            {
                if (!hitInfo.collider.isTrigger)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        if (isGrounded)
        {
            isJumping = false;
            FallingCheck();
            if (animator.GetBool("IsFalling"))
            {
                animator.SetBool("IsFalling", false);
                animator.SetBool("HasLanded", true);
            }
        }
    }

    private void FallingCheck()
    {
        if(yVelocity<0 && !isClimbing && !isClimbingLadder)
        {
            animator.SetBool("IsFalling", true);
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
        Vector3 jumpVector = new Vector3(jumpLength, jumpForce);
        rb.AddForce(jumpVector, ForceMode.VelocityChange);
        ignoreIsGroundedOneTime = true;
    }

    public void Jump()
    {
        GroundedCheck();
        if (!isGrounded) return;
        rb.velocity = Vector3.zero;
        animator.SetTrigger("Jump");
        animator.SetBool("IsJumping",true);
    }

    private void ClimbCheck()
    {
        Vector3 climbDirection = Vector3.zero;
        /*
        if (currentLookDirection == LookDirection.Left)
        {
            climbDirection = Vector3.left;
        }
        else if (currentLookDirection == LookDirection.Right)
        {
            climbDirection = Vector3.right;
        }
        else if (currentLookDirection == LookDirection.Front)
        {
            climbDirection = Vector3.forward;
        }
        else
        {
            climbDirection = Vector3.back;
        }
        */

        Vector3 angle=Vector3.zero;

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

        RaycastHit hitInfo;
        if(Physics.Raycast(raycastClimb.position, climbDirection, out hitInfo, maxClimbLength, GameManager.instance.GetClimbLayer()))
        {
            if (hitInfo.collider.bounds.max.y - cl.bounds.max.y < maxClimbHeight)
            {
                modelTransform.localEulerAngles = angle;
                //modelTransform.localEulerAngles = climbDirection;
                /*Vector3 newPosition = transform.position;// + Vector3.ClampMagnitude(hitInfo.transform.position - transform.position, hitInfo.collider.bounds.size.x / 2);
                newPosition.y = hitInfo.collider.bounds.max.y + cl.bounds.center.y - cl.bounds.min.y;
                newPosition.y += 5;
                Vector3[] positions = new Vector3[2];
                positions[0] = transform.position;
                positions[1] = newPosition;
                climbPosition = newPosition;
                isAlive = false;*/
                isClimbing = true;
                rb.isKinematic = true;
                animator.SetBool("Climb", true);
            }
        }
    }

    public void StopClimb()
    {
        transform.position = hipPosition.position;
        modelTransform.localPosition = localModelPosition;

        rb.isKinematic = false;
        //isAlive = true;
        isClimbing = false;
        animator.SetBool("Climb", false);
        velocity = 0;
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

    public void SetIsClimbing(bool b)
    {
        isClimbingLadder = b;
        if (b) rb.useGravity = false;
        else rb.useGravity = true;
    }

    public bool GetIsClimbingLadder()
    {
        return isClimbingLadder;
    }

    public void SetHasReachedTop(bool b)
    {
        hasReachedTop = b;
    }

    public void SetHasReachedBottom(bool b)
    {
        hasReachedBottom = b;
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
 