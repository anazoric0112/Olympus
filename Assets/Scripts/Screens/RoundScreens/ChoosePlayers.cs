using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoosePlayers : MonoBehaviour
{
    [SerializeField] GameObject playersScroll;
    [SerializeField] GameObject playerNamePrefab;
    [SerializeField] TMP_Text instructionText;
    [SerializeField] Button submitButton;

    List<GameObject> selectedPlayers = new List<GameObject>();
    List<Button> buttons = new List<Button>();

    private WiFiManager wiFiManager;
    private int selectable=1;
    private bool submitted=false;
    private bool runOut=false;

    void Start()
    {
        TimerManager.Instance.StartMove();
        
        wiFiManager=FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(submitButton);

        FillNames();
        instructionText.text=GamePlayer.Instance.Role.Behaviour.MoveInst;

        submitButton.onClick.AddListener(()=>{
            if (submitted){
                DisplayManager.GoToNextScene(submitButton);
            } else {
                DisplayManager.PressButtonAndWait(submitButton);
                try{
                    Submit();
                    DisplayManager.UnpressButton(submitButton);
                }catch(Exception e){
                    Debug.Log(e);
                    Unsubmit();
                }
            }
        });
    }
    
    void Update(){
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()) {
            runOut = true;
            DisplayManager.GoToNextScene();
        }
    }

    private void FillNames(){
        int playersCnt = 0;

        if (GamePlayer.Instance.Role.GetCardName()==RolesManager.CardName.Pandora) selectable=2;

        foreach(KeyValuePair<string,string> kp in GameManager.Instance.playerNames){
            string id = kp.Key, name = kp.Value;

            if (AuthenticationService.Instance.PlayerId==id) continue;

            GameObject obj = DisplayManager.InstantiateWithParent(playerNamePrefab, playersScroll);
            Button b = obj.GetComponentInChildren<Button>();

            b.GetComponentInChildren<TMP_Text>().text = name;
            b.onClick.AddListener(()=>{
                SelectPlayer(obj);
            });
            wiFiManager.AddToInteractables(b);
            buttons.Add(b);

            playersCnt++;
        }

        playersScroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(480, playersCnt+100) );
    }

    private void SelectPlayer(GameObject o){
        if (PlayerSelected(GetName(o))){
            DeselectPlayer(o);
        } else{
            if(selectedPlayers.Count==selectable) {
                DeselectPlayer(selectedPlayers[0]);
            }
            o.GetComponentInChildren<Image>().color = DisplayManager.PressedButtonColor;
            selectedPlayers.Add(o);
        }
    }

    private void DeselectPlayer(GameObject o){ 
        o.GetComponentInChildren<Image>().color = DisplayManager.TextBlockColor;
        if (GetName(selectedPlayers[0])==GetName(o)) selectedPlayers.RemoveAt(0);
        else selectedPlayers.RemoveAt(1);
    }

    private bool PlayerSelected(string name){
        foreach(GameObject o in selectedPlayers)
            if (GetName(o)==name) return true;
        
        return false;
    }

    private string GetName(GameObject o){
        return o.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text;
    }

    private void Submit(){
        if (selectedPlayers.Count!=selectable) {
            throw new Exception("Not enough selected");
        }

        foreach(Button b in buttons) b.interactable=false;
        submitted=true;
        submitButton.GetComponentInChildren<TMP_Text>().text="Next";

        RoleBehaviour rb = GamePlayer.Instance.Role.Behaviour;

        if (selectable==2) rb.DoActive(GetName(selectedPlayers[0]), GetName(selectedPlayers[1]));
        else rb.DoActive(GetName(selectedPlayers[0]));

        if (rb.RevealMyCard){
            TimerManager.Instance.SaveTime();
            SceneManager.LoadScene((int)DisplayManager.Scenes.NewRoleDisplay);
        }
    }

    private void Unsubmit(){
        DisplayManager.UnpressButton(submitButton);
        foreach(Button b in buttons) b.interactable=true;
        submitted=false;
        submitButton.GetComponentInChildren<TMP_Text>().text="Submit";
        submitButton.GetComponent<Image>().color = DisplayManager.ErrorColor;
    }

}
