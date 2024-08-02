using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsPeek : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] GameObject cardsGrid;
    [SerializeField] GameObject cardElementPrefab;
    [SerializeField] GameObject emptyText;
    [SerializeField] TMP_Text instruction;

    private bool runOut=false;

    void Awake(){
        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });
    }

    void Start()
    {   
        TimerManager.Instance.StartMove();
        TimerManager.Instance.StartInitialTimer("Next",3f,nextButton);

        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(nextButton);

        RoleBehaviour roleBehaviour = GamePlayer.Instance.Role.Behaviour;
        List<RoleBehaviour.RevealingCard> cards = roleBehaviour.RevealingCards;
        instruction.text=roleBehaviour.MoveInst;

        if (cards.Count==0){
            emptyText.SetActive(true);
            return;
        }

        foreach (RoleBehaviour.RevealingCard card in cards){
            GameObject o = DisplayManager.InstantiateWithParent(cardElementPrefab, cardsGrid);
            o.GetComponentInChildren<Image>().sprite = card.Image;
            o.GetComponentInChildren<TMP_Text>().text = card.PlayerName;
        }
        cardsGrid.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 500*(cards.Count/2 + cards.Count%2)+100 );
    }

    void Update(){
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()) {
            runOut = true;
            DisplayManager.GoToNextScene();
        }
    }

}
