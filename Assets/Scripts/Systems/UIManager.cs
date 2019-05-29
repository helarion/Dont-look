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
    [SerializeField] private Slider brightnessControl;
    [SerializeField] private Toggle toggleTracker;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private float waitBeforeDeath=5;
    [SerializeField] private float brightnessMin = -1.5f;
    [SerializeField] private float brightnessMax = 1.5f;
    [SerializeField] private float waitEndTime = 10;
    [SerializeField] private int endScene = 6;

    [Header("Sounds")]
    [SerializeField] private string sliderSound;
    [SerializeField] private string checkboxSound;

    public bool isFading = false;
    private bool isEnding = false;
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
        PostProcessInstance.instance.bloom.active = true;
        float brightness = PostProcessInstance.instance.colorGrading.postExposure.value;
        float value = brightness.Remap(brightnessMin, brightnessMax, brightnessControl.minValue, brightnessControl.maxValue);
        brightnessControl.value = value;
        fadeImg.color = new Color(0, 0, 0, 1);
    }

    public void FadeOutIntro()
    {
        FadeOut(fadeImg, 2, 0);
        StartCoroutine(StartCoroutine());
    }

    public void BrightnessValueChangeCheck()
    {
        AkSoundEngine.PostEvent(sliderSound, gameObject);
        PostProcessInstance.instance.colorGrading.enabled.value = true;
        float brightness = brightnessControl.value.Remap(brightnessControl.minValue, brightnessControl.maxValue, brightnessMin, brightnessMax);
        PostProcessInstance.instance.colorGrading.postExposure.value = brightness;
    }

    public void VolumeValueChangeCheck()
    {
        AkSoundEngine.PostEvent(sliderSound, gameObject);
        AkSoundEngine.SetRTPCValue("Master_Volume_Slider", volumeControl.value);
    }

    public void CheckBoxValueChangeCheck()
    {
        AkSoundEngine.PostEvent(checkboxSound, gameObject);
        GameManager.instance.CheckTracker();
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
        AkSoundEngine.PostEvent(GameManager.instance.playRandomSounds, GameManager.instance.gameObject);
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
        if(isEnding) SceneManager.LoadScene(endScene);// StartCoroutine(EndCoroutine());
    }

    /*IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(1f);
        while(isFading)
        {
            yield return new WaitForSeconds(0.5f);
        }
        SceneManager.LoadScene(endScene);
        yield return null;
    }*/

    public void FadeDeath(bool b)
    {
        if(b)FadeIn(fadeImg, 0.3f, 0f);
        else FadeOut(fadeImg, 1.5f, waitBeforeDeath);
    }

    public void Pause(bool b)
    {
        pausePanel.SetActive(b);
        //eventSystem.firstSelectedGameObject = gammaControl.gameObject;
        eventSystem.SetSelectedGameObject(brightnessControl.gameObject);
        brightnessControl.Select();
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
        isEnding = true;
        StartCoroutine(Wait(waitEndTime));
        StartCoroutine(FadeInCoroutine(fadeImg,2f,0f));
    }

    IEnumerator Wait(float waitTime)
    {
        isFading = true;
        yield return new WaitForSeconds(waitTime);
        isFading = false;
        yield return null;
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
            //eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        }
        else
        {
            eventSystem.firstSelectedGameObject = brightnessControl.gameObject;
            //eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        }
    }

    public bool GetCheckTracker()
    {
        return toggleTracker.isOn;
    }
}
