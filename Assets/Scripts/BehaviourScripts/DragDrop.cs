using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [SerializeField] public TextMeshProUGUI proverbText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
        GetComponent<RectTransform>().anchoredPosition;

        string draggedButtonText = eventData.pointerDrag.GetComponentInChildren<TextMeshProUGUI>().text;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(proverbText, Input.mousePosition, null);
        string[] splits = proverbText.text.Split(" ");
        splits[wordIndex] = draggedButtonText;
        proverbText.text = string.Join(" ", splits);
        Destroy(eventData.pointerDrag, 0);

        //proverbText.textInfo.wordInfo[wordIndex].textComponent.text = "AAAA";
        //Debug.Log(proverbText.textInfo);
    }
}
