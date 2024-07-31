using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    [SerializeField] Image timerTop;
    [SerializeField] Image timerBottom;
    private float moveTime = 20;
    private float roleDisplayTime = 30;
    private float discussionTime = 60*5;
    private float votingTime = 30;
    private float resultTime = 60;
    private float current = 0;
    private float currentMax = 0;
    private bool running = false;
    private float savedTime = -1;


    //initial button-unclickable time for moves
    private bool initialOver = true;
    private float initialTimer = -1;
    private string buttonText = "";
    private Button button;


    
    static TimerManager instance;
    static public TimerManager Instance {
        get { return instance; }
    }

    void Awake(){
        if (instance){
            gameObject.SetActive(false);
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        UpdateInitialTimer();
        UpdateTimer();
    }

    private void UpdateTimer(){
        current -= Time.deltaTime;
        if (current<0f){
            running = false;
        } 
        timerTop.fillAmount = GetFill();
        timerBottom.fillAmount = 1-GetFill();
    }

    public bool IsRunOut(){
        return !running;
    }

    public void StartMove(){
        current = currentMax = moveTime;
        running=true;
    }

    public void StartRoleDisplay(){
        current = currentMax = roleDisplayTime;
        running=true;
    }

    public void StartDiscussion(){
        current = currentMax = discussionTime;
        running=true;
    }
    
    public void StartVote(){
        current = currentMax = votingTime;
        running=true;
    }

    public void StartResultScreen(){
        current = currentMax = resultTime;
        running=true;
    }

    public float GetFill(){
        if (current<0) return 0;
        return current/currentMax;
    }

    public void SaveTime(){
        savedTime=current;
    }

    public void RestoreSavedTime(){
        if (savedTime==-1) return;
        
        current=savedTime;
        savedTime=-1;
    }

    public int GetMinutes(){
        double currentRound = Math.Round(current);
        return (int)Math.Floor(currentRound/60);
    }
    
    public int GetSeconds(){
        int currentRound = (int)Math.Round(current);
        return currentRound%60;
    }

    //-----------------Initial timer functions------------------
    public void StartInitialTimer(string text, float time, Button btn){
        buttonText=text;
        button=btn;
        initialTimer = time;
        initialOver = false;
        button.interactable=false;
        SetButtonText(initialTimer.ToString());
    }
    private void UpdateInitialTimer(){
        initialTimer-=Time.deltaTime;
        
        if ((int)initialTimer>=0 && !initialOver){
            SetButtonText(((int)initialTimer).ToString());
        } else if (!initialOver){
            button.interactable=true;
            SetButtonText(buttonText);
            initialOver=true;
        }
    }
    private void SetButtonText(string t){
        button.GetComponentInChildren<TMP_Text>().text=t;
    }
}
