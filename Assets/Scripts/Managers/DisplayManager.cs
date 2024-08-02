using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    //-------Colors-------
    [SerializeField] public Color32 errorColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 inputColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 pressedButtonColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 pressedNonTransparentColor = new Color32(0,0,0,0);
    [SerializeField] public Color32 buttonColor = new Color(0,0,0,0);
    [SerializeField] public Color32 textBlockColor = new Color(0,0,0,0);
    [SerializeField] public Color32 winColor = new Color(0,0,0,0);
    [SerializeField] public Color32 lossColor = new Color(0,0,0,0);

    //-------Sprites-------
    [SerializeField] public Sprite cardBack;
    [SerializeField] public Sprite questionBack;
    [SerializeField] public Sprite questionBackNonClickable;

    //-------Properties-------
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
    static public Sprite QuestionBackNonClickable{
        get {return FindObjectOfType<DisplayManager>().questionBackNonClickable;}
    }
    static private List<RotatingCard> currentRotates = new List<RotatingCard>();
    
    //------------------------------------------------------------
    //Code for rotating cards
    //------------------------------------------------------------

    class RotatingCard{
        Sprite sprite;
        float rotation = 180;
        static float step=2f;
        static int stepCnt = 5;
        GameObject go;
        Image img;

        public RotatingCard(GameObject g, Sprite s, Image i){
            go=g;
            sprite=s;
            img=i;
        }

        public void Rotate(){
            for (int i=0;i<stepCnt;i++) {
                go.transform.Rotate(0,step,0);
                rotation+=step;
                if (rotation>=270) {
                    img.sprite=sprite;
                    break;
                }
            }
        }

        public void RotateToStart(){
            go.transform.Rotate(0,-360,0);
            rotation=180;
        }

        public bool RotationCompleted{
            get{return rotation>=360;}
        }
        public int Hash{
            get {return go.GetHashCode();}
        }
    }
    
    private void Update(){
        try{
            List<int> toRemove = new List<int>();

            for(int i=0;i<currentRotates.Count;i++){
                RotatingCard card = currentRotates[i];
                card.Rotate();
                if (card.RotationCompleted) {
                    toRemove.Add(i);
                    card.RotateToStart();
                }
            }

            for(int i=toRemove.Count-1;i>=0;i--){
                currentRotates.RemoveAt(toRemove[i]);
            }
        }catch (Exception e){
            Debug.Log(e);
            currentRotates.Clear();
        }

    }

    static public bool RotateCard(GameObject card, Image image, Sprite sprite){
        foreach(RotatingCard c in currentRotates){
            if (c.Hash==card.GetHashCode()) return false;
        }
        currentRotates.Add(new RotatingCard(card, sprite, image));
        return true;
    }

    //------------------------------------------------------------
    //Methods for moving between scenes
    //------------------------------------------------------------

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
        SceneManager.LoadScene((int)Scenes.Waiting);
    }

    static public void GoToNextScene(Button b=null){
        try{
            PressButtonAndWait(b);
            ToWaiting();
            RPCsManager.Instance.GoNextServerRpc(AuthenticationService.Instance.PlayerId);
        }catch(Exception e){
            UnpressButton(b);
        }
    }

    static public void LeaveGame(Image background){
        background.color=new Color32(255,255,255,255);
        FindObjectOfType<ConnectionManager>().LeaveRelay(GameManager.Instance.playersIdsList[0]);
        
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }

    //------------------------------------------------------------
    //Helper methods for controlling UI
    //------------------------------------------------------------

    static public void PressButtonAndWait(Button b){
        if(b==null) return;
        b.GetComponent<Image>().color = PressedButtonColor;
        b.interactable=false;
    }

    static public void UnpressButton(Button b){
        b.GetComponent<Image>().color = ButtonColor;
        b.interactable=true;
    }

    static public GameObject InstantiateWithParent(GameObject prefab, GameObject parent){
        GameObject o = Instantiate(prefab);
        o.transform.parent = parent.transform;
        o.transform.localScale = new Vector3(1,1,1);
        return o;
    }


}
