using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TimerManager : PunRaiseEvents
{    
    // Public Variables
    [Header("Set Timer")]
    [Range(0, 59)] public int Minutes;      // Set Number of Minutes
    [Range(0, 59)] public int Seconds;      // Set Number of Seconds

    [Header("References")]
    public TextMeshProUGUI TimerText;       // UI Timer Text
    public TextMeshProUGUI CountdownText;   // UI Countdown Text
    public ParticleSystem ArenaAesthetic;   // Arena Particle System (Aesthetic)

    [Header("Set Countdown and Last Minute Mechanic")]
    public float CountdownTimer;            // Set Timer for Initial Countdown
    public float SetLastMinute;             // Set Time Portion where Last Minute will be announced

    // Private Variables
    private float currentCountdownTime;
    private float currentTime;
    private bool isActive;
    private bool isLastMinute;
    private TimeSpan timer;

    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public override void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.InitCountdownEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            StartCoroutine(InitiateCountdown());
        }
        else if (photonEvent.Code == (byte)RaiseEventsCode.TimerEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            StartCoroutine(Timer());

            // Last Minute
            if (currentTime <= SetLastMinute && !isLastMinute)
            {
                StartCoroutine(LastMinute());
            }
        }
        else if (photonEvent.Code == (byte)RaiseEventsCode.TimeOverEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            StartCoroutine(TimeOver());
        }
    }

    public override void SetRaiseEvent()
    {
        // event data
        object[] data = new object[] {  };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOption = new SendOptions
        {
            Reliability = false
        };

        // Call RaiseEvents
        // Initial Countdown RaiseEvent
        if (currentCountdownTime >= 0 && !isActive)
        {
            PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.InitCountdownEventCode, data, raiseEventOptions, sendOption);
        }
        // Timer RaiseEvent
        else if (currentTime > 0 && isActive)
        {
            PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.TimerEventCode, data, raiseEventOptions, sendOption);
        }
        // Time Over RaiseEvent
        else
        {
            PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.TimeOverEventCode, data, raiseEventOptions, sendOption);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize variables
        currentCountdownTime = CountdownTimer + 1;      // Add 1 for a 1 second Delay before initiating
        currentTime = (Minutes * 60) + Seconds;         // Add all timer inputs for Minutes and Seconds
        isActive = false;                               // Timer Deactivated
        isLastMinute = false;                           // Last Minute Phase Deactivated

        // Call Initiate Countdown Function
        CallRaiseEvent();
    }

    // Start Countdown
    IEnumerator InitiateCountdown()
    {
        // Decrement currentCountdownTime variable
        currentCountdownTime--;
        
        yield return new WaitForSeconds(1f);

        // Visual indication of Countdown
        CountdownText.text = currentCountdownTime.ToString("0");

        // Game Officially Starts
        // Announce "GO!!!"
        // Proceed to Timer Function
        if (currentCountdownTime <= 0f)
        {
            Debug.Log("Go!!!");
            CountdownText.text = "GO!";
            yield return new WaitForSeconds(1f);
            
            CountdownText.text = "";
            isActive = true;

            CallRaiseEvent();

            // Terminate Cycle
            yield break;
        }

        // Continue Decrement Cycle
        CallRaiseEvent();
    }

    // Timer
    // Decrement currentTime every 1 second
    IEnumerator Timer()
    {
        currentTime--;

        // Convert currentTime float to Minutes/Seconds Form
        timer = TimeSpan.FromSeconds(currentTime);

        // Final Countdown of 10 Seconds
        if (currentTime > 0f && currentTime <= 10f)
        {
            CountdownText.text = timer.Seconds.ToString("0");
        }
        
        // Print Timer in Minutes/Seconds Form
        TimerText.text = timer.Minutes.ToString("00") + ":" + timer.Seconds.ToString("00");

        // Time Up
        // Call TimeOver Function
        if (currentTime <= 0)
        {
            CallRaiseEvent();

            // Terminate Cycle
            yield break;
        }

        yield return new WaitForSeconds(1f);

        // Continue Decrement Cycle
        CallRaiseEvent();
    }

    // Announces Last Minute
    // Intensify Game Atmosphere
    IEnumerator LastMinute()
    {
        // Announcement Text
        CountdownText.text = "LAST 2 MINUTES!";
        isLastMinute = true;

        yield return new WaitForSeconds(2f);
        
        CountdownText.text = "";

        // Faster Movement Speed of Arena Aesthetic Particles
        var main = ArenaAesthetic.main;
        main.simulationSpeed = 4f;
    }

    // Called when Timer reaches 0
    IEnumerator TimeOver()
    {
        Debug.Log("Time's Up!");

        // Declare Time Over
        CountdownText.text = "Time's Up!";
        isActive = false;

        yield return new WaitForSeconds(2.0f);

        CountdownText.text = "";
        Debug.Log("Declare Winner!");
    }

    // Call Raise Event only once
    void CallRaiseEvent()
    { 
        if (photonView.IsMine)
        {
            SetRaiseEvent();
        }
    }
}