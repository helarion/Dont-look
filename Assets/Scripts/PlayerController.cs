using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
using Aura2API;

public class PlayerController : MonoBehaviour
{
    Light lt;
    AuraLight al;
    Rigidbody rb;
    Vector3 cursorPos;
    Transform lightTransform;

    private bool _hasHistoricPoint;
    private Vector3 _historicPoint;
    [Range(0.1f, 1.0f), Tooltip("How heavy filtering to apply to gaze point bubble movements. 0.1f is most responsive, 1.0f is least responsive.")]
    public float FilterSmoothingFactor = 0.15f;

    [SerializeField] float moveSpeed = 3;
    [SerializeField] float jumpSpeed = 1.5f;
    [SerializeField] float sizeSpeed = 5;
    [SerializeField] float stickSpeed = 3;
    [SerializeField] bool controllMouse = false;
    [SerializeField] bool controllEye = true;
    [SerializeField] bool controllGamePad = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lt = GetComponentInChildren<Light>();
        lightTransform = lt.transform;
        al = GetComponentInChildren<AuraLight>();
        lt.type = LightType.Spot;
        LightEnabled(true);
        Cursor.visible = false;

        cursorPos = TobiiAPI.GetGazePoint().Screen;
        if (!float.IsNaN(cursorPos.x))
        {
            cursorPos.z = 7;
            cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
            cursorPos.z = 7;
        }
        else
        {
            SwitchControl(2);
            cursorPos = Vector3.zero;
        }
        
    }

    void Update()
    {
        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        // MOVEMENT
        if (hMove!=0)
        {
            transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
        }
        // JUMP
        if (Input.GetButtonDown("Jump"))
        {
            print("test");
            rb.AddForce(Vector3.up * jumpSpeed *10000 * Time.deltaTime);
        }

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

            Vector2 filteredPoint;
            Vector2 gazePoint = TobiiAPI.GetGazePoint().Screen;
            filteredPoint = Smoothify(gazePoint);
            cursorPos = gazePoint;
            cursorPos.z = 7;
            cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
            cursorPos.z = 7;
            lightTransform.LookAt(cursorPos);
        }
        else
        {
            // CHECK DU BOUTON POUR FERMER LES YEUX SI L'EYE TRACKER N'EST PAS ACTIVÉ
            if (Input.GetButton("CloseEyes")) ClosedEyes(true);
            else ClosedEyes(false);

            // MOUSE OPTION
            if (controllMouse)
            {
                float xMouse = Input.GetAxis("Mouse X");
                float yMouse = Input.GetAxis("Mouse Y");

                if (xMouse != 0 || yMouse != 0)
                {
                    cursorPos = Input.mousePosition;
                    cursorPos.z = 7;
                    cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
                    cursorPos.z = 7;
                    lightTransform.LookAt(cursorPos);
                }
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

    private Vector3 Smoothify(Vector3 point)
    {
        if (!_hasHistoricPoint)
        {
            _historicPoint = point;
            _hasHistoricPoint = true;
        }

        var smoothedPoint = new Vector3(
            point.x * (1.0f - FilterSmoothingFactor) + _historicPoint.x * FilterSmoothingFactor,
            point.y * (1.0f - FilterSmoothingFactor) + _historicPoint.y * FilterSmoothingFactor,
            point.z * (1.0f - FilterSmoothingFactor) + _historicPoint.z * FilterSmoothingFactor);

        _historicPoint = smoothedPoint;

        return smoothedPoint;
    }

    private void ClosedEyes(bool isClosed)
    {
        LightEnabled(!isClosed);
    }

    public void LightEnabled(bool isEnabled)
    {
        lt.enabled = isEnabled;
        al.enabled = isEnabled;
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

    public Vector3 getCursorPos()
    {
        return cursorPos;
    }

    public Vector3 getPlayerPos()
    {
        return transform.position;
    }
}