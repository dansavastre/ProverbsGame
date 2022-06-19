using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomListItem : MonoBehaviour {
    [SerializeField] TMP_Text text;

    public RoomInfo info;

    public void SetUp(RoomInfo _info) {
        info = _info;
        text.text = _info.Name;
    }

    private bool IsFIBScene() {
        return SceneManager.GetActiveScene().name == "FillInBlanks";
    }

    private bool IsMMScene() {
        return !IsFIBScene();
    }

    public void OnClick() {
        if (IsFIBScene())
            Launcher_FIB.Instance.JoinRoom(info);
        else
            Launcher_MM.Instance.JoinRoom(info);
    }
}
