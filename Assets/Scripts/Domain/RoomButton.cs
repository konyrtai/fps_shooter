using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    /// <summary>
    /// Отображение названия комнаты
    /// </summary>
    public TMP_Text buttonText;
    
    /// <summary>
    /// Информация о комнате из PUN
    /// </summary>
    private RoomInfo info;

    /// <summary>
    /// Детали комнаты
    /// </summary>
    /// <param name="inputInfo"></param>
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;
        buttonText.text = inputInfo.Name;
    }

    /// <summary>
    /// Подключиться к комнате
    /// </summary>
    public void JoinRoom()
    {
        if(info == null)
            return;
        
        LauncherManager.instance.JoinRoom(info);
    }
}
