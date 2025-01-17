using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

// Serves as Back up Manager for timer
public class BackupTimerManager : PunRaiseEvents
{
    // Public Variables
    [Header("Set Timer")]
    [Range(0, 59)] public int Minutes;      // Set Number of Minutes
    [Range(0, 59)] public int Seconds;      // Set Number of Seconds

    [Header("References")]
    public TextMeshProUGUI TimerText;       // UI Timer Text
    public TextMeshProUGUI CountdownText;   // UI Countdown Text

    [Header("Set Countdown and Last Minute Mechanic")]
    public float CountdownTimer;            // Set Timer for Initial Countdown
    public float SetLastMinute;             // Set Time Portion where Last Minute will be announced

    // Private Variables
    private float gameTime;
    private float currentCountdownTime;
    private float currentTime;
    private bool isLastMinute;
    private TimeSpan timer;

    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        EventManager.Instance.EndGame -= TimeUp;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize variables
        currentCountdownTime = CountdownTimer;

        CountdownText = GameManager.Instance.CountdownText;
        TimerText = GameManager.Instance.TimerText;

        EventManager.Instance.EndGame += TimeUp;

        gameTime = (Minutes * 60) + Seconds;         // Add all timer inputs for Minutes and Seconds
        currentTime = gameTime;                      // Set Current Time
        isLastMinute = false;                        // Last Minute Phase Deactivated

        CallRaiseEvent();
    }

    public override void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)RaiseEvents.INITIAL_COUNTDOWN:
                object[] data = (object[])photonEvent.CustomData;
                StartCoroutine(InitiateCountdown());
                break;

            case (byte)RaiseEvents.TIMER:
                StartCoroutine(Timer());
                break;

            case (byte)RaiseEvents.TIME_UP:
                StartCoroutine(TimeOver());
                break;
        }
    }
    public override void SetRaiseEvent()
    {
        // event data
        object[] data = new object[] { };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOption = new SendOptions
        {
            Reliability = false
        };

        // Call Raise Event based on Game State
        switch (GameManager.Instance.GameState)
        {
            case GameManager.GameStates.INITIAL:
                PhotonNetwork.RaiseEvent((byte)RaiseEvents.INITIAL_COUNTDOWN, data, raiseEventOptions, sendOption);
                break;

            case GameManager.GameStates.PLAYING:
                PhotonNetwork.RaiseEvent((byte)RaiseEvents.TIMER, data, raiseEventOptions, sendOption);
                break;

            case GameManager.GameStates.GAME_OVER:
                PhotonNetwork.RaiseEvent((byte)RaiseEvents.TIME_UP, data, raiseEventOptions, sendOption);
                break;
        }
    }

    // Start Countdown
    IEnumerator InitiateCountdown()
    {
        // 1-second delay at Startup
        if (currentCountdownTime == CountdownTimer)
        {
            yield return new WaitForSeconds(1f);
        }

        while (currentCountdownTime > 0f)
        {
            // Visual indication of Countdown
            CountdownText.text = currentCountdownTime.ToString("0");
            yield return new WaitForSeconds(1f);

            // Decrement currentCountdownTime variable
            currentCountdownTime--;
        }

        // Game Officially Starts
        // Proceed to Timer Function
        CountdownText.text = "GO!";

        // Find All Player Game Object Prefabs
        //GameManager.Instance.PlayerGameObjects = GameObject.FindGameObjectsWithTag("Player");

        yield return new WaitForSeconds(1f);

        GameManager.Instance.GameState = GameManager.GameStates.PLAYING;
        EventManager.Instance.InitiateGame.Invoke();

        // Show Score Panel while maintaining TimePanel
        PanelManager.Instance.ActivatePanel("score-panel");
        GameManager.Instance.TimePanel.SetActive(true);

        // Play BGM
        AudioManager.Instance.Play("game-bgm");

        CountdownText.text = "";

        CallRaiseEvent();
    }

    // Timer
    // Decrement currentTime every 1 second
    IEnumerator Timer()
    {
        while (currentTime > 0f)
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

            if (currentTime <= 120 && !isLastMinute)
            {
                StartCoroutine(LastMinute());
                Debug.Log("LAST 2 MINUTES");
            }
            else if (currentTime <= 0f)
            {
                // Time's Up
                // Call TimeOver Function
                GameManager.Instance.GameState = GameManager.GameStates.GAME_OVER;
                CallRaiseEvent();
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }

        CallRaiseEvent();
    }

    // Announces Last Minute
    // Intensify Game Atmosphere
    IEnumerator LastMinute()
    {
        // Announcement Text
        CountdownText.text = "LAST 2 MINUTES!";
        isLastMinute = true;

        // Stop BGM and Change the Pitch
        AudioManager.Instance.Stop("game-bgm");
        AudioManager.Instance.ModifyPitch("game-bgm", 1.25f);

        yield return new WaitForSeconds(2f);

        // Play BGM with Higher Pitch
        AudioManager.Instance.Play("game-bgm");

        CountdownText.text = "";

        // Intensity Atmosphere at Last Minute
        EventManager.Instance.Intensify.Invoke();
    }

    // Called when Timer reaches 0
    IEnumerator TimeOver()
    {
        Debug.Log("Time's Up!");

        // Declare Time Over
        CountdownText.text = "Time's Up!";

        EventManager.Instance.EndGame.Invoke();

        yield return new WaitForSeconds(2.0f);

        CountdownText.text = "";

        PanelManager.Instance.ActivatePanel("results-panel");
        ScoreManager.Instance.SortScore();
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

    void TimeUp()
    {
        currentTime = 0;
        timer = TimeSpan.FromSeconds(currentTime);
        TimerText.text = timer.Minutes.ToString("00") + ":" + timer.Seconds.ToString("00");
    }
}
