using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisplayManager : MonoBehaviour
{
    //cast to int to get a Scene index
    public enum Scenes {
        MainMenu,           //0
        RulesHelp,          //1
        ClientJoin,         //2
        HostJoin,           //3
        Lobby,              //4
        SelectRoles,        //5
        CardsPeek,          //6
        ChoosePlayers,      //7
        InfoPeek,           //8
        RoleDisplay,        //9
        Discussion,         //10
        EndGame,            //11
        Voting,             //12
        VotingResult,       //13
        CardsHelp,          //14
        Waiting,            //15
        NewRoleDisplay,     //16
        SingleElderPeek,    //17
        ElderShow,          //18
    }

    private const int scenesCount = 19;
    [SerializeField] public Color32 errorColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 inputColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 pressedButtonColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 pressedNonTransparentColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 buttonColor = new Color(0,0,0,0);
    [SerializeField] public Color32 textBlockColor = new Color(0,0,0,0);
    [SerializeField] public Color32 winColor = new Color(0,0,0,0);
    [SerializeField] public Color32 lossColor = new Color(0,0,0,0);

    [SerializeField] public Sprite cardBack;
    [SerializeField] public Sprite questionBack;
    static public Color32 ErrorColor{
        get {return FindObjectOfType<DisplayManager>().errorColor;}
    }
    static public Color32 InputColor{
        get {return FindObjectOfType<DisplayManager>().inputColor;}
    }
    static public Color32 PressedButtonColor{
        get {return FindObjectOfType<DisplayManager>().pressedButtonColor;}
    }
    static public Color32 PressedNonTransparentColor{
        get {return FindObjectOfType<DisplayManager>().pressedNonTransparentColor;}
    }
    static public Color32 ButtonColor{
        get {return FindObjectOfType<DisplayManager>().buttonColor;}
    }
    static public Color32 TextBlockColor{
        get {return FindObjectOfType<DisplayManager>().textBlockColor;}
    }
    static public Color32 WinColor{
        get {return FindObjectOfType<DisplayManager>().winColor;}
    }
     static public Color32 LossColor{
        get {return FindObjectOfType<DisplayManager>().lossColor;}
    }

    static public Sprite CardBack{
        get {return FindObjectOfType<DisplayManager>().cardBack;}
    }
    static public Sprite QuestionBack{
        get {return FindObjectOfType<DisplayManager>().questionBack;}
    }
    
    static public void BackToStart(){
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }

    static public void ToGameStart(){
        SceneManager.LoadScene((int)Scenes.RoleDisplay);
    }

    static public void ToSelect(){
        SceneManager.LoadScene((int)Scenes.SelectRoles);
    }

    static public void ToWaiting(){
        
        Debug.Log("Loading waiting screen");
        SceneManager.LoadScene((int)Scenes.Waiting);
    }

    static public void PressButtonAndWait(Button b){
        if(b==null) return;
        b.GetComponent<Image>().color = PressedButtonColor;
        b.interactable=false;
    }

    static public void UnpressButton(Button b){
        b.GetComponent<Image>().color = ButtonColor;
        b.interactable=true;
    }
    static public void GoToNextScene(Button b=null){
        try{
            Debug.Log("Go next scene call start"+(b==null?" because of timeout":"from button click"));
            PressButtonAndWait(b);
            ToWaiting();
            RPCsManager.Instance.GoNextServerRpc(AuthenticationService.Instance.PlayerId);
            Debug.Log("Go next scene call done");
        }catch(Exception e){
            Debug.Log(e);
            UnpressButton(b);
        }
    }

    static public GameObject InstantiateWithParent(GameObject prefab, GameObject parent){
        GameObject o = Instantiate(prefab);
        o.transform.parent = parent.transform;
        o.transform.localScale = new Vector3(1,1,1);
        return o;
    }
}
