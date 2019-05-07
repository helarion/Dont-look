using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image fadeImg = null;
    [SerializeField] private float fadeTime=0.5f;
    public GameObject pausePanel = null;
    [SerializeField] private Image pauseImg = null;
    [SerializeField] private GameObject ControlPanel=null;
    [SerializeField] private GameObject NoEyePanel=null;
    [SerializeField] private Slider volumeControl;
    [SerializeField] private Slider gammaControl;
    [SerializeField] private Toggle toggleTracker;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private PostProcessVolume postProcess;
    [SerializeField] private float gammaMin=-1.5f;
    [SerializeField] private float gammaMax=1.5f;

    private ColorGrading colorGrading;
    public bool isFading = false;

    public static UIManager instance = null;

    void Awake()
    {
        StopAllCoroutines();
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        postProcess.profile.TryGetSettings(out colorGrading);
        volumeControl.onValueChanged.AddListener(delegate { VolumeValueChangeCheck(); });
        gammaControl.onValueChanged.AddListener(delegate { GammaValueChangeCheck(); });
        if (!GameManager.instance.isTesting)
        {
            GameManager.instance.SetIsPaused(true);
            fadeImg.color = new Color(0, 0, 0, 1);
            FadeOut(fadeImg, 2, 0);
            StartCoroutine("StartCoroutine");
        }
        eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        volumeControl.OnSelect(null);
        toggleTracker.OnSelect(null);
    }

    public void GammaValueChangeCheck()
    {
        colorGrading.enabled.value = true;
        float gamma = gammaControl.value.Remap(gammaControl.minValue, gammaControl.maxValue, gammaMin, gammaMax);
        print("gamma:" + gamma);
        Vector4 vec= new Vector4(gamma,gamma,gamma,gamma);
        colorGrading.gamma.value =vec;
    }

    public void VolumeValueChangeCheck()
    {
        AkSoundEngine.SetRTPCValue("Master_Volume_Slider", volumeControl.value);
        //Debug.Log(volumeControl.value);
    }

    IEnumerator StartCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        GameManager.instance.SetIsPaused(false);
    }

    IEnumerator FadeOutCoroutine(Image img, float duration, float waitBefore)
    {
        Color savedColor = img.color;
        savedColor.a = 1;
        img.color = savedColor;
        while (isFading)
        {
            yield return new WaitForSeconds(0.1f);
        }
        isFading = true;
        yield return new WaitForSeconds(waitBefore);
        for (float i = duration; i > 0; i -= Time.deltaTime / duration)
        {
            // set color with i as alpha
            savedColor.a = i;
            img.color = savedColor;
            yield return null;
        }
        savedColor.a = 0;
        img.color = savedColor;
        isFading = false;
    }

    IEnumerator FadeInCoroutine(Image img, float duration, float waitBefore)
    {
        Color savedColor = img.color;
        savedColor.a = 0;
        img.color = savedColor;
        while (isFading)
        {
            yield return new WaitForSeconds(0.1f);
        }
        isFading = true;
        yield return new WaitForSeconds(waitBefore);
        for (float i = 0; i < duration; i += Time.deltaTime / duration)
        {
            // set color with i as alpha
            savedColor.a = i;
            img.color = savedColor;
            yield return null;
        }
        savedColor.a = 1;
        img.color = savedColor;
        isFading = false;
    }

    IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(1f);
        while(isFading)
        {
            yield return null;
        }
        SceneManager.LoadScene(4);
        yield return null;
    }

    public void FadeDeath(bool b)
    {
        if(b)FadeIn(fadeImg, 1f, 0f);
        else FadeOut(fadeImg, 1f, 1f);
    }

    public void FadePause(bool b)
    {
        if (b) FadeIn(pauseImg, 0.5f, 0);
        else FadeOut(pauseImg, 0.5f, 0);
    }

    public void FadeIn(Image img, float duration, float waitBefore)
    {
        StartCoroutine(FadeInCoroutine(img,duration,waitBefore));
    }


    public void FadeInEnd()
    {
        StartCoroutine(FadeInCoroutine(fadeImg,2f,0f));
        StartCoroutine("EndCoroutine");
    }

    public void FadeOut(Image img, float duration, float waitBefore)
    {
        StartCoroutine(FadeOutCoroutine(img,duration,waitBefore));
    }

    public void DisableControlPanel(bool b)
    {
        ControlPanel.SetActive(!b);
        NoEyePanel.SetActive(b);
        
        if(!b)
        {
            eventSystem.firstSelectedGameObject = toggleTracker.gameObject;
            eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        }
        else
        {
            eventSystem.firstSelectedGameObject = volumeControl.gameObject;
            eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        }
    }

    public bool GetCheckTracker()
    {
        return toggleTracker.isOn;
    }
}
