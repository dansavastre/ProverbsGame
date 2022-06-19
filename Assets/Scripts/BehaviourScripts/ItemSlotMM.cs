using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotMM : MonoBehaviour, IDropHandler 
{

    public PhotonView _photon;
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
            GetComponent<RectTransform>().anchoredPosition;

        string draggedButtonText = eventData.pointerDrag.GetComponentInChildren<TextMeshProUGUI>().text;
        string buttonText = GetComponentInChildren<TextMeshProUGUI>().text;
        MeaningMatchingGame.buttonIndices[MeaningMatchingGame.buttonIndices.IndexOf(draggedButtonText)] = "";
        Destroy(eventData.pointerDrag.GetComponent<Button>().GameObject());
        SendChat(draggedButtonText, buttonText);
    }
    
    public void SendChat(string msg, string player)
    {
        Debug.Log(PhotonNetwork.PlayerList.Length);
        _photon.RPC("ReceiveChat", PhotonNetwork.PlayerList.First(p => p.NickName.Equals(player)), msg);
    }
}
