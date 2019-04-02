using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
using Rewired;

public class PlayerController : MonoBehaviour
{
    Light lt;
    Rigidbody rb;
    Vector3 lookAtPos;
    Vector3 cursorPos;

    [Header("Movement")]
    [SerializeField] float runSpeed = 3;
    [SerializeField] float walkSpeed = 1;
    [SerializeField] float jumpForce = 1.5f;
    [SerializeField] float rayCastLength = 0.1f;

    [Header("Light")]
    [SerializeField] float sizeSpeed = 5;
    [SerializeField] float stickSpeed = 3;
    [SerializeField] float lightSpeed = 1;
    [SerializeField] Transform lightTransform;

    [Header("Debug")]
    [SerializeField] bool disableTracker = false;
    [SerializeField] Transform raycastPosition;
    [SerializeField] bool isGrounded = false;

    float moveSpeed;

    public bool lightOn = true;
    bool isAlive = true;
    bool isRunning = false;
    bool isClimbing = false;
    bool hasReachedTop = false;
    bool isTrackerOn = false; // Is the eye tracker activated ?
    bool isGrabbing = false;
    bool pressedJump = false;
    Transform objectGrabbed = null;

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        lt = lightTransform.GetComponentInChildren<Light>();
        lt.type = LightType.Spot;
        ClosedEyes(false);
        Cursor.visible = false;
        moveSpeed = walkSpeed;

        cursorPos = Input.mousePosition;
    }

    void Update()
    {
        if (!isAlive) return;
        LightAim();
        Movement();

        // TAILLE DE LA FLASHLIGHT
        float range = Input.GetAxisRaw("LightRange")*sizeSpeed*100*Time.deltaTime;
        if (range!=0)
        {
            lt.spotAngle += range;
        }
        isGrounded = Physics.Raycast(raycastPosition.position, -Vector3.up, rayCastLength);
    }


    void LightAim()
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
        if (!disableTracker && TobiiAPI.IsConnected) // EYE TRACKER OPTION
        {
            isTrackerOn = true;
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

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.getWallsAndMobsLayer()))
        {
            lookAtPos = hit.point;
        }
        lightTransform.rotation = Quaternion.Slerp(lightTransform.rotation, Quaternion.LookRotation(lookAtPos - lightTransform.position), Time.deltaTime * lightSpeed * 100);
    }

    // CHECK LES INPUTS DE MOVEMENT
    void Movement()
    {
        float hMove = GameManager.instance.controls.GetAxis("Move Horizontal");
        float vMove = GameManager.instance.controls.GetAxis("Move Vertical");

        if (GameManager.instance.controls.GetAxisRaw("Sprint") != 0) moveSpeed = runSpeed;
        else moveSpeed = walkSpeed;

        // MOVEMENT
        if (isGrabbing)
        {
            if (hMove != 0)
            {
                Vector3 translation = Vector3.right * hMove * moveSpeed * Time.deltaTime;

                transform.Translate(translation);
                objectGrabbed.position += translation*-1;
                //objectGrabbed.Translate(translation,Space.World);
            }
        }
        else if (!isClimbing)
        {
            if (hMove != 0)
            {
                transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
            }
            if (vMove != 0)
            {
                transform.Translate(Vector3.back * -1 * vMove * moveSpeed * Time.deltaTime);
            }
            // JUMP
            if (isGrounded)
            {
                if (GameManager.instance.controls.GetButtonDown("Jump")) Jump();
                else pressedJump = false;
            }
        }
        else
        {
            if (vMove < 0 || (vMove>0 && !hasReachedTop))
            {
                transform.Translate(Vector3.up * vMove * moveSpeed * Time.deltaTime );
            }
        }
        
    }

    public void Jump()
    {
        if (pressedJump) return;
        pressedJump = true;
        Vector3 jumpVector = new Vector3(0, jumpForce);
        rb.AddForce(jumpVector, ForceMode.VelocityChange);
    }

    // APPELER LORSQUE LE JOUEUR FERME LES YEUX
    private void ClosedEyes(bool isClosed)
    {
        lt.enabled = !isClosed;
        lightOn = !isClosed;
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
        isClimbing = b;
        if (b) rb.useGravity = false;
        else rb.useGravity = true;
    }

    public bool GetIsClimbing()
    {
        return isClimbing;
    }

    public void SetHasReachedTop(bool b)
    {
        hasReachedTop = b;
    }

    public void DisableTracker(bool b)
    {
        disableTracker = b;
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
}