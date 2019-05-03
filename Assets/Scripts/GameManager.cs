using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region variables
    public Camera mainCamera;
    public PlayerController player;

    [HideInInspector]public static GameManager instance = null;
    private List<Enemy> enemyList;
    [SerializeField] Enemy[] listE;

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
    [SerializeField] public string ChaseAmbPlay;
    [SerializeField] public string ChaseAmbStop;
    [SerializeField] public string HeartPlay;
    [SerializeField] public string HeartStop;
    [SerializeField] public AudioRoom startRoom;

    [HideInInspector] public Player controls; // The Rewired Player

    private Vector3 originalPos;
    private bool isPaused = false;
    private bool isTrackerEnabled = true;
    private bool isPlayingHeart = false;
    [HideInInspector] public CameraHandler camHandler;
    #endregion

    #region startupdate
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
        AkSoundEngine.PostEvent(startRoom.playEvent, GameManager.instance.gameObject);
        AkSoundEngine.SetRTPCValue("position_relative_volume_0", 50);
        AkSoundEngine.SetRTPCValue("position_gd_0", 50);
        CheckTracker();
        camHandler = mainCamera.GetComponent<CameraHandler>();
        controls = ReInput.players.GetPlayer(0);
        ResumeGame();
        originalPos = mainCamera.transform.localPosition;
        Cursor.visible = false;
        enemyList = new List<Enemy>();
        foreach(Enemy e in listE)
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

    #endregion

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

    public void UseCheckpoint(Checkpoint c)
    {
        player.transform.position = c.transform.position;
        camHandler.SetNewZ(c.room.newZ);
        camHandler.SetNewOffset(c.room.newOffset);
        player.SetLightRange(c.room.newLightRange);
        
    }

    public void CheckTracker()
    {
        isTrackerEnabled = UIManager.instance.GetCheckTracker();
    }

    public void DeleteEnemyFromList(Enemy e)
    {
        //int index =enemyList.IndexOf(e);
        enemyList.Remove(e);
    }
    // SAUVEGARDE LE NOUVEAU CHECKPOINT : POINT DE RESPAWN POUR LE JOUEUR 
    public void SetNewCheckpoint(Checkpoint c)
    {
        if (c == lastCheckpoint) return;
        lastCheckpoint = c;
        //print("New Checkpoint activated");
    }

    public void PlayHeart()
    {
        if (isPlayingHeart) return;
        StartCoroutine("HeartCoroutine");
    }

    private IEnumerator HeartCoroutine()
    {
        isPlayingHeart = true;
        AkSoundEngine.PostEvent(HeartPlay, player.gameObject);
        yield return new WaitForSeconds(6);
        AkSoundEngine.PostEvent(HeartStop, player.gameObject);
        isPlayingHeart = false;
    }

    #region death

    // TUE LE JOUEUR
    public void Death()
    {
        if (!player.getIsAlive()) return;
        player.Reset();
        player.SetIsAlive(false);
        UIManager.instance.FadeDeath(true);
        StartCoroutine("DeathCoroutine");
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

    // RESPAWN CHAQUE ENNEMI
    private void RespawnEnemies()
    {
        //print("test-1");
        foreach (Enemy e in enemyList)
        {
            //print("test0");
            e.Respawn();
        }
        //print("test6");
    }

    // RESPAWN LE JOUEUR
    private void RespawnPlayer()
    {
        lastCheckpoint.Reset();
        UseCheckpoint(lastCheckpoint);
        player.SetIsAlive(true);
    }

    #endregion

    #region shake
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

    // SHAKESCREEN POUR LA DUREE ENTREE
    public void ShakeScreen(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeAmount = intensity;
    }

    #endregion

    #region camera
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

    #endregion

    #region pause
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
    #endregion

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
