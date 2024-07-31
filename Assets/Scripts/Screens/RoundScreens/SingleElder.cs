using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleElder : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] GameObject cardToShow;
    [SerializeField] GameObject emptyText;

    private bool runOut=false;
    private string playerId="";

    void Start()
    {
        TimerManager.Instance.StartMove();
        TimerManager.Instance.StartInitialTimer("Next",3f,nextButton);
        
        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(nextButton);

        playerId=GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Tartarus);

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });
        cardToShow.GetComponentInChildren<Button>().onClick.AddListener(()=>{
            ShowCursed(playerId);
        });
        cardToShow.GetComponentInChildren<Image>().sprite=DisplayManager.QuestionBack;
    }

    void Update(){
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()) {
            runOut = true;
            DisplayManager.GoToNextScene();
        }
    }

    void ShowCursed(string playerId){
        cardToShow.GetComponentInChildren<Button>().interactable=false;
        RolesManager.CardName card = GameManager.Instance.playerCards[playerId];
        
        DisplayManager.RotateCard(cardToShow.GetComponentInChildren<Image>().gameObject, cardToShow.GetComponentInChildren<Image>(), GameManager.Instance.roleInstances[card].GetImage());

        cardToShow.GetComponentInChildren<TMP_Text>().text = GameManager.Instance.playerNames[playerId];
        RPCsManager.Instance.SwapVotingMovesServerRpc();
    }
}
