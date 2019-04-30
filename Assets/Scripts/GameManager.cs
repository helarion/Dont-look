﻿using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public PlayerController player;

    [HideInInspector]public static GameManager instance = null;
    private List<Enemy> enemyList;

    [SerializeField] private float cameraSpeed=3;

    [Header("Debug")]
    [SerializeField] private Checkpoint lastCheckpoint = null;
    [SerializeField] public bool isTesting = false;
    [SerializeField] private Checkpoint[] CheckPointList;
    [SerializeField] private float cameraMoveOffset = 20;

    [Header("ScreenShake")]
    [SerializeField] private float shakeDuration = 0f;
    [SerializeField] private float shakeAmount = 0.7f;
    [SerializeField] private float decreaseFactor = 1.0f;
    [SerializeField] private float maxValue = 0.1f;

    [Header("Layers")]
    [SerializeField] private LayerMask wallsAndMobsLayer;
    [SerializeField] private LayerMask lookLayer;
    [SerializeField] private LayerMask climbLayer;

    [Header("Sons")]
    [SerializeField] public AK.Wwise.Event ChaseAmbPlay;
    [SerializeField] public AK.Wwise.Event ChaseAmbStop;
    [SerializeField] public AK.Wwise.Event HeartPlay;
    [SerializeField] public AK.Wwise.Event HeartStop;


    [HideInInspector] public Player controls; // The Rewired Player

    private Vector3 originalPos;
    private bool isPaused = false;
    private bool isTrackerEnabled = true;

    private void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CheckTracker();
        controls = ReInput.players.GetPlayer(0);
        ResumeGame();
        originalPos = mainCamera.transform.localPosition;
        Cursor.visible = false;
        enemyList = new List<Enemy>();
        Enemy[] temp = FindObjectsOfType<Enemy>();
        foreach(Enemy e in temp)
        {
            enemyList.Add(e);
        }
    }

    private void Update()
    {
        // PAUSE HANDLER
        if (controls.GetButtonDown("Pause"))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
        if (isPaused) return;
        CheckShake();
        TP();
    }

   // [ExecuteInEditMode]
    private void TP()
    {
        int check = -1;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            check = 0;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            check = 1;
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            check = 2;
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            check = 3;
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            check = 4;
        }

        if (check != -1)
        {
            player.Reset();
            UseCheckpoint(CheckPointList[check]);
        }
    }

    // Shaking screen for duration set previously
    private void CheckShake()
    {
        if (shakeDuration > 0)
        {
            mainCamera.transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            mainCamera.transform.localPosition = originalPos;
        }
    }

    // RESPAWN CHAQUE ENNEMI
    private void RespawnEnemies()
    {
        foreach(Enemy e in enemyList)
        {
            e.Respawn();
        }
    }

    // TUE LE JOUEUR
    public void Death()
    {
        if (!player.getIsAlive()) return;
        player.Reset();
        player.SetIsAlive(false);
        UIManager.instance.FadeDeath(true);
        StartCoroutine("DeathCoroutine");
    }

    public void UseCheckpoint(Checkpoint c)
    {
        player.transform.position = c.transform.position;
        mainCamera.GetComponent<CameraHandler>().SetNewZ(c.newZ);
    }

    // RESPAWN LE JOUEUR
    private void RespawnPlayer()
    {
        lastCheckpoint.Reset();
        UseCheckpoint(lastCheckpoint);
        player.SetIsAlive(true);
    }

    public void DeleteEnemyFromList(Enemy e)
    {
        //int index =enemyList.IndexOf(e);
        enemyList.Remove(e);
    }

    // COROUTINE DE FADE OUT / IN DE LA MORT
    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        RespawnEnemies();
        RespawnPlayer();
        UIManager.instance.FadeDeath(false);
    }

    // SAUVEGARDE LE NOUVEAU CHECKPOINT : POINT DE RESPAWN POUR LE JOUEUR 
    public void SetNewCheckpoint(Checkpoint c)
    {
        if (c == lastCheckpoint) return;
        lastCheckpoint = c;
        //print("New Checkpoint activated");
    }

    // SHAKESCREEN POUR LA DUREE ENTREE
    public void ShakeScreen(float duration)
    {
        shakeDuration = duration;
    }

    // SHAKESCREEN A VALEUR PROGRESSIVE /// PAS ENCORE FONCTIONNEL
    public void ProgressiveShake(float duration)
    {
        float savedAmount = shakeAmount;
        shakeAmount = 0;
        ShakeScreen(duration);
        StartCoroutine("ShakeCoroutine", duration);
        shakeAmount = savedAmount;
    }

    // SHAKESCREEN PROGRESSIF COROUTINE /// PAS ENCORE FONCTIONNEL
    private IEnumerator ShakeCoroutine(float maxTime)
    {
        float time = 0;
        float update = maxValue *(0.01f*maxTime);
        while (time<maxTime)
        {
            if (shakeAmount < maxValue) shakeAmount += update;
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

    public void CheckTracker()
    {
        isTrackerEnabled = UIManager.instance.GetCheckTracker();
    }

    // BOUGER LA CAMERA
    public void MoveCamera(Vector3 newPos)
    {
        float speed = cameraSpeed;
        if (player.velocity > 0) speed /= (player.velocity * cameraMoveOffset);
        originalPos = Vector3.Lerp(originalPos, newPos, Time.deltaTime/speed);
    }

    public void RotateCamera(Quaternion newRotate)
    {
        //print(newRotate);
        //float speed = cameraSpeed;
        //if(player.velocity >0) speed /= (player.velocity * cameraMoveOffset);
        mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, newRotate, Time.deltaTime/cameraSpeed);
    }

    public void PauseGame()
    {
        Cursor.visible = true;
        Time.timeScale = 0;
        isPaused = true;
        UIManager.instance.pausePanel.SetActive(true);
        //UIManager.instance.FadePause(true);
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Time.timeScale = 1;
        isPaused = false;
        UIManager.instance.pausePanel.SetActive(false);
        //UIManager.instance.FadePause(false);
    }

    #region getter-setter

    public LayerMask GetWallsAndMobsLayer()
    {
        return wallsAndMobsLayer;
    }

    public LayerMask GetLookLayer()
    {
        return lookLayer;
    }

    public LayerMask GetClimbLayer()
    {
        return climbLayer;
    }

    public float GetShakeIntensity()
    {
        return shakeAmount;
    }

    public void SetShakeIntensity(float newIntensity)
    {
        shakeAmount = newIntensity;
    }

    public bool GetIsTrackerEnabled()
    {
        return isTrackerEnabled;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }

    public void SetIsPaused(bool b)
    {
        isPaused = b;
    }
    #endregion
}
