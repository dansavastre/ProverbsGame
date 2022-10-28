using System.Linq;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IDropHandler 
{
    public PhotonView _photon;

    /// <summary>
    /// Executed when the user drops the element.
    /// </summary>
    /// <param name="eventData">A pointer to the event data of the drag-drop action.</param>
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
            GetComponent<RectTransform>().anchoredPosition;

        string draggedButtonText = eventData.pointerDrag.GetComponentInChildren<TextMeshProUGUI>().text;
        string buttonText = GetComponentInChildren<TextMeshProUGUI>().text;
        CoopGame.allWords.Remove(draggedButtonText);
        CoopGame.buttonIndices[CoopGame.buttonIndices.IndexOf(draggedButtonText)] = "";
        Destroy(eventData.pointerDrag.GetComponent<Button>().GameObject());
        SendChat(draggedButtonText, buttonText);
    }
    
    /// <summary>
    /// Method for sending a message to a certain player via the chat.
    /// </summary>
    /// <param name="msg">String denoting the message to be sent.</param>
    /// <param name="player">String denoting the username of the player that the message should be sent to.</param>
    public void SendChat(string msg, string player)
    {
        _photon.RPC("ReceiveChat", PhotonNetwork.PlayerList.First(p => p.NickName.Equals(player)), msg);
    }
}