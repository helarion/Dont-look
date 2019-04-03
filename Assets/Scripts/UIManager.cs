using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image fadeImg = null;
    [SerializeField] float fadeTime=0.5f;
    public GameObject pausePanel = null;
    [SerializeField] Image pauseImg = null;
    [SerializeField] GameObject ControlPanel=null;
    [SerializeField] GameObject NoEyePanel=null;

    public bool isFading = false;

    public static UIManager instance = null;

    void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!GameManager.instance.isTesting)
        {
            fadeImg.color = new Color(0, 0, 0, 1);
            GameManager.instance.player.SetIsAlive(false);
            FadeOut(fadeImg, 1, 0);
            StartCoroutine("StartCoroutine");
        }
    }

    IEnumerator StartCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        GameManager.instance.player.SetIsAlive(true);
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
        if(b)FadeIn(fadeImg, 1, 1);
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
}
