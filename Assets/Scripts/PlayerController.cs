using System.Collections;
using UnityEngine;
using Tobii.Gaming;

public class PlayerController : MonoBehaviour
{
    private Light lt;
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
    [SerializeField] private Transform lightTransform=null;

    [Header("Debug")]
    [SerializeField] private Transform raycastPosition=null;
    [SerializeField] private bool isGrounded = false;

    private float moveSpeed;

    public bool lightOn = true;
    private bool isAlive = true;
    private bool isClimbingLadder = false;
    private bool hasReachedTop = false;
    private bool isClimbing = false;
    private CameraBlock currentCameraBlock = null;

    private bool isGrabbing = false;
    private bool isMoving = false;
    private bool pressedJump = false;
    private Transform objectGrabbed = null;

    private int inverse = 1;

    private float vMove;
    private float hMove;

    enum LookDirection { Left, Right};
    LookDirection currentLookDirection = LookDirection.Right;

    private void Start()
    {
        isAlive = true;
        cl = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        lt = lightTransform.GetComponentInChildren<Light>();
        lt.type = LightType.Spot;
        ClosedEyes(false);
        Cursor.visible = false;
        moveSpeed = walkSpeed;
        animator = GetComponentInChildren<Animator>();

        cursorPos = Input.mousePosition;
    }

    private void Update()
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

