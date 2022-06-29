using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PunRaiseEvents : MonoBehaviourPunCallbacks
{
    public enum RaiseEventsCode
    {
        InitCountdownEventCode = 0,
        TimerEventCode = 1,
        TimeOverEventCode = 2
    }

    public virtual void OnEnable()
    {
    
    }

    public virtual void OnDisable()
    {

    }

    public virtual void OnEvent(EventData photonEvent)
    {

    }

    public virtual void SetRaiseEvent()
    {
        
    }
}