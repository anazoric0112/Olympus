using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
}