            cursorPos.x += xLight * stickSpeed * 100 * Time.deltaTime;
            cursorPos.y += yLight * stickSpeed * 100 * Time.deltaTime;
        }
        RaycastHit hit;
        Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(cursorPos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetWallsAndMobsLayer()))
        {
            lookAtPos = hit.point;
        }
        lt.transform.rotation = Quaternion.Slerp(lt.transform.rotation, Quaternion.LookRotation(lookAtPos - lt.transform.position), Time.deltaTime * lightSpeed * 100);
    }

    // APPELER LORSQUE LE JOUEUR FERME LES YEUX
    private void ClosedEyes(bool isClosed)
    {
        lt.enabled = !isClosed;
        lightOn = !isClosed;
    }

    // Bouge les pieds du joueur à la position donnée

    public void moveTo(Vector3 positionToMove)
    {
        positionToMove.y += cl.bounds.center.y - cl.bounds.min.y;
        transform.position = positionToMove;
    }

    #region Movement

    void Move()
    {
        isMoving = false;
        Vector3 lMovement = Vector3.zero;

        if (GameManager.instance.controls.GetAxisRaw("Sprint") != 0)
        {
            moveSpeed = runSpeed;
            animator.speed = 1.8f;
        }
        else
        {
            moveSpeed = walkSpeed;
            animator.speed = 1;
        }


        hMove = GameManager.instance.controls.GetAxis("Move Horizontal");
        if(!isClimbingLadder)
        {
            if (hMove != 0)
                lMovement += HorizontalMove(hMove);
            else if (_horizontalAccDecLerpValue != 0)
                lMovement += HorizontalSlowDown();
        }

        vMove = GameManager.instance.controls.GetAxis("Move Vertical");
        if(!isGrabbing)
        {
            if (vMove != 0)
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
            rb.MovePosition(transform.position+lMovement * Time.deltaTime);
            if(isGrabbing)
            {
                objectGrabbed.position += lMovement * Time.deltaTime;
            }
        }
        animator.SetBool("IsMoving", isMoving);
    }

    Vector3 HorizontalMove(float lXmovValue)
    {
        _horizontalAccDecLerpValue += Time.deltaTime * _horizontalAccSpeed * Mathf.Sign(lXmovValue);
        _horizontalAccDecLerpValue = Mathf.Clamp(_horizontalAccDecLerpValue, -1, 1);

        Vector3 lMovement = new Vector3(Mathf.Abs(lXmovValue), 0, 0);
        lMovement = lMovement.normalized * moveSpeed * Time.deltaTime * Mathf.Sign(_horizontalAccDecLerpValue);

        _horizontalLastMovement = lMovement;

        lMovement *= _horizontalAccelerationCurve.Evaluate(Mathf.Abs(_horizontalAccDecLerpValue));

        return lMovement;
    }

    Vector3 VerticalMove(float lYmovValue)
    {
        _verticalAccDecLerpValue += Time.deltaTime * _verticalAccSpeed * Mathf.Sign(lYmovValue);
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
        
        lMovement = lMovement.normalized * moveSpeed * Time.deltaTime * Mathf.Sign(_verticalAccDecLerpValue);

        _verticalLastMovement = lMovement;

        lMovement *= _verticalAccelerationCurve.Evaluate(Mathf.Abs(_verticalAccDecLerpValue));

        return lMovement;
    }

    Vector3 HorizontalSlowDown()
    {
        float pastLerp = _horizontalAccDecLerpValue;
        _horizontalAccDecLerpValue -= Time.deltaTime * _horizontalDecSpeed * Mathf.Sign(_horizontalAccDecLerpValue);
        if (Mathf.Sign(pastLerp) != Mathf.Sign(_horizontalAccDecLerpValue))
        {
            _horizontalAccDecLerpValue = 0;
        }

        Vector3 lMovement = _horizontalLastMovement;
        lMovement *= _horizontalDecelerationCurve.Evaluate(Mathf.Abs(_horizontalAccDecLerpValue));
        //print("SLOW = " + lMovement.x + " lastMovement = " + _horizontalLastMovement.x + " and lerp = " + _horizontalDecelerationCurve.Evaluate(Mathf.Abs(_horizontalAccDecLerpValue)) + " lerp = " + _horizontalAccDecLerpValue);

        return lMovement;
    }

    Vector3 VerticalSlowDown()
    {
        float pastLerp = _verticalAccDecLerpValue;
        _verticalAccDecLerpValue -= Time.deltaTime * _verticalDecSpeed * Mathf.Sign(_verticalAccDecLerpValue);
        if (Mathf.Sign(pastLerp) != Mathf.Sign(_verticalAccDecLerpValue))
        {
            _verticalAccDecLerpValue = 0;
        }

        Vector3 lMovement = _verticalLastMovement;
        lMovement *= _verticalDecelerationCurve.Evaluate(Mathf.Abs(_verticalAccDecLerpValue));

        return lMovement;
    }

    void BodyRotation()
    {
        Quaternion save = lt.transform.rotation;
        float speed = 0.1f;
        if (vMove > 0)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), speed);
            inverse = 1;
        }
        else if (vMove < 0)
        {
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, Quaternion.Euler(new Vector3(0, 0, 0)), speed);
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
    void JumpCheck()
    {
        isGrounded = Physics.Raycast(raycastPosition.position, -Vector3.up, rayCastLength);
        // JUMP
        if (isGrounded)
        {
            if (GameManager.instance.controls.GetButtonDown("Jump")) Jump();
            else pressedJump = false;
        }
    }
    
    public void Jump()
    {
        if (pressedJump) return;
        pressedJump = true;
        Vector3 jumpVector = new Vector3(0, jumpForce);
        rb.AddForce(jumpVector, ForceMode.VelocityChange);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Climbable")
        {
            if (cl.bounds.min.y > -0.25f && cl.bounds.min.y - collision.collider.bounds.max.y < -0.25f && cl.bounds.min.y - collision.collider.bounds.max.y > -maxClimbHeight && !isClimbing)
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
            currentClimbTime += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
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

    // Renvoie la position du curseur sur l'écran (souris ou eye tracker)

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

    public bool GetIsClimbing()
    {
        return isClimbingLadder;
    }

    public void SetHasReachedTop(bool b)
    {
        hasReachedTop = b;
    }

    public void SetIsGrabbing(bool b, Transform obj)
    {
        isGrabbing = b;
        objectGrabbed = obj;
    }

    public bool GetIsGrabbing()
    {
        return isGrabbing;
    }

    public CameraBlock getCameraBlock()
    {
        return currentCameraBlock;
    }
    #endregion
}