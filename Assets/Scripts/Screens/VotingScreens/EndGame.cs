using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    [SerializeField] Button newGameButton;
    [SerializeField] TMP_Text title;
    [SerializeField] Image myCard;
    [SerializeField] GameObject scroll;
    [SerializeField] GameObject playerCardPrefab;
    [SerializeField] Image background;
    

    void Start()
    {
        FindObjectOfType<WiFiManager>().AddToInteractables(newGameButton);

        newGameButton.onClick.AddListener(()=>{
            DisplayManager.LeaveGame(background);
        });

        FillTitle();
        FillMyCard();
        FillCards();
    }

    void FillTitle(){
        RolesManager.Team winner = GameManager.Instance.WinnerTeam;
        RolesManager.Team myTeam = GamePlayer.Instance.Role.Behaviour.Team;
        if (myTeam==winner){
            title.text = "You win!";
            background.color=DisplayManager.WinColor;
        } else {
            title.text = "You lose :(";
            background.color=DisplayManager.LossColor;
        }
    }

    void FillMyCard(){
        Role card = GamePlayer.Instance.Role;
        myCard.sprite = card.GetImage();
    }

    void FillCards(){
        int cnt =0;
        foreach(KeyValuePair<string,RolesManager.CardName> kp in GameManager.Instance.playerCards){
            string player = kp.Key;
            RolesManager.CardName card = kp.Value;

            if (player==GamePlayer.Instance.Id) continue;
            
            GameObject cardObject = DisplayManager.InstantiateWithParent(playerCardPrefab, scroll);
            cardObject.GetComponentInChildren<TMP_Text>().text = GameManager.Instance.playerNames[player];
            cardObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.roleInstances[card].GetImage();
            cnt++;
        }
        
        scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(500, 500*(cnt/2 + cnt%2)+100) );
    }

}
