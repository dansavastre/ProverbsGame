using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropMM : DragDrop, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public string initialText;
    public Vector2 initialSize;
    public MeaningMatchingGame meaningMatchingGame;
    public Sprite correctAnswerGivenSprite;

    public void OnPointerUp(PointerEventData eventData)
    {
        // Snap button back to starting position if not placed on a player
        List<RaycastResult> result = RaycastMouse();
        if (result.Count > 1)
        {
            foreach (var r in result)
            {
                if (r.gameObject.GetComponentInChildren<TextMeshProUGUI>().text.Contains(MeaningMatchingGame.currentMeaning))
                {
                    if (initialText.Equals(MeaningMatchingGame.correctProverb))
                    {
                        resetComponent();
                        GetComponent<Image>().sprite = correctAnswerGivenSprite;
                        GetComponent<Button>().interactable = false;
                        meaningMatchingGame.correctAnswerObjectToRemove = this.GameObject();
                        meaningMatchingGame.Answer(true);
                    }
                    else
                    {
                        resetComponent();
                        meaningMatchingGame.Answer(false);
                    }
                    return;
                }
                resetComponent();
            }
        }
        base.OnPointerUp(eventData);
        if (GetComponent<RectTransform>().anchoredPosition.Equals(startingPosition))
        {
            resetComponent();
        }
    }

    private void resetComponent()
    {
        GetComponent<RectTransform>().anchoredPosition = startingPosition;
        GetComponentInChildren<TextMeshProUGUI>().text = initialText;
        GetComponent<RectTransform>().sizeDelta = initialSize;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = "...";
        GetComponent<RectTransform>().sizeDelta = new Vector2(255, 100);
        GetComponent<RectTransform>().position = eventData.position;
        base.OnBeginDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}