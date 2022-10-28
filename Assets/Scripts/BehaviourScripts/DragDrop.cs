using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Canvas canvas;
    public Vector3 startingPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [SerializeField] public TextMeshProUGUI proverbText;

    /// <summary>
    /// Executed when an instance of this class is initialized.
    /// </summary>
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Executed when the user clicks.
    /// </summary>
    /// <param name="eventData">A pointer to the event data of the click action.</param>
    public void OnPointerDown(PointerEventData eventData) { }

    /// <summary>
    /// Executed when the user releases the click.
    /// </summary>
    /// <param name="eventData">A pointer to the event data of the click action.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Snap button back to starting position if not placed on a player
        List<RaycastResult> result = RaycastMouse();
        if (result.Count > 1)
        {
            foreach (var r in result)
            {
                if (r.gameObject.GetComponentInChildren<TextMeshProUGUI>().text.Contains("Player")) return;
                GetComponent<RectTransform>().anchoredPosition = startingPosition;
            }
        }
        else GetComponent<RectTransform>().anchoredPosition = startingPosition;
    }
    
    /// <summary>
    /// Method for raycasting the mouse position.
    /// </summary>
    /// <returns>a raycast result denoting the mouse position</returns>
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

    /// <summary>
    /// Executed whenever the user starts a drag action.
    /// </summary>
    /// <param name="eventData">A pointer to the event data of the drag action.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    /// <summary>
    /// Executes on each frame the user is performing a drag action.
    /// </summary>
    /// <param name="eventData">A pointer to the event data of the drag action.</param>
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    /// <summary>
    /// Executed whenever the user ends a drag action.
    /// </summary>
    /// <param name="eventData">A pointer to the event data of the drag action.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
        GetComponent<RectTransform>().anchoredPosition;

        string draggedButtonText = eventData.pointerDrag.GetComponentInChildren<TextMeshProUGUI>().text;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(proverbText, Input.mousePosition, null);
        string[] splits = proverbText.text.Split(" ");

        if ((wordIndex > -1) && (splits[wordIndex].Contains("<u><alpha=#00>xxxxx</color></u>")))
        {
            Debug.Log(splits[wordIndex]);
            splits[wordIndex] = Regex.Replace(splits[wordIndex], "<u><alpha=#00>xxxxx</color></u>", draggedButtonText, RegexOptions.IgnoreCase);
            proverbText.text = string.Join(" ", splits);
            Destroy(eventData.pointerDrag, 0);
            CoopGame.buttonIndices[CoopGame.buttonIndices.IndexOf(draggedButtonText)] = "";
        }
    }
}