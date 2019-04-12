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
    private Animator animator;

    [Header("Model and light objects")]
    [SerializeField] private Transform modelTransform;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float rayCastLength = 0.1f;
    [SerializeField] private float maxClimbHeight = 1.0f;
    [SerializeField] private float climbTime = 1f;

    [Header("Movement")]
    [SerializeField] private float walkTime;
    [SerializeField] private float runTimeMinus;
    [SerializeField] private float deadZoneValue = 0.3f;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float walkSpeed = 1;
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

    [Header("Light")]
    [SerializeField] private float sizeSpeed = 5;
    [SerializeField] private float stickSpeed = 3;
    [SerializeField] private float lightSpeed = 1;
    [SerializeField] private Transform flashlight;
    [SerializeField] private Transform cameraLight;
    [SerializeField] private Light pointLight;

    [Header("Debug")]
    [SerializeField] private Transform[] raycastPosition=null;
    [SerializeField] private bool isGrounded = false;

    private float moveSpeed;

    public bool lightOn = true;
    private bool isAlive = true;
    private bool isClimbingLadder = false;
    [SerializeField] private bool hasReachedTop = false;
    private bool isClimbing = false;
    private CameraBlock currentCameraBlock = null;

    private bool isGrabbing = false;
    private bool isMoving = false;
    [SerializeField]private bool pressedJump = false;
    private Rigidbody objectGrabbed = null;
    private float objectGrabbedWidth = 0;
    private bool isTouchingBox = false;

    private int inverse = 1;

    private float vMove;
    private float hMove;
    private bool walkRoutine = false;

    enum LookDirection { Left, Right};
    LookDirection currentLookDirection = LookDirection.Right;

    #endregion

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
        animator = GetComponent<Animator>();

        cursorPos = Input.mousePosition;
    }

    private void Update()
    {
        GroundedCheck();
    }

    private void FixedUpdate()
    {
        if (!isAlive || GameManager.instance.GetIsPaused()) return;
        LightAim();
        Move();
        JumpCheck();
        BodyRotation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(lookAtPos, new Vector3(0.1f, 0.1f, 0.1f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "CameraBlock")
        {
            currentCameraBlock = other.GetComponent<CameraBlock>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "CameraBlock")
        {
            currentCameraBlock = null;
        }
    }

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

    // Bouge les pieds du joueur à la position donnée
    public void moveTo(Vector3 positionToMove)
    {
        positionToMove.y += cl.bounds.center.y - cl.bounds.min.y;
        transform.position = positionToMove;
    }

    public void Reset()
    {
        SetIsAlive(false);
        SetHasReachedTop(false);
        pressedJump = false;
        isGrabbing = false;
        isClimbing = false;
        isClimbingLadder = false;
        isTouchingBox = false;
    }

    #region Movement

    private void Move()
    {
        isMoving = false;
        Vector3 lMovement = Vector3.zero;

        hMove = GameManager.instance.controls.GetAxis("Move Horizontal");
        if(!isClimbingLadder)
        {
            if (Mathf.Abs(hMove) > deadZoneValue)
                lMovement += HorizontalMove(hMove);
            else if (_horizontalAccDecLerpValue != 0)
                lMovement += HorizontalSlowDown();
        }

        vMove = GameManager.instance.controls.GetAxis("Move Vertical");
        if(!isGrabbing)
        {
            if (vMove < -deadZoneValue || (vMove>deadZoneValue && !hasReachedTop &&isClimbingLadder) || (vMove>deadZoneValue && !isClimbingLadder))
                lMovement += VerticalMove(vMove);
            else if (_verticalAccDecLerpValue != 0)
                lMovement += VerticalSlowDown();
        }

        Vector3 lPoint;
        if (isClimbingLadder)
        {
           lPoint = new Vector3(transform.position.x + lMovement.x, transform.position.y + lMovement.y,0);
        }
        else
        {
            lPoint = new Vector3(transform.position.x + lMovement.x, 0, transform.position.z + lMovement.y);
        }

        if (lMovement != Vector3.zero)
        {
            isMoving = true;
            if (GameManager.instance.controls.GetAxisRaw("Sprint") != 0)
            {
                animator.SetBool("IsRunning", true);
                moveSpeed = runSpeed;
            }
            else
            {
                animator.SetBool("IsRunning", false);
                moveSpeed = walkSpeed;
            }

            if(!isGrounded) moveSpeed = runSpeed;

            if (isGrabbing)
            {
                lMovement *= 0.25f;
                int direction = (transform.position.x - objectGrabbed.position.x) > 0 ? -1 : 1;
                objectGrabbed.MovePosition(transform.position + lMovement + new Vector3(objectGrabbedWidth * direction, 0, 0));
            }
            rb.MovePosition(transform.position + lMovement);
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
        Quaternion save = lt.transform.rotation;
        float speed = 0.1f;
        if (vMove > deadZoneValue)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 0, 0)), speed);
            inverse = 1;
        }
        else if (vMove < -deadZoneValue)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), speed);
            inverse = -1;
        }
        else
        {
            if ((lookAtPos.x - transform.position.x < -0.25f) || (currentLookDirection == LookDirection.Right && lookAtPos.x - transform.position.x < 0.25f))
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 270, 0)), speed / 2.0f);
                inverse = 1;
                currentLookDirection = LookDirection.Right;
            }
            else
            {
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 90, 0)), speed / 2.0f);
                inverse = -1;
                currentLookDirection = LookDirection.Left;
            }
        }
        lt.transform.rotation = save;
    }

    #endregion

    #region Jump
    private void GroundedCheck()
    {
        bool temp = false;
        foreach(Transform t in raycastPosition)
        {
            if(Physics.Raycast(t.position, -Vector3.up, rayCastLength)) temp=true;
        }
        isGrounded = temp;
    }

    private void FallingCheck()
    {
        //pressedJump = false;
    }

    private void JumpCheck()
    {
        // JUMP
        if (GameManager.instance.controls.GetButtonDown("Jump"))
        {
            if (isGrounded && !pressedJump)
            {
                pressedJump = true;
                Jump();
            }
        }
    }
    
    public void JumpLand()
    {
        animator.SetBool("IsJumping", false);
    }

    public void JumpStart()
    {
        Vector3 jumpVector = new Vector3(0, jumpForce);
        rb.AddForce(jumpVector, ForceMode.VelocityChange);
        pressedJump = false;
    }

    public void Jump()
    {
        //print("pressed?" + pressedJump);
        animator.SetTrigger("Jump");
        animator.SetBool("IsJumping",true);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Climbable")
        {
            if (cl.bounds.min.y > -0.25f && cl.bounds.min.y - collision.collider.bounds.max.y < -0.25f && cl.bounds.min.y - collision.collider.bounds.max.y > -maxClimbHeight && !isClimbing && !isGrounded)
            {
                Vector3 newPosition = transform.position + 0.5f * (collision.transform.position - transform.position);
                newPosition.y = collision.collider.bounds.max.y + cl.bounds.center.y - cl.bounds.min.y;
                Vector3[] positions = new Vector3[2];
                positions[0] = transform.position;
                positions[1] = newPosition;
                isAlive = false;
                isClimbing = true;
                rb.isKinematic = true;
                StartCoroutine("ClimbCoroutine", positions);
            }
        }
    }

    IEnumerator ClimbCoroutine(Vector3[] positions)
    {
        float currentClimbTime = 0.0f;
        while (currentClimbTime < climbTime)
        {
            transform.position = Vector3.Lerp(positions[0], positions[1], currentClimbTime/climbTime);
            currentClimbTime += Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        isAlive = true;
        isClimbing = false;
        rb.isKinematic = false;
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

    public bool GetIsMoving()
    {
        return isMoving;
    }
    #endregion
}