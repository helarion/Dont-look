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
    [SerializeField] int nbAudioRoomId;
    [SerializeField] float audioFadeSpeed=10;

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
        player.SetCurrentAudioRoom(startRoom);
        PlayCurrentAudioRoom(startRoom);
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

    #region audio
    private void PlayCurrentAudioRoom(AudioRoom ar)
    {
        ar.PlayEvent();
        AkSoundEngine.SetRTPCValue("position_gd_" + ar.id, 50);
        StartCoroutine("FadeInAudioRoutine", ar);
    }

    private void StopCurrentAudioRoom()
    {
        StartCoroutine("FadeOutAudioRoutine");
    }

    private void ResetAllAudioRooms()
    {
        for (int i = 0; i < nbAudioRoomId; i++)
        {
            AkSoundEngine.SetRTPCValue("position_relative_volume_" + i, 0);
            AkSoundEngine.SetRTPCValue("position_gd_" + i, 0);
        }
    }

    IEnumerator FadeInAudioRoutine(AudioRoom ar)
    {
        player.SetCurrentAudioRoom(lastCheckpoint.aRoom);
        float i = 0;
        while (i<50)
        {
            i += Time.deltaTime*audioFadeSpeed;
            AkSoundEngine.SetRTPCValue("position_relative_volume_" + ar.id, i);
            yield return new WaitForEndOfFrame();
        }
        i = 50;
        AkSoundEngine.SetRTPCValue("position_relative_volume_" + ar.id, i);

        yield return null;
    }

    IEnumerator FadeOutAudioRoutine()
    {
        float value = 50;
        //AkSoundEngine.GetRTPCValue("position_relative_volume_" + player.GetCurrentAudioRoom().id,0,0,value);
        while(value>0)
        {
            value-= Time.deltaTime * audioFadeSpeed;
            AkSoundEngine.SetRTPCValue("position_relative_volume_" + player.GetCurrentAudioRoom().id, value);
            yield return new WaitForEndOfFrame();
        }
        AkSoundEngine.SetRTPCValue("position_relative_volume_" + player.GetCurrentAudioRoom().id, 0);
        ResetAllAudioRooms();
        PlayCurrentAudioRoom(lastCheckpoint.aRoom);
        yield return null;
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

    #endregion

    public void CheckTracker()
    {
        isTrackerEnabled = UIManager.instance.GetCheckTracker();
    }

    public void DeleteEnemyFromList(Enemy e)
    {
        //int index =enemyList.IndexOf(e);
        enemyList.Remove(e);
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

    #region Checkpoints

    public void UseCheckpoint(Checkpoint c)
    {
        player.transform.position = c.transform.position;
        camHandler.SetNewZ(c.sRoom.newZ);
        camHandler.SetNewOffset(c.sRoom.newOffset);
        player.SetLightRange(c.sRoom.newLightRange);
        StopCurrentAudioRoom();
    }

    // SAUVEGARDE LE NOUVEAU CHECKPOINT : POINT DE RESPAWN POUR LE JOUEUR 
    public void SetNewCheckpoint(Checkpoint c)
    {
        if (c == lastCheckpoint) return;
        lastCheckpoint = c;
        //print("New Checkpoint activated");
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
        else if(Input.GetKeyDown(KeyCode.F6))
        {
            check = 5;
        }

        if (check != -1)
        {
            player.Reset();
            UseCheckpoint(CheckPointList[check]);
        }
    }

    #endregion

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
        if (shakeDuration > 0)
        {
            if (intensity > shakeAmount) shakeAmount = intensity;
        }
        else shakeAmount = intensity;
        shakeDuration = duration; 
    }

    #endregion


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
