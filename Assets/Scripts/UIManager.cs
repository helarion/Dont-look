    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Toggle toggleTracker;

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
        volumeControl.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        if (!GameManager.instance.isTesting)
        {
            GameManager.instance.SetIsPaused(true);
            fadeImg.color = new Color(0, 0, 0, 1);
            FadeOut(fadeImg, 1, 0);
            StartCoroutine("StartCoroutine");
        }
    }

    public void ValueChangeCheck()
    {
        AkSoundEngine.SetRTPCValue("Master_Volume_Slider", volumeControl.value);
        Debug.Log(volumeControl.value);
    }

    IEnumerator StartCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        GameManager.instance.SetIsPaused(false);
    }

    IEnumerator FadeOutCoroutine(object[] param)
    {
        Image img = (Image)param[0];
        float duration = (float)param[1];
        float waitBefore = (float)param[2];
        Color savedColor = img.color;
        savedColor.a = 1;
        img.color = savedColor;
        while (isFading)
        {
            yield return new WaitForSeconds(0.1f);
        }
        isFading = true;
        yield return new WaitForSeconds(waitBefore);
        for (float i = duration; i > 0; i -= Time.deltaTime * fadeTime)
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

    IEnumerator FadeInCoroutine(object[] param)
    {
        Image img = (Image)param[0];
        float duration = (float)param[1];
        float waitBefore = (float)param[2];
        Color savedColor = img.color;
        savedColor.a = 0;
        img.color = savedColor;
        while (isFading)
        {
            yield return new WaitForSeconds(0.1f);
        }
        isFading = true;
        yield return new WaitForSeconds(waitBefore);
        for (float i = 0; i < duration; i += Time.deltaTime * fadeTime)
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

    public void FadeDeath(bool b)
    {
        if(b)FadeIn(fadeImg, 1, 0);
        else FadeOut(fadeImg, 1, 1);
    }

    public void FadePause(bool b)
    {
        if (b) FadeIn(pauseImg, 0.5f, 0);
        else FadeOut(pauseImg, 0.5f, 0);
    }

    public void FadeIn(Image img, float duration, float waitBefore)
    {
        object[] o = { img, duration, waitBefore };
        StartCoroutine("FadeInCoroutine",o);
    }

    public void FadeOut(Image img, float duration, float waitBefore)
    {
        object[] o = { img, duration, waitBefore };
        StartCoroutine("FadeOutCoroutine",o);
    }

    public void DisableControlPanel(bool b)
    {
        ControlPanel.SetActive(!b);
        NoEyePanel.SetActive(b);
    }

    public bool GetCheckTracker()
    {
        return toggleTracker.isOn;
    }
}
