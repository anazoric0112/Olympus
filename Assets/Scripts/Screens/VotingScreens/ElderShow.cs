using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElderShow : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] Image cardImage;
    [SerializeField] TMP_Text nameText;
    
    private bool runOut = false;
    
    void Start()
    {
        TimerManager.Instance.StartMove();
        TimerManager.Instance.StartInitialTimer("Next", 3, nextButton);
        
        FindObjectOfType<WiFiManager>().AddToInteractables(nextButton);
        
        string id = GameManager.Instance.NextElderShow;
        RolesManager.CardName card = GameManager.Instance.playerCards[id];
        cardImage.sprite = GameManager.Instance.roleInstances[card].GetImage();
        nameText.text = GameManager.Instance.playerNames[id];

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });
    }

    void Update()
    {
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()){
            runOut=true;
            DisplayManager.GoToNextScene(nextButton);
        }
    }
}
