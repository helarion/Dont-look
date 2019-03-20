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
    Transform lightTransform;

    [SerializeField] float runSpeed = 3;
    [SerializeField] float walkSpeed = 1;
    [SerializeField] float jumpSpeed = 1.5f;
    [SerializeField] float sizeSpeed = 5;
    [SerializeField] float stickSpeed = 3;
    [SerializeField] float lightSpeed = 1;

    float moveSpeed;

    public bool lightOn = true;
    bool isAlive = true;
    bool isRunning = false;

    private Player controls; // The Rewired Player

    bool isTrackerOn = false;

    void Start()
    {
        controls = ReInput.players.GetPlayer(0);
        rb = GetComponent<Rigidbody>();
        lt = GetComponentInChildren<Light>();
        lightTransform = lt.transform;
        lt.type = LightType.Spot;
        LightEnabled(true);
        Cursor.visible = false;
        moveSpeed = walkSpeed;

        cursorPos = Input.mousePosition;
        cursorPos.z = 7;
        cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
        cursorPos.z = 7;
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
    }

    void LightAim()
    {
        // LIGHT AIM CONTROL
        if (TobiiAPI.IsConnected) // EYE TRACKER OPTION
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
            cursorPos.z = 7;
            cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
            cursorPos.z = transform.position.z+3;

            Vector3 direction = cursorPos - lightTransform.position;

            lightTransform.rotation = Quaternion.RotateTowards(lightTransform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * lightSpeed * 100);
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
                cursorPos.z = 7;
                cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
                cursorPos.z = 7;
            }

            cursorPos.z = 7;
            cursorPos.x += xLight * stickSpeed * 100 * Time.deltaTime;
            cursorPos.y += yLight * stickSpeed * 100 * Time.deltaTime;
            lightTransform.LookAt(cursorPos);

        }
    }

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
        if (controls.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpSpeed * 10000 * Time.deltaTime);
        }
    }

    private void ClosedEyes(bool isClosed)
    {
        LightEnabled(!isClosed);
    }

    public void LightEnabled(bool isEnabled)
    {
        lt.enabled = isEnabled;
        lightOn = isEnabled;
    }

    public Vector3 GetCursorPos()
    {
        return cursorPos;
    }

    public float GetLightSpeed()
    {
        return lightSpeed;
    }

    public Vector3 GetPlayerPos()
    {
        return transform.position;
    }

    public void SetIsAlive(bool b)
    {
        isAlive = b;
    }
}