using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VotingResult : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] Image votedForCard;
    [SerializeField] TMP_Text votedForName;
    [SerializeField] GameObject scroll;
    [SerializeField] GameObject playerCardPrefab;
    [SerializeField] GameObject noneOut;
    [SerializeField] GameObject noneVotedFor;
    [SerializeField] Image background;


    private bool runOut = false;

    void Start()
    {
        TimerManager.Instance.StartResultScreen();

        FindObjectOfType<WiFiManager>().AddToInteractables(nextButton);

        FillVotedFor();
        FillOut();
        if (ThisPlayerOut()){
            background.color = DisplayManager.LossColor;
            nextButton.GetComponentInChildren<TMP_Text>().text="Leave game";
        }
        
        nextButton.onClick.AddListener(()=>{
            if (ThisPlayerOut()){
                DisplayManager.LeaveGame(background);
            } else {
                DisplayManager.GoToNextScene(nextButton);
            }
        });
    }

    private void FillVotedFor(){
        List<string> votedList = GameManager.Instance.LastVotedOutList;
        if (votedList.Count>1) {
            noneVotedFor.SetActive(true);
            votedForCard.gameObject.SetActive(false);
            votedForName.gameObject.SetActive(false);
            return;
        }

        string votedName = GameManager.Instance.LastVotedOutName;
        foreach(KeyValuePair<string, RolesManager.CardName> kp in GameManager.Instance.lastPlayersOut){
            if (kp.Key==votedName){
                votedForCard.sprite=GameManager.Instance.roleInstances[kp.Value].GetImage();
                votedForName.text=votedName;
                return;
            }
        }
        RolesManager.CardName card = GameManager.Instance.playerCards[votedList[0]];
        votedForCard.sprite=GameManager.Instance.roleInstances[card].GetImage();
        votedForName.text=GameManager.Instance.playerNames[votedList[0]];
    }

    private void FillOut(){
        if (GameManager.Instance.lastPlayersOut.Count==0){
            noneOut.SetActive(true);
            return;
        }
        int cnt=0;
        foreach(KeyValuePair<string,RolesManager.CardName> kp in GameManager.Instance.lastPlayersOut){
            string playerName = kp.Key;
            RolesManager.CardName card = kp.Value;
            
            GameObject cardObject = DisplayManager.InstantiateWithParent(playerCardPrefab, scroll);
            cardObject.GetComponentInChildren<TMP_Text>().text = playerName;
            cardObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.roleInstances[card].GetImage();
            cnt++;
        }
        
        scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(500, 500*(cnt/2 + cnt%2)+100) );
    }

    private bool ThisPlayerOut(){
        foreach(string name in GameManager.Instance.lastPlayersOut.Keys){
            if(name==GamePlayer.Instance.Name) return true;
        }
        return false;
    }

    void Update()
    {
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()){
            runOut=true;
            DisplayManager.GoToNextScene();
        }
    }
}
