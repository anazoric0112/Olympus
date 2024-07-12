using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Discussion : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] TMP_Text timerText;

    private bool runOut=false;

    // void Awake(){
    //     TimerManager.Instance.StartDiscussion();
    // }

    void Start(){
        FindObjectOfType<WiFiManager>().AddToInteractables(nextButton);

        TimerManager.Instance.StartDiscussion();
        UpdateTimerText();

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });
    }

    void Update(){   
        UpdateTimerText();
        
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()){
            runOut=true;
            SceneManager.LoadScene((int)DisplayManager.Scenes.Voting);
        }
    }

    void UpdateTimerText(){
        string seconds = TimerManager.Instance.GetSeconds().ToString();
        if (seconds.Length==1) seconds="0"+seconds;
        
        timerText.text="0"+TimerManager.Instance.GetMinutes().ToString()+":"+seconds;
    }
}
