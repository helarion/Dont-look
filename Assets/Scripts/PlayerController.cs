using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
using Rewired;

public class PlayerController : MonoBehaviour
{
    Light lt;
    Rigidbody rb;
    Vector3 cursorPos;

    [Header("Movement")]
    [SerializeField] float runSpeed = 3;
    [SerializeField] float walkSpeed = 1;
    [SerializeField] float jumpSpeed = 1.5f;
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

    private Player controls; // The Rewired Player

    bool isTrackerOn = false; // Is the eye tracker activated ?

    Vector3 lookAt; // Point exact où le joueur

    void Start()
    {
        controls = ReInput.players.GetPlayer(0);
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
        // LIGHT AIM CONTROL
        if (TobiiAPI.IsConnected && !disableTracker) // EYE TRACKER OPTION
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

            Vector2 gazePoint = TobiiAPI.GetGazePoint().Screen;
            cursorPos = gazePoint;

            RaycastHit hit;
            Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(cursorPos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.getWallsAndMobsLayer()))
            {
                lookAt = hit.point;
            }
        }
        else
        {
            // CHECK DU BOUTON POUR FERMER LES YEUX SI L'EYE TRACKER N'EST PAS ACTIVÉ
            if (controls.GetAxis("Light")!=0)
            {
                ClosedEyes(true);
            }
            else
            {
                ClosedEyes(false);
            }

            float xLight = controls.GetAxis("Light Horizontal");
            float yLight = controls.GetAxis("Light Vertical");

            if(Input.GetAxis("Mouse X")!=0 || Input.GetAxis("Mouse Y")!=0)
            {
                cursorPos = Input.mousePosition;
            }

            cursorPos.x += xLight * stickSpeed * 100 * Time.deltaTime;
            cursorPos.y += yLight * stickSpeed * 100 * Time.deltaTime;

            RaycastHit hit;
            Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(cursorPos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.getWallsAndMobsLayer()))
            {
                lookAt = hit.point;
            }
        }
        lightTransform.rotation = Quaternion.Slerp(lightTransform.rotation, Quaternion.LookRotation(lookAt - lightTransform.position), Time.deltaTime * lightSpeed * 100);
    }

    // CHECK LES INPUTS DE MOVEMENT
    void Movement()
    {
        float hMove = controls.GetAxis("Move Horizontal");
        float vMove = controls.GetAxis("Move Vertical");

        if (controls.GetAxisRaw("Sprint") != 0) moveSpeed = runSpeed;
        else moveSpeed = walkSpeed;

        // MOVEMENT
        if (hMove != 0)
        {
            transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
        }
        if (vMove != 0)
        {
            transform.Translate(Vector3.back* -1 * vMove * moveSpeed * Time.deltaTime);
        }
        // JUMP
        if (controls.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpSpeed * 10000 * Time.deltaTime);
        }
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
        return lookAt;
    }

    // RENVOIE LA POSITION ACTUELLE DU JOUER
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
}