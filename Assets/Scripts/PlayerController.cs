﻿using System.Collections;
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

    [SerializeField] Transform lightTransform;
    [SerializeField] float moveSpeed;
    [SerializeField] float sizeSpeed;
    [SerializeField] bool controllMouse=false;
    [SerializeField] bool controllEye = true;

    void Start()
    {
        lt = GetComponentInChildren<Light>();
        al = GetComponentInChildren<AuraLight>();
        lt.type = LightType.Spot;
    }

    void Update()
    {
        float xMouse = Input.GetAxis("Mouse X");
        float yMouse = Input.GetAxis("Mouse Y");

        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        Vector2 filteredPoint;
        Vector2 gazePoint = TobiiAPI.GetGazePoint().Screen;
        //filteredPoint = Vector2.Lerp(filteredPoint, gazePoint, 0.5f);

        if (hMove!=0)
        {
            transform.Translate(Vector3.right * hMove * moveSpeed * Time.deltaTime);
        }


        if (controllMouse && (xMouse !=0 || yMouse !=0))
        {
            // lt.spotAngle += xMouse;
            //Vector3 pos = Input.mousePosition;
            Vector3 pos = Input.mousePosition;
            pos.z = 7;
            pos = GameManager.instance.mainCamera.ScreenToWorldPoint(pos);
            pos.z = 7;
            lightTransform.LookAt(pos);
        }
        else if(controllEye)
        {
            Vector3 pos = gazePoint;
            //print(gazePoint);
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

    public void LightEnabled(bool isEnabled)
    {
        lt.enabled = isEnabled;
        al.enabled = isEnabled;
    }
}