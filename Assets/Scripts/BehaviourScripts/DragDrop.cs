using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
<<<<<<< HEAD
using TMPro;
=======
using UnityEngine.UI;
>>>>>>> 64b14b06d40ef2e3153cff488bd815c754bfc216

public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Vector3 startingPosition;

    [SerializeField] public TextMeshProUGUI proverbText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Snap button back to starting position if not placed on a player
        List<RaycastResult> result = RaycastMouse();
        if (result.Count > 1)
        {
            foreach (var r in result)
            {
                if (r.gameObject.GetComponentInChildren<TextMeshProUGUI>().text.Contains("Player"))
                {
                    return;
                }
                GetComponent<RectTransform>().anchoredPosition = startingPosition;
            }
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = startingPosition;
        }
    }
    
    public List<RaycastResult> RaycastMouse(){
         
        PointerEventData pointerData = new PointerEventData (EventSystem.current)
        {
            pointerId = -1,
        };
         
        pointerData.position = Input.mousePosition;
 
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results;
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
