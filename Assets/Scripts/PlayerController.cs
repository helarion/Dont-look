using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class PlayerController : MonoBehaviour
{
    Light lt;
    Rigidbody rb;
    Vector3 cursorPos;
    Transform lightTransform;

    [SerializeField] float moveSpeed = 3;
    [SerializeField] float jumpSpeed = 1.5f;
    [SerializeField] float sizeSpeed = 5;
    [SerializeField] float stickSpeed = 3;
    [SerializeField] float lightSpeed = 1;
    [SerializeField] bool controllMouse = false;
    [SerializeField] bool controllEye = true;
    [SerializeField] bool controllGamePad = false;

    public bool lightOn = true;
    bool isAlive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lt = GetComponentInChildren<Light>();
        lightTransform = lt.transform;
        lt.type = LightType.Spot;
        LightEnabled(true);
        Cursor.visible = false;

        cursorPos = Input.mousePosition;
    }

    void Update()
    {
        if (!isAlive) return;
        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        // LIGHT AIM CONTROL
        if (controllEye) // EYE TRACKER OPTION
        {
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
            cursorPos.z = 7;

            Vector3 direction = cursorPos - lightTransform.position;

            lightTransform.rotation = Quaternion.RotateTowards(lightTransform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * lightSpeed *100);
        }
        else
        {
            // CHECK DU BOUTON POUR FERMER LES YEUX SI L'EYE TRACKER N'EST PAS ACTIVÉ
            if (Input.GetAxisRaw("CloseEyes") != 0)
            {
                ClosedEyes(true);
            }
            else
            {
                ClosedEyes(false);
            }

            // MOUSE OPTION
            if (controllMouse)
            {
                float xMouse = Input.GetAxis("Mouse X");
                float yMouse = Input.GetAxis("Mouse Y");

                cursorPos = Input.mousePosition;
                cursorPos.z = 7;
                cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
                cursorPos.z = 7;
                lightTransform.LookAt(cursorPos);
            }
            // GAMEPADE OPTION
            else if (controllGamePad)
            {
                cursorPos.z = 7;
                cursorPos.x += Input.GetAxis("Stick X") * stickSpeed * 100 * Time.deltaTime;
                cursorPos.y += Input.GetAxis("Stick Y") * stickSpeed *100 * Time.deltaTime;
                lightTransform.LookAt(cursorPos);
            }
        }

        // MOVEMENT
        if (hMove != 0)
        {
            transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
        }
        if(vMove !=0)
        {
            transform.Translate(Vector3.back * vMove * moveSpeed * Time.deltaTime);
        }
        // JUMP
        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpSpeed * 10000 * Time.deltaTime);
        }

        // TAILLE DE LA FLASHLIGHT
        float range = Input.GetAxisRaw("LightRange")*sizeSpeed*100*Time.deltaTime;
        if (range!=0)
        {
            lt.spotAngle += range;
        }

        if (Input.GetKeyDown(KeyCode.F1)) SwitchControl(1);
        else if (Input.GetKeyDown(KeyCode.F2)) SwitchControl(2);
        else if (Input.GetKeyDown(KeyCode.F3)) SwitchControl(3);

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

    public void SwitchControl(int num)
    {
        switch(num)
        {
            case 1:
                print("Eye tracker activated");
                controllEye = true;
                controllMouse = false;
                controllGamePad = false;
                break;
            case 2:
                print("Mouse activated");
                controllEye = false;
                controllMouse = true;
                controllGamePad = false;
                break;
            case 3:
                print("GamePad activated");
                controllEye = false;
                controllMouse = false;
                controllGamePad = true;
                break;
        }
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