using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro2 : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float waitBefore=0.5f;
    [SerializeField] private Image fadeImg;
    [SerializeField] Canvas firstCanvas;
    [SerializeField] Canvas hasEyeTracker;
    [SerializeField] Canvas noEyeTracker;
    [SerializeField] int scene=2;
    [SerializeField] Slider gammaControl;
    [SerializeField] private float gammaMin = -1.5f;
    [SerializeField] private float gammaMax = 1.5f;

    private ColorGrading colorGrading;
    private Bloom bloom;
    private bool isFading=false;
    private bool isEnding = false;
    private bool waitForButton = true;

    private void Start()
    {
        if (scene == -1)
        {
            AkSoundEngine.PostEvent("Play_Random_Track2", gameObject);
            if(GameManager.instance!=null) Destroy(GameManager.instance.gameObject);
            if(UIManager.instance!=null) Destroy(UIManager.instance.gameObject);
        }
        StartCoroutine(FadeOutCoroutine());
        if (scene == 2)
        {
            StartCoroutine(ControlersCoroutine());
        }
        else if(scene ==3)
        {
            PostProcessInstance.instance.volume.profile.TryGetSettings(out colorGrading);
            PostProcessInstance.instance.volume.profile.TryGetSettings(out bloom);
            bloom.active = false;
            gammaControl.onValueChanged.AddListener(delegate { GammaValueChangeCheck(); });
            StartCoroutine(BrightnessCoroutine());
        }
    }

    public void GammaValueChangeCheck()
    {
        colorGrading.enabled.value = true;
        float gamma = gammaControl.value.Remap(gammaControl.minValue, gammaControl.maxValue, gammaMin, gammaMax);
        print("gamma:" + gamma);
        Vector4 vec = new Vector4(gamma, gamma, gamma, gamma);
        colorGrading.gamma.value = vec;
    }

    public void ButtonNext()
    {
        waitForButton = false;
    }

    IEnumerator StartCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        GameManager.instance.SetIsPaused(false);
    }

    IEnumerator BrightnessCoroutine()
    {
        while(waitForButton)
        {
            yield return new WaitForEndOfFrame();
        }
        isEnding = true;
        StartCoroutine(FadeInCoroutine());
        yield return null;
    }

    IEnumerator ControlersCoroutine()
    {
        while (isFading)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(5);
        StartCoroutine(FadeInCoroutine());
        while (isFading)
        {
            yield return new WaitForEndOfFrame();
        }
        firstCanvas.gameObject.SetActive(false);
        bool tobiiConnected = Tobii.Gaming.TobiiAPI.IsConnected;
        if (tobiiConnected)
        {
            hasEyeTracker.gameObject.SetActive(true);
        }
        else noEyeTracker.gameObject.SetActive(true);
        StartCoroutine(FadeOutCoroutine());
        while (isFading)
        {
            yield return new WaitForEndOfFrame();
        }
        if(tobiiConnected) yield return new WaitForSeconds(3);
        else
        {
            while (waitForButton)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        isEnding = true;
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        while (isFading)
        {
            yield return new WaitForEndOfFrame();
        }
        isFading = true;
        Color savedColor = fadeImg.color;
        savedColor.a = 1;
        fadeImg.color = savedColor;
        for (float i = 1; i > 0; i -= Time.deltaTime/fadeDuration)
        {
            // set color with i as alpha
            savedColor.a = i;
            fadeImg.color = savedColor;
            yield return new WaitForEndOfFrame();
        }
        savedColor.a = 0;
        fadeImg.color = savedColor;
        yield return new WaitForSeconds(waitBefore);
        isFading = false;
    }

    IEnumerator FadeInCoroutine()
    {
        while (isFading)
        {
            yield return new WaitForEndOfFrame();
        }
        isFading = true;
        Color savedColor = fadeImg.color;
        savedColor.a = 0;
        fadeImg.color = savedColor;

        for (float i = 0; i < fadeDuration; i += Time.deltaTime/fadeDuration)
        {
            // set color with i as alpha
            savedColor.a = i;
            fadeImg.color = savedColor;
            yield return new WaitForEndOfFrame();
        }
        savedColor.a = 1;
        fadeImg.color = savedColor;
        if (scene == -1)
        {
            AkSoundEngine.PostEvent("Stop_Random_Track2", gameObject);
            yield return new WaitForSeconds(waitBefore);
            Application.Quit();
        }
        else if(isEnding)SceneManager.LoadScene(scene);
        isFading = false;
        yield return null;
    }
}
