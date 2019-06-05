﻿using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region variables
    public PlayerController player;

    [HideInInspector]public static GameManager instance = null;
    [HideInInspector] public List<Enemy> enemyList;
    [SerializeField] public List<Enemy> listE;

    [SerializeField] private float cameraSpeed=3;

    [Header("Debug")]
    [SerializeField] private Checkpoint lastCheckpoint = null;
    [SerializeField] public bool isTesting = false;
    [SerializeField] private Checkpoint[] CheckPointList;
    [SerializeField] private float cameraMoveOffset = 20;
    [SerializeField] private float lightVecOffset = 0.3f;

    [Header("Camera")]
    public Camera mainCamera;
    //[SerializeField] private float maxValue = 0.1f;
    //[SerializeField] private float shakeDuration = 0f;
    //[SerializeField] private float shakeAmount = 0.7f;
    public List<Vector2> shakeRequests = new List<Vector2>();
    float timeDuringCurrentShake = 0.0f;
    [SerializeField] private float decreaseFactor = 1.0f;
    [HideInInspector] public CameraHandler camHandler;
    [SerializeField] private float camSpeedModifer=8;
    private float bobTimer = 0;
    //private float midpoint = 2;
    [SerializeField] private float dutchAngle=0;
    [SerializeField] public float contrePlongeeAngle = 15;
    [SerializeField] private float contrePlongeeHauteur = 1.5f;
    [SerializeField] public float cameraBlockChangeMalusSpeedTime = 0.5f;
    [SerializeField] float cameraBlockChangeMalusSpeedRate = 0.5f;

    [Header("PostProcessValues")]
    [SerializeField] private float maxGrain;
    private float minVignette;
    [SerializeField] private float maxVignette;
    [SerializeField] private float maxTemperature = 100;
    [SerializeField] private float distanceStartVignette=10;
    [SerializeField] private float distanceStartGrain = 10;
    [SerializeField] private float distanceStartTemperature = 10;

    [Header("Layers")]
    [SerializeField] private LayerMask wallsAndMobsLayer;
    [SerializeField] private LayerMask lookLayer;
    [SerializeField] private LayerMask climbLayer;

    [Header("Sons")]
    [SerializeField] private string Intro;
    [SerializeField] private float waitIntroTime = 8;

    [SerializeField] public string ChaseSpiderAmbPlay;
    [SerializeField] public string ChaseSpiderAmbStop;
    [SerializeField] public string ChaseSpiderKillStop;
    [SerializeField] public string ChaseBipedeAmbPlay;
    [SerializeField] public string ChaseBipedeAmbStop;

    [SerializeField] public string playRandomSounds;
    [SerializeField] public string stopRandomSounds;

    [SerializeField] private string pauseGameSounds;
    [SerializeField] private string resumeGameSounds;

    [SerializeField] public string triggerBipede1Sound;

    [SerializeField] public string HeartPlay;
    [SerializeField] public string HeartStop;

    [SerializeField] private string radioPlay;
    [SerializeField] private GameObject radioObject1;
    [SerializeField] private GameObject radioObject2;

    [SerializeField] string monsterDeathSound;
    [SerializeField] string doorDeathSound;

    [SerializeField] public AudioRoom startRoom;
    [SerializeField] int nbAudioRoomId;
    [SerializeField] float audioFadeSpeed=10;
    [SerializeField] private float heartVibration = 0.1f;

    [HideInInspector] public Player controls; // The Rewired Player

    private Vector3 originalPos;
    private bool isPaused = false;
    private bool isTrackerEnabled = true;
    private bool isPlayingHeart = false;

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
        CheckTracker();
        minVignette = PostProcessInstance.instance.vignette.intensity.value;
        camHandler = mainCamera.GetComponent<CameraHandler>();
        controls = ReInput.players.GetPlayer(0);
        originalPos = mainCamera.transform.localPosition;
        Cursor.visible = false;
        enemyList = new List<Enemy>();
        foreach (Enemy e in listE)
        {
            enemyList.Add(e);
        }
        StartCoroutine(ShakeScreenCoroutine());

        if(!isTesting)
        {
            GameManager.instance.SetIsPaused(true);
            StartCoroutine(IntroCoroutine());
        }
        else
        {
            player.SetCurrentAudioRoom(startRoom);
            PlayCurrentAudioRoom(startRoom);
            ResumeGame();
            UIManager.instance.isIntro = false;
            UIManager.instance.FadeOutIntro();
        }
        AkSoundEngine.PostEvent(radioPlay, radioObject1);
        AkSoundEngine.PostEvent(radioPlay, radioObject2);
    }

    IEnumerator IntroCoroutine()
    {
        AkSoundEngine.PostEvent(Intro, gameObject);
        yield return new WaitForSeconds(waitIntroTime);
        player.SetCurrentAudioRoom(startRoom);
        PlayCurrentAudioRoom(startRoom);
        ResumeGame();
        UIManager.instance.FadeOutIntro();
        UIManager.instance.isIntro = false;
        CursorIfPc();
        yield return null;
    }

    public void CursorIfPc()
    {
        if ((!player.GetTobii() || player.GetTobii() && UIManager.instance.GetCheckTracker()) && player.GetInputMode() == PlayerController.InputMode.PC)
        {
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        // PAUSE HANDLER
        if (controls.GetButtonDown("Pause"))
        {
            if (isPaused)
            {
                ResumeGame();
                CursorIfPc();
            }
            else PauseGame();
        }
        if (isPaused) return;
        //mainCamera.transform.localPosition = originalPos;
        //CheckShake();
        TP();
    }

    #endregion

    #region audio
    private void PlayCurrentAudioRoom(AudioRoom ar)
    {
        ar.PlayEvent();
        AkSoundEngine.SetRTPCValue("position_gd_" + ar.id, 50);
        StartCoroutine(FadeInAudioRoutine(ar));
    }

    private void StopCurrentAudioRoom()
    {
        StartCoroutine(FadeOutAudioRoutine());
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
        yield return new WaitForSeconds(5);
        PlayCurrentAudioRoom(lastCheckpoint.aRoom);
        yield return null;
    }

    public void PlayHeart()
    {
        if (isPlayingHeart) return;
        isPlayingHeart = true;
        AkSoundEngine.PostEvent(HeartPlay, gameObject);
        if(player.GetInputMode()==PlayerController.InputMode.Pad)StartCoroutine(HeartCoroutine());
    }

    public void StopHeart()
    {
        if (!isPlayingHeart) return;
        isPlayingHeart = false;
        AkSoundEngine.PostEvent(HeartStop, gameObject);
        StopCoroutine(HeartCoroutine());
    }

    private IEnumerator HeartCoroutine()
    {
        while(isPlayingHeart)
        {
            controls.SetVibration(0, heartVibration,0.3f);
            yield return new WaitForSeconds(0.3f);
            controls.SetVibration(1, heartVibration, 0.3f);
            yield return new WaitForSeconds(1);
        }
        yield return null;
    }

    #endregion

    #region PostProcess

    public void UpdatePostProcess(float distance)
    {
        if (distance < distanceStartGrain)
        {
            float newGrain = distanceStartGrain - distance;
            newGrain = newGrain.Remap(0f, distanceStartGrain, 0f, maxGrain);
            PostProcessInstance.instance.grain.intensity.value = newGrain;
        }
        if (distance < distanceStartVignette)
        {
            float newVignette = distanceStartVignette - distance;
            newVignette = newVignette.Remap(0f, distanceStartVignette, minVignette, maxVignette);
            PostProcessInstance.instance.vignette.intensity.value = newVignette;
        }
        if (distance < distanceStartTemperature)
        {
            float newTemperature = distanceStartTemperature - distance;
            newTemperature = newTemperature.Remap(0f, distanceStartTemperature, 0, maxTemperature);
            PostProcessInstance.instance.colorGrading.temperature.value = newTemperature;
        }
    }

    public void PostProcessReset()
    {
        PostProcessInstance.instance.vignette.intensity.value = minVignette;
        PostProcessInstance.instance.grain.intensity.value = 0;
        PostProcessInstance.instance.colorGrading.temperature.value = 0;
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


    #region Light

    public bool LightDetection(GameObject objectPosition, bool needsConcentration)
    {
        if (!player.lightOn) return false;
        bool isLit = false;
        Vector3 playerPosition = player.getLight().transform.position;
        playerPosition.y += lightVecOffset;
        Vector3 lightVec = player.GetLookAt() - playerPosition;
        Vector3 playerToObjectVec = objectPosition.transform.position - player.transform.position;

        float playerToObjectLength = playerToObjectVec.magnitude;
        Light playerLight = player.getLight();
        BoxCollider col = objectPosition.GetComponent<BoxCollider>();
        if (col == null) col = objectPosition.GetComponentInChildren<BoxCollider>();
        float bonus=0;
        if(col != null) bonus = col.size.x;
        float lightRange = playerLight.range+bonus;
        float lightAngle = playerLight.spotAngle / 2.0f;

        if (objectPosition.name == "Bipede")
        {
            BoxCollider bipedeCollider = objectPosition.GetComponent<BoxCollider>();
            playerToObjectVec = bipedeCollider.bounds.center - player.transform.position;
            playerToObjectLength = playerToObjectVec.magnitude;

            if (playerToObjectLength <= lightRange)
            {
                float angleFromLight = Mathf.Acos(Vector3.Dot(lightVec, playerToObjectVec) / (lightVec.magnitude * playerToObjectLength)) * Mathf.Rad2Deg;

                Vector3 upPoint = bipedeCollider.bounds.center;
                upPoint.y = bipedeCollider.bounds.max.y;
                Vector3 upVec = upPoint - player.transform.position;

                Vector3 downPoint = bipedeCollider.bounds.center;
                downPoint.y = bipedeCollider.bounds.min.y;
                Vector3 downVec = downPoint - player.transform.position;
                float maxAngleFromLight = Mathf.Acos(Vector3.Dot(upVec, downVec) / (upVec.magnitude * downVec.magnitude)) * Mathf.Rad2Deg;
                
                Debug.DrawLine(playerPosition, upPoint, Color.green, Time.deltaTime);
                Debug.DrawLine(playerPosition, downPoint, Color.green, Time.deltaTime);
                if (angleFromLight <= maxAngleFromLight)
                {
                    lightVec = Vector3.RotateTowards(lightVec, playerToObjectVec, angleFromLight, Mathf.Infinity);

                    RaycastHit hit;
                    Ray ray = new Ray(playerPosition, lightVec);
                    Physics.Raycast(ray, out hit, Mathf.Infinity, wallsAndMobsLayer);

                    if (hit.transform.gameObject.tag == objectPosition.tag)
                    {
                        if (!needsConcentration) isLit = true;
                        else if (player.GetConcentration()) isLit = true;
                    }
                    //print("Touched " + hit.transform.gameObject.name);
                    //print(isLit);
                }
            }
            return isLit;
        }

        if (playerToObjectLength <= lightRange)
        {
            float angleFromLight = Mathf.Acos(Vector3.Dot(lightVec, playerToObjectVec) / (lightVec.magnitude * playerToObjectLength)) * Mathf.Rad2Deg;
            if (angleFromLight <= lightAngle)
            {
                lightVec = Vector3.RotateTowards(lightVec, playerToObjectVec, angleFromLight, Mathf.Infinity);

                RaycastHit hit;
                Ray ray = new Ray(playerPosition, lightVec);
                Physics.Raycast(ray, out hit, Mathf.Infinity, wallsAndMobsLayer);

                if (hit.transform.gameObject.tag == objectPosition.tag)
                {
                    if (!needsConcentration) isLit = true;
                    else if( player.GetConcentration()) isLit = true;
                }
                //print("Touched " + hit.transform.gameObject.name);
                //print(isLit);
            }
        }
        return isLit;
    }

    #endregion

    #region death

    // TUE LE JOUEUR
    public void Death(int source)
    {
        if (!player.getIsAlive()) return;
        AkSoundEngine.PostEvent(stopRandomSounds, gameObject);
        player.Reset();
        player.SetIsAlive(false);
        player.ClosedEyes(true);
        UIManager.instance.FadeDeath(true);
        StartCoroutine(DeathCoroutine());
        if(source==0)AkSoundEngine.PostEvent(monsterDeathSound, gameObject);
        else if(source==1) AkSoundEngine.PostEvent(doorDeathSound, gameObject);
    }

    // COROUTINE DE FADE OUT / IN DE LA MORT
    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        RespawnEnemies();
        yield return new WaitForSeconds(0.5f);
        RespawnPlayer();
        PostProcessReset();
        UIManager.instance.FadeDeath(false);
        player.ClosedEyes(false);
    }

    // RESPAWN CHAQUE ENNEMI
    private void RespawnEnemies()
    {
        //print("test-1");
        foreach (Enemy e in enemyList)
        {
            //print("test0");
            e.StopChase();
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
        contrePlongeeAngle = c.sRoom.newContrePlongeeAngle;
        contrePlongeeHauteur = c.sRoom.newContrePlongeeHauteur;
        player.SetLightRange(c.sRoom.newLightRange);
        player.SetLightAngle(c.sRoom.newLightAngle);
        SetDutchAngle(c.sRoom.newDutchAngle);
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
        if (!Input.GetKey(KeyCode.Tab))
        {
            return;
        }
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
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            check = 6;
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            check = 7;
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            check = 8;
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            check = 9;
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            check = 10;
        }
        else if (Input.GetKeyDown(KeyCode.F12))
        {
            check = 11;
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
        //print(newPos);
        float speed = cameraSpeed;
        speed /= (1 + player.velocity * camSpeedModifer);
        if (player.changingCameraBlock)
        {
            speed /= cameraBlockChangeMalusSpeedRate;
        }
        newPos.y += contrePlongeeHauteur;
        mainCamera.transform.localPosition = Vector3.Lerp(originalPos, newPos, Time.deltaTime * speed);
        originalPos = Vector3.Lerp(originalPos, newPos, Time.deltaTime/speed);
    }

public void RotateCamera(Quaternion newRotate)
    {
        //print(newRotate);
        //float speed = cameraSpeed;
        //if(player.velocity >0) speed /= (player.velocity * cameraMoveOffset);
        newRotate.z = dutchAngle.Remap(0,360,0,1);

        if(player.getCameraBlock()==null)newRotate.x = contrePlongeeAngle.Remap(0, 360, 0, 1);
        mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, newRotate, Time.deltaTime/cameraSpeed);
    }

    #region shake
    // Shaking screen for duration set previously
   /* private void CheckShake()
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
    }*/

    private IEnumerator ShakeScreenCoroutine()
    {
        bool vibrated = false;
        while (true)
        {
            if (shakeRequests.Count != 0 && !isPaused)
            {
                if (!vibrated && player.GetInputMode() == PlayerController.InputMode.Pad)
                {
                    controls.SetVibration(0, shakeRequests[0].y, shakeRequests[0].x);
                    controls.SetVibration(1, shakeRequests[0].y, shakeRequests[0].x);
                    vibrated = true;
                }
                mainCamera.transform.localPosition = originalPos + Random.insideUnitSphere * shakeRequests[0].y;
                timeDuringCurrentShake += Time.deltaTime * decreaseFactor;
                if (timeDuringCurrentShake > shakeRequests[0].x)
                {
                    shakeRequests.RemoveAt(0);
                    timeDuringCurrentShake = 0.0f;
                    vibrated = false;
                }
            }
            yield return null;
        }
    }

    // SHAKESCREEN POUR LA DUREE ENTREE
    public void ShakeScreen(float duration, float intensity, int offset = 0)
    {
        //print("screenshake : " + duration + ";" + intensity);
        if (shakeRequests.Count == offset)
        {
            shakeRequests.Insert(offset, new Vector2(duration, intensity));
        }
        else
        {
            if (intensity < shakeRequests[offset].y)
            {
                if (duration > shakeRequests[offset].x)
                {
                    ShakeScreen(duration - shakeRequests[offset].x, intensity, offset + 1);
                }
            }
            else
            {
                if (offset == 0)
                {
                    shakeRequests[0] = new Vector2(shakeRequests[0].x - timeDuringCurrentShake, shakeRequests[0].y);
                    timeDuringCurrentShake = 0.0f;
                }
                if (duration < shakeRequests[offset].x)
                {
                    Vector2 oldShakeRequest = shakeRequests[offset];
                    shakeRequests.RemoveAt(offset);
                    shakeRequests.Insert(offset, new Vector2(duration, intensity));
                    shakeRequests.Insert(offset + 1, new Vector2(oldShakeRequest.x - duration, oldShakeRequest.y));
                }
                else
                {
                    shakeRequests.Insert(offset, new Vector2(duration, intensity));
                    while (shakeRequests.Count > offset + 1)
                    {
                        if (duration > shakeRequests[offset + 1].x)
                        {
                            duration -= shakeRequests[offset + 1].x;
                            shakeRequests.RemoveAt(offset + 1);
                        }
                        else
                        {
                            shakeRequests[offset + 1] = new Vector2(shakeRequests[offset + 1].x - duration, shakeRequests[offset + 1].y);
                        }
                    }
                }
            }
        }
    }

    /*
    public void ShakeScreen(float duration, float intensity)
    {
        if (shakeDuration > 0)
        {
            if (intensity > shakeAmount) shakeAmount = intensity;
        }
        else shakeAmount = intensity;
        shakeDuration = duration; 
    }
    */

    #endregion


    #endregion

    #region pause
    public void PauseGame()
    {
        Cursor.visible = true;
        Time.timeScale = 0;
        isPaused = true;
        AkSoundEngine.PostEvent(pauseGameSounds, gameObject);
        UIManager.instance.Pause(true);
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Time.timeScale = 1;
        isPaused = false;
        AkSoundEngine.PostEvent(resumeGameSounds, gameObject);
        UIManager.instance.Pause(false);
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
    
    public void SetDutchAngle(float angle)
    {
        dutchAngle = angle;
    }

    public void SetContrePlongeeHauteur(float hauteur)
    {
        contrePlongeeHauteur = hauteur;
    }

    public void SetContrePlongeeAngle(float angle)
    {
        contrePlongeeAngle = angle;
    }
    #endregion
}

public static class ExtensionMethods
{
    public static float Remap(this float value, float vMin, float vMax, float rMin, float rMax)
    {

        return rMin + (Mathf.Clamp(value, vMin, vMax) - vMin) * (rMax - rMin) / (vMax - vMin);
    }
}