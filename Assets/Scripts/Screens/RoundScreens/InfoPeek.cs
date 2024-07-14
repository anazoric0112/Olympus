using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPeek : MonoBehaviour
{
    [SerializeField] GameObject scroll;
    [SerializeField] GameObject playerCardPrefab;
    [SerializeField] GameObject tableCardPrefab;
    [SerializeField] TMP_Text instructionText;
    [SerializeField] GameObject emptyText;
    [SerializeField] Button submitButton;

    List<GameObject> selectedCards = new List<GameObject>();
    List<Button> buttons = new List<Button>();
    List<RoleBehaviour.RevealingCard> revealingCards = new List<RoleBehaviour.RevealingCard>();

    private WiFiManager wiFiManager;
    private int selectable=1;
    private bool submitted=false;
    private bool runOut=false;
    
    void Start()
    {
        TimerManager.Instance.StartMove();
        
        wiFiManager=FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(submitButton);

        FillCards();
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
                    Unsubmit();
                    Debug.Log(e);
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

    private void FillCards(){
        Role r = GamePlayer.Instance.Role;
        revealingCards=r.Behaviour.GetReveals();
        GameObject prefab = playerCardPrefab;
        int cardsCnt=0;

        if(r.GetCardName()==RolesManager.CardName.Cassandra){
            prefab=tableCardPrefab;
            selectable = GameManager.Instance.TableCardsCnt<5 ? 1:2;
        }        
        
        if (revealingCards.Count==0){
            emptyText.SetActive(true);
            submitted=true;
            submitButton.GetComponentInChildren<TMP_Text>().text="Next";
            return;
        }

        foreach(RoleBehaviour.RevealingCard card in revealingCards){
            GameObject obj = DisplayManager.InstantiateWithParent(prefab, scroll);

            Button b = obj.GetComponentInChildren<Button>();
            obj.GetComponentInChildren<TMP_Text>().text = card.PlayerName;
            b.GetComponentInChildren<Image>().sprite = DisplayManager.QuestionBack;

            b.onClick.AddListener(()=>{
                SelectCard(obj);
            });
            wiFiManager.AddToInteractables(b);
            buttons.Add(b);

            cardsCnt++;
        }

        scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 500*(cardsCnt/2 + cardsCnt%2)+100 );
    }

    private void SelectCard(GameObject o){
        if (Selected(GetName(o))){
            Deselect(o);
        } else {
            if (selectedCards.Count==selectable) {
                Deselect(selectedCards[0]);
            }
            o.GetComponentInChildren<Image>().color = DisplayManager.PressedNonTransparentColor;
            selectedCards.Add(o);
        }
    }

    private void Deselect(GameObject o){ 
        o.GetComponentInChildren<Image>().color = new Color32(255,255,255,255);
        if (GetName(selectedCards[0])==GetName(o)) selectedCards.RemoveAt(0);
        else selectedCards.RemoveAt(1);
    }

    private bool Selected(string name){
        foreach(GameObject o in selectedCards)
            if (GetName(o)==name) return true;
        
        return false;
    }

    private string GetName(GameObject o){
        return o.GetComponentInChildren<TMP_Text>().text;
    }

    private Sprite FindImageInRevealing(string name){
        foreach(RoleBehaviour.RevealingCard c in revealingCards){
            if (c.PlayerName==name) return c.Image;
        }
        return null;
    }

    private void Submit(){
        if(selectable!=selectedCards.Count){
            throw new Exception("Not enough selected");
        }
        foreach(Button b in buttons) b.interactable=false;
        submitted=true;
        submitButton.GetComponentInChildren<TMP_Text>().text="Next";

        foreach(GameObject o in selectedCards){
            string name = o.GetComponentInChildren<TMP_Text>().text;
            
            DisplayManager.RotateCard(o.GetComponentInChildren<Image>().gameObject, o.GetComponentInChildren<Image>(), FindImageInRevealing(name));

            o.GetComponentInChildren<Image>().color = new Color32(255,255,255,255);
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
