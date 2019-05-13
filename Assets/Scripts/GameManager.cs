using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region variables
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
    [SerializeField] private float bobbingSpeed = 0.25f;
    [SerializeField] private float normalBobbingAmount = 0.2f;
    [SerializeField] private float runningBobbingAmount = 0.5f;
    private float bobTimer = 0;
    //private float midpoint = 2;
    [SerializeField] private float dutchAngle=0;

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
        //AkSoundEngine.SetObjectObstructionAndOcclusion(iGameObject, MAIN_LISTENER, fCalculatedObs, fCalculatedOcc);
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
        StartCoroutine(ShakeScreenCoroutine());
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
        //mainCamera.transform.localPosition = originalPos;
        //CheckShake();
        CameraBob();
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
        PlayCurrentAudioRoom(lastCheckpoint.aRoom);
        yield return null;
    }

    public void PlayHeart()
    {
        if (isPlayingHeart) return;
        isPlayingHeart = true;
        AkSoundEngine.PostEvent(HeartPlay, player.gameObject);
        if(player.GetInputMode()==PlayerController.InputMode.Pad)StartCoroutine(HeartCoroutine());
    }

    public void StopHeart()
    {
        if (!isPlayingHeart) return;
        isPlayingHeart = false;
        AkSoundEngine.PostEvent(HeartStop, player.gameObject);
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

    public void CheckTracker()
    {
        isTrackerEnabled = UIManager.instance.GetCheckTracker();
    }

    public void DeleteEnemyFromList(Enemy e)
    {
        //int index =enemyList.IndexOf(e);
        enemyList.Remove(e);
    }

    public bool LightDetection(Transform objectPosition, bool needsConcentration)
    {
        bool isLit = false;
        Vector3 playerPosition = player.getLight().transform.position;
        playerPosition.y += lightVecOffset;
        Vector3 lightVec = player.GetLookAt() - playerPosition;
        Vector3 playerToObjectVec = objectPosition.position - player.transform.position;

        float playerToObjectLength = playerToObjectVec.magnitude;
        Light playerLight = player.getLight();
        float lightRange = playerLight.range;
        float lightAngle = playerLight.spotAngle / 2.0f;
        if (playerToObjectLength <= lightRange)
        {
            float angleFromLight = Mathf.Acos(Vector3.Dot(lightVec, playerToObjectVec) / (lightVec.magnitude * playerToObjectVec.magnitude)) * Mathf.Rad2Deg;
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

    #region death

    // TUE LE JOUEUR
    public void Death()
    {
        if (!player.getIsAlive()) return;
        player.Reset();
        player.SetIsAlive(false);
        UIManager.instance.FadeDeath(true);
        StartCoroutine(DeathCoroutine());
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
        float speed = cameraSpeed;
        if (player.velocity > 0) speed /= (player.velocity * cameraMoveOffset);
        mainCamera.transform.localPosition = Vector3.Lerp(originalPos, newPos, Time.deltaTime / speed);
        originalPos = Vector3.Lerp(originalPos, newPos, Time.deltaTime/speed);
    }

    private void CameraBob()
    {
        float bobbingAmount= normalBobbingAmount;
        if (player.GetIsRunning()) bobbingAmount = runningBobbingAmount;

        float waveslice = 0.0f;
        float horizontal = controls.GetAxis("Move Horizontal");
        Vector3 cSharpConversion = originalPos;

        if (Mathf.Abs(horizontal) == 0)
        {
            bobTimer = 0.0f;
        }
        else
        {
            waveslice = Mathf.Sin(bobTimer);
            bobTimer += bobbingSpeed;
            if (bobTimer > Mathf.PI * 2)
            {
                bobTimer -=Mathf.PI * 2;
            }
        }
        if (waveslice != 0)
        {
            float translateChange = waveslice * bobbingAmount;
            float totalAxes = Mathf.Abs(horizontal);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            cSharpConversion.y = originalPos.y + translateChange;
        }
        else
        {
            cSharpConversion.y = originalPos.y;
        }
        //print("bob:" + cSharpConversion);
        MoveCamera(cSharpConversion);
    }

public void RotateCamera(Quaternion newRotate)
    {
        //print(newRotate);
        //float speed = cameraSpeed;
        //if(player.velocity >0) speed /= (player.velocity * cameraMoveOffset);
        newRotate.z = dutchAngle.Remap(0,360,0,1);
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
            if (shakeRequests.Count != 0)
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
        UIManager.instance.Pause(true);
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Time.timeScale = 1;
        isPaused = false;
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
    #endregion
}

public static class ExtensionMethods
{
    public static float Remap(this float value, float vMin, float vMax, float rMin, float rMax)
    {

        return rMin + (Mathf.Clamp(value, vMin, vMax) - vMin) * (rMax - rMin) / (vMax - vMin);
    }
}