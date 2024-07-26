using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleDisplay : MonoBehaviour
{
    [SerializeField] GameObject image;
    [SerializeField] Button toggleImageButton;
    [SerializeField] Button nextButton;
    [SerializeField] GameObject instructionText;
    [SerializeField] Sprite nyxHemeraImage;

    [SerializeField] Button descOpenButton;
    [SerializeField] GameObject descModal;
    [SerializeField] TMP_Text descText;
    [SerializeField] TMP_Text descTitle;
    [SerializeField] Button descCloseButton;


    private bool showed = false;
    private bool runOut = false;
    private bool initialOver = false;
    private Sprite roleImage;
    private float initialTimer = 10;

    void Start()
    {
        TimerManager.Instance.StartRoleDisplay();
        TimerManager.Instance.RestoreSavedTime();
        
        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(nextButton);
        
        Role role = GamePlayer.Instance.Role;
        string roleTitle = role.GetName();
        string roleDesc = role.GetDescription();
        roleImage = role.GetImage();

        if (role.GetCardName()==RolesManager.CardName.Nyx 
            || role.GetCardName()==RolesManager.CardName.Hemera) {
                roleImage = nyxHemeraImage;
                roleTitle="Nyx & Hemera";
                roleDesc = "You don't know whether you are Nyx or Hemera. At the beginning of a round, they both see info that someone is Tartarus member, but Hemera's info is true and Nyx's is false.";
        }
        
        image.GetComponentInChildren<Image>().color = new Color32(255,255,255,255);
        image.GetComponentInChildren<Image>().sprite = DisplayManager.CardBack;

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });

        toggleImageButton.onClick.AddListener(()=>{
            ToggleImage();
        });

        descText.text = roleDesc;
        descTitle.text = roleTitle;

        descOpenButton.onClick.AddListener(()=>{
            descModal.SetActive(true);
        });
        descCloseButton.onClick.AddListener(()=>{
            descModal.SetActive(false);
        });

        SetButtonText(((int)initialTimer).ToString());
        nextButton.interactable=false;
    }

    void Update(){
        initialTimer-=Time.deltaTime;
        if ((int)initialTimer>=0){
            SetButtonText(((int)initialTimer).ToString());
        } else if (!initialOver){
            nextButton.interactable=true;
            SetButtonText("Next");
            initialOver=true;
        }

        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()) {
            runOut = true;
            DisplayManager.GoToNextScene();
        }
    }

    private void ToggleImage(){
        bool added = false;
        if (showed){
            added = DisplayManager.RotateCard(image.gameObject, image.GetComponentInChildren<Image>(), DisplayManager.CardBack);
        } else {
            added = DisplayManager.RotateCard(image.gameObject, image.GetComponentInChildren<Image>(), roleImage);
            instructionText.SetActive(false);
        }
        if (added) showed = !showed;
    }

    private void SetButtonText(string t){
        nextButton.GetComponentInChildren<TMP_Text>().text=t;
    }
}
