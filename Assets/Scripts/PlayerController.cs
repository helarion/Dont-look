using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float minAngle = 10;
    float maxAngle = 90;

    Light lt;
    [SerializeField] Transform lightTransform;
    [SerializeField] float moveSpeed;
    [SerializeField] float sizeSpeed;
    [SerializeField] bool moveCamera=false;

    void Start()
    {
        lt = GetComponentInChildren<Light>();
        lt.type = LightType.Spot;
    }

    void Update()
    {
        float xMouse = Input.GetAxis("Mouse X");
        float yMouse = Input.GetAxis("Mouse Y");

        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        if(hMove!=0)
        {
            transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
        }

        if (xMouse !=0 || yMouse !=0)
        {
            // lt.spotAngle += xMouse;
            Vector3 pos = Input.mousePosition;
            pos.z = 7;
            pos = GameManager.instance.mainCamera.ScreenToWorldPoint(pos);
            pos.z = 7;
            lightTransform.LookAt(pos);
        }

        float range = Input.GetAxisRaw("LightRange")*sizeSpeed;
        if (range!=0)
        {
            lt.spotAngle += range;
            //lt.range += range;
        }
    }
}