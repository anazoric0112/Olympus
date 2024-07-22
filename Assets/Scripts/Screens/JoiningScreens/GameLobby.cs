using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLobby : MonoBehaviour
{   
    [SerializeField] Button nextButton;
    [SerializeField] Button backButton;
    [SerializeField] TMP_Text joinCodeText;

    [SerializeField] GameObject errorModal;
    [SerializeField] TMP_Text errorText;

    private ConnectionManager connectionManager;
    private bool leaving = false;

    void Awake(){
        connectionManager = FindObjectOfType<ConnectionManager>();
        

        nextButton.onClick.AddListener(async ()=>{
            leaving = true;
            backButton.interactable=false;
            DisplayManager.PressButtonAndWait(nextButton);
            try{
                int pCnt = connectionManager.GetPlayers().Count;
                // if (pCnt<5) throw new Exception("You need at least 5 players to play."); ### uncomment for final version
                if (pCnt>20) throw  new Exception("You exceeded maximum number of players (20).");

                await connectionManager.MoveToSelect();
            } catch(Exception e){
                errorModal.SetActive(true);
                errorText.text=e.Message;

                // Debug.Log(e);
                DisplayManager.UnpressButton(nextButton);
                leaving=false;
                backButton.interactable=true;
            }
        });
        backButton.onClick.AddListener(async ()=>{
            leaving = true;
            nextButton.interactable=false;
            DisplayManager.PressButtonAndWait(backButton);
            await GoBack();
            DisplayManager.UnpressButton(backButton);
            nextButton.interactable=true;
            leaving = false;
        });
        errorModal.GetComponentInChildren<Button>().onClick.AddListener(()=>{
            errorModal.SetActive(false);
        });
    }

    void Start()
    {   
        joinCodeText.text = connectionManager.GetLobbyCode();

        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(backButton);
        wiFiManager.AddToInteractables(nextButton);
    }

    void Update()
    {
        UpdateInteractables();
    }

    private async Task GoBack(){
        await connectionManager.LeaveLobby();
        DisplayManager.BackToStart();
    }

    private void UpdateInteractables(){
        if (!WiFiManager.IsConnected() || leaving) return;
        
        nextButton.interactable =  AuthenticationService.Instance.PlayerId==connectionManager.GetHostId();
    }

}
