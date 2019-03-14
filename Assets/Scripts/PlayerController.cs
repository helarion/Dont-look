using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
using Aura2API;

public class PlayerController : MonoBehaviour
{
    float minAngle = 10;
    float maxAngle = 90;

    Light lt;
    AuraLight al;
    Rigidbody rb;
    Vector3 cursorPos;

    private bool _hasHistoricPoint;
    private Vector3 _historicPoint;
    [Range(0.1f, 1.0f), Tooltip("How heavy filtering to apply to gaze point bubble movements. 0.1f is most responsive, 1.0f is least responsive.")]
    public float FilterSmoothingFactor = 0.15f;

    [SerializeField] Transform lightTransform;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float sizeSpeed;
    [SerializeField] float stickSpeed;
    [SerializeField] bool controllMouse = false;
    [SerializeField] bool controllEye = true;
    [SerializeField] bool controllGamePad = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lt = GetComponentInChildren<Light>();
        al = GetComponentInChildren<AuraLight>();
        lt.type = LightType.Spot;
        LightEnabled(true);
        Cursor.visible = false;

        cursorPos = TobiiAPI.GetGazePoint().Screen;
        cursorPos.z = 7;
        cursorPos = GameManager.instance.mainCamera.ScreenToWorldPoint(cursorPos);
        cursorPos.z = 7;
    }

    void Update()
    {
        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        Vector2 filteredPoint;
        Vector2 gazePoint = TobiiAPI.GetGazePoint().Screen;
        filteredPoint = Smoothify(gazePoint);

        // MOVEMENT
        if (hMove!=0)
        {
            transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
        }
        // JUMP
        if (Input.GetButtonDown("Jump"))
        {
            print("test");
            rb.AddForce(Vector3.up * jumpSpeed *1000 * Time.deltaTime);
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

            //cursorPos = gazePoint;
            cursorPos = filteredPoint;
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
        float range = Input.GetAxisRaw("LightRange")*sizeSpeed;
        if (range!=0)
        {
            lt.spotAngle += range;
        }

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
        if (isClosed)
        {
            print("tes yeux sont fermés");
        }
        else
        {
            print("tes yeux sont ouverts");
        }
    }

    public void LightEnabled(bool isEnabled)
    {
        lt.enabled = isEnabled;
        al.enabled = isEnabled;
    }
}