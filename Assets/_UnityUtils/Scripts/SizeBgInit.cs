using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SizeBgInit : MonoBehaviour
{
    [SerializeField] private RectTransform bg;
    [SerializeField] private CanvasScaler canvasScaler;

    private void Awake()
    {
        var sizeInit = bg.sizeDelta;
        var sizeCv = canvasScaler.referenceResolution;
        int w = Screen.width;
        int h = Screen.height;

        if (w <= h)
        {
            float ratio = sizeInit.y / sizeCv.y;
            bg.sizeDelta = new Vector2(sizeInit.x / ratio, sizeCv.y);
            
        }
        else
        {
            float ratio = sizeInit.x / sizeCv.x;
            bg.sizeDelta = new Vector2(sizeCv.x, sizeInit.y / ratio);
        }
        
        if ((float)h / w > sizeCv.y / sizeCv.x)
        {
            bg.sizeDelta *= (sizeCv.x * h) / (w * sizeCv.y);
        }
        else
        {
            bg.sizeDelta *=  (w * sizeCv.y)  / (sizeCv.x * h);
        }
        
        bg.anchoredPosition = new Vector2(0, (sizeInit.y - bg.sizeDelta.y)/2);
        
    }
}
