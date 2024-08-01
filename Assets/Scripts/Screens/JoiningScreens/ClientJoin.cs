using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientJoin : MonoBehaviour
{
    [SerializeField] Button joinGame;
    [SerializeField] Button backButton;
    [SerializeField] TMP_InputField gameCode;
    [SerializeField] TMP_InputField playerName;


    private ConnectionManager connectionManager;
    private TMP_InputField selectedInput = null;
    
    void Awake(){
        connectionManager = FindObjectOfType<ConnectionManager>();

        joinGame.onClick.AddListener(async ()=>{
            backButton.interactable = false;
            DisplayManager.PressButtonAndWait(joinGame);
            await JoinLobby();
            DisplayManager.UnpressButton(joinGame);
            backButton.interactable = true;
        });
        backButton.onClick.AddListener(()=>{
            DisplayManager.BackToStart();
        });

        gameCode.onValueChanged.AddListener((string val)=>{
            gameCode.text=val;
        });
        playerName.onValueChanged.AddListener((string val)=>{
            playerName.text=val;
        });
    }

    void Start(){
        gameCode.onSelect.AddListener((string val)=>{
            FindObjectOfType<KeyboardManager>().SelectInput(gameCode);
        });
        playerName.onSelect.AddListener((string val)=>{
            FindObjectOfType<KeyboardManager>().SelectInput(playerName);
        });

        gameCode.shouldHideMobileInput=true;
        playerName.shouldHideMobileInput=true;
        gameCode.shouldHideSoftKeyboard=true;
        playerName.shouldHideSoftKeyboard=true;

        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(joinGame);
        wiFiManager.AddToInteractables(gameCode);
        wiFiManager.AddToInteractables(playerName);
    }

    private async Task JoinLobby(){
        if (playerName.text=="" || playerName.text.Length>40){
            playerName.GetComponent<Image>().color = DisplayManager.ErrorColor;
            return;
        } 
        gameCode.GetComponent<Image>().color = DisplayManager.InputColor;
        playerName.GetComponent<Image>().color = DisplayManager.InputColor;

        try {
            await connectionManager.JoinLobbyByCode(playerName.text, gameCode.text);        
        }catch (Exception e){
            Debug.Log(e.ToString());
            if (e.Message=="Player with that name already exists"){
                playerName.GetComponent<Image>().color = DisplayManager.ErrorColor;
            }
            else {
                gameCode.GetComponent<Image>().color = DisplayManager.ErrorColor;
            }

            return;
        }
        while (connectionManager.GetAsyncOngoing()) { 
            // yield return null;
        }
        SceneManager.LoadScene((int)DisplayManager.Scenes.Lobby);        

    }

}
