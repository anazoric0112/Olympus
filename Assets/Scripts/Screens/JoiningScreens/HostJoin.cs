using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HostJoin : MonoBehaviour
{
    [SerializeField] Button makeLobby;
    [SerializeField] Button backButton;
    [SerializeField] TMP_InputField name;
    private ConnectionManager connectionManager;


    void Awake(){
        connectionManager = FindObjectOfType<ConnectionManager>();

        makeLobby.onClick.AddListener(async ()=>{
            backButton.interactable=false;
            DisplayManager.PressButtonAndWait(makeLobby);
            await CreateLobby();
            DisplayManager.UnpressButton(makeLobby);
            backButton.interactable=true;
        });
        backButton.onClick.AddListener(()=>{
            DisplayManager.BackToStart();
        });
        name.onValueChanged.AddListener((string val)=>{
            name.text=val;
        });
    }

    void Start(){        
        name.onSelect.AddListener((string val)=>{
            FindObjectOfType<KeyboardManager>().SelectInput(name);
        });
        name.shouldHideMobileInput=true;
        name.shouldHideSoftKeyboard=true;

        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(makeLobby);
        wiFiManager.AddToInteractables(name);
    }

    private async Task CreateLobby(){  
        if (name.text=="" || name.text.Length>40){
            name.GetComponent<Image>().color = DisplayManager.ErrorColor;
            return;
        }

        name.GetComponent<Image>().color = DisplayManager.InputColor;
        
        try{
            await connectionManager.CreateLobby(name.text);
            SceneManager.LoadScene((int)DisplayManager.Scenes.Lobby);
        } catch(Exception e){
            name.GetComponent<Image>().color = DisplayManager.ErrorColor;
        }
    }
}
