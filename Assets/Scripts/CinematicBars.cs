using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicBars : MonoBehaviour
{
    private RectTransform topBar, bottomBar;
    private float targetSize;
    private float changeSizeAmount;
    private bool isActive;

    [SerializeField] Color barColor;

    private void Awake()
    {
        GameObject gameObject = new GameObject("topBar", typeof(Image));
        gameObject.transform.SetParent(transform, false);
        gameObject.GetComponent<Image>().color = barColor;
        topBar = gameObject.GetComponent<RectTransform>();
        topBar.anchorMin = new Vector2(0, 1);
        topBar.anchorMax = new Vector2(1, 1);
        topBar.sizeDelta = new Vector2(0, 300);

        gameObject = new GameObject("bottomBar", typeof(Image));
        gameObject.transform.SetParent(transform, false);
        gameObject.GetComponent<Image>().color = barColor;
        bottomBar = gameObject.GetComponent<RectTransform>();
        bottomBar.anchorMin = new Vector2(0, 0);
        bottomBar.anchorMax = new Vector2(1, 0);
        bottomBar.sizeDelta = new Vector2(0, 300);
        /*
        CMDebug.ButtonUI(new Vector2(200,0), "Show" ,() => 
        {
            Show(300, .3f);
        }
        */
    }

    private void Start()
    {
        Hide(0);
    }

    private void Update()
    {
        if (isActive)
        {
            Vector2 sizeDelta = topBar.sizeDelta;
            sizeDelta.y += changeSizeAmount * Time.deltaTime;
            if (changeSizeAmount > 0)
            {
                if (sizeDelta.y >= targetSize)
                {
                    sizeDelta.y = targetSize;
                    isActive = false;
                }
            }
            else
            {
                if (sizeDelta.y <= targetSize)
                {
                    sizeDelta.y = targetSize;
                    isActive = false;
                }
            }
            topBar.sizeDelta = sizeDelta;
            bottomBar.sizeDelta = sizeDelta;
        }
    }

    public void Show(float targetSize, float time)
    {
        this.targetSize = targetSize;
        changeSizeAmount = (targetSize - topBar.sizeDelta.y) / time;
        isActive = true;
    }

    public void Hide(float time)
    {
        targetSize = 0f;
        changeSizeAmount = (targetSize - topBar.sizeDelta.y) / time;
        isActive = true;
    }
}
