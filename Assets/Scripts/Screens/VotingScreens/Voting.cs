using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Voting : MonoBehaviour
{
    [SerializeField] Button voteButton;
    [SerializeField] GameObject scroll;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] TMP_Text voteTitle;

    private WiFiManager wiFiManager;
    private bool runOut=false;
    private GameObject selected = null;
    private bool voteForCursed=false; //false-elders, true-cursed
    
    void Start(){
        TimerManager.Instance.StartVote();

        wiFiManager=FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(voteButton);

        voteForCursed = GameManager.Instance.IsCursedVote;

        if (voteForCursed) voteTitle.text="Vote for Cursed";
        else voteTitle.text="Vote for Elder";

        FillNames();

        voteButton.onClick.AddListener(()=>{
            DisplayManager.PressButtonAndWait(voteButton);
            try{
                SubmitVote();
            }catch(Exception e){
                Debug.Log(e);
                DisplayManager.UnpressButton(voteButton);
            }
        });
    }

    void Update(){
        int secondsLeft = TimerManager.Instance.GetSeconds();
        if (secondsLeft<7 && secondsLeft>0){
            TMP_Text btnText = voteButton.GetComponentInChildren<TMP_Text>();
            voteButton.GetComponentInChildren<Image>().color=DisplayManager.ErrorColor;
            btnText.text="Hurry up ";
            for(int i=0;i<7-secondsLeft;i++) btnText.text+="!";
        }

        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()){
            runOut=true;
            SubmitVote();
        }
    }

    private void FillNames(){
        int playersCnt = 0;

        foreach(KeyValuePair<string,string> kp in GameManager.Instance.playerNames){
            string id = kp.Key, name = kp.Value;

            if (GamePlayer.Instance.Id==id) continue;

            GameObject obj = DisplayManager.InstantiateWithParent(playerPrefab, scroll);
            Button b = obj.GetComponentInChildren<Button>();

            b.GetComponentInChildren<TMP_Text>().text = name;
            b.onClick.AddListener(()=>{
                SelectPlayer(obj);
            });
            wiFiManager.AddToInteractables(b);
            playersCnt++;
        }

        scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(480, playersCnt+100) );
    }

    private void SelectPlayer(GameObject o){
        if (GetName(o)==GetName(selected)){
            DeselectPlayer(o);
            return;
        }
        if (selected!=null){
            DeselectPlayer(selected);
        }

        o.GetComponentInChildren<Image>().color = DisplayManager.PressedButtonColor;
        selected=o;

        voteButton.GetComponentInChildren<TMP_Text>().text="Submit vote";
        voteButton.GetComponentInChildren<Image>().color=DisplayManager.ButtonColor;
    }

    private void DeselectPlayer(GameObject o){
        o.GetComponentInChildren<Image>().color = DisplayManager.TextBlockColor;
        selected=null;
    }

    private string GetName(GameObject o){
        if (o==null) return "";
        return o.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>().text;
    }

    private void SubmitVote(){
        if (selected==null){
            voteButton.GetComponentInChildren<Image>().color=DisplayManager.ErrorColor;
            voteButton.GetComponentInChildren<TMP_Text>().text="You must vote";
            voteButton.interactable=true;
            return;
        }
        RPCsManager.Instance.VoteServerRpc(GetName(selected),GamePlayer.Instance.Id,!voteForCursed);
        DisplayManager.GoToNextScene();
    }
}
