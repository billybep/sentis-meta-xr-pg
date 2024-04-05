using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FingerDrawing : MonoBehaviour
{
    [SerializeField] private RawImage displayImage;
    [SerializeField] private ClassifyHandwrittenDigit classifier;
    [SerializeField] private Transform fingerTipMarkerTransform;
    [SerializeField] private float delayToSend = 1f;
    [SerializeField] private float distanceToCanvas = 0.07f;

    private bool hasDrawn = false;
    private float lastDrawnTime;
    private Camera mainCamera;
    private Texture2D drawingTexture;
    private Coroutine checkForSendCoroutine;

    private void Start()
    {
        drawingTexture = new Texture2D(28, 28, TextureFormat.RG32, false);
        displayImage.texture = drawingTexture;
        mainCamera = Camera.main;
        // ClearTexture();
    }

    private void Update()
    {
        bool isDrawing = Vector3
            .Distance(fingerTipMarkerTransform.position, displayImage.transform.position) < distanceToCanvas;
        
        if (isDrawing)
        {
            if (checkForSendCoroutine != null)
            {
                StopCoroutine(checkForSendCoroutine);
                checkForSendCoroutine = null;
            }

            Draw(fingerTipMarkerTransform.position);
            hasDrawn = true;
            lastDrawnTime = Time.time;
        }
        else if (hasDrawn && Time.time - lastDrawnTime > delayToSend && checkForSendCoroutine == null)
        {
            checkForSendCoroutine = StartCoroutine(checkForSend());
        }
    }

    private void Draw(Vector3 fingerTipPos)
    {
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(fingerTipPos);
        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(displayImage.rectTransform, screenPoint, mainCamera, out Vector2 localPoint);
        Vector2 normalizePoint = Rect.PointToNormalized(displayImage.rectTransform.rect, localPoint);

        AddPixels(normalizePoint);
    }

    private void AddPixels(Vector3 normalizePoint)
    {
        int texX = (int)(normalizePoint.x * drawingTexture.width);
        int texY = (int)(normalizePoint.y * drawingTexture.height);

        if (texX >= 0 && texY < drawingTexture.width && texX >= 0 && texY < drawingTexture.height)
        {
            drawingTexture.SetPixel(texX, texY, Color.white);
            drawingTexture.Apply();
        }
    }

    private IEnumerator checkForSend()
    {
        yield return new WaitForSeconds(delayToSend);
        classifier.ExecuteModel(drawingTexture);
        hasDrawn = false;
        checkForSendCoroutine = null;
    }

    public void ClearTexture()
    {
        Color[] clearColors = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < clearColors.Length; i++)
            clearColors[i]= Color.black;
        
        drawingTexture.SetPixels(clearColors);
        drawingTexture.Apply();
    }
}
