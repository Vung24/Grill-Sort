using System;
using UnityEngine;
using UnityEngine.UI;

public class DEV_SCREENSHOT : MonoBehaviour
{
    [SerializeField] private Camera renderCamera;   
    [SerializeField] private RawImage rawImage;
    [SerializeField] private RenderTexture renderTexture;

    private void Start()
    {
        rawImage.texture = renderTexture;
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();
    }
}
