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
    private Sprite roleImage;

    void Start()
    {
        TimerManager.Instance.StartRoleDisplay();
        TimerManager.Instance.RestoreSavedTime();
        
        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(nextButton);
        
        Role role = GamePlayer.Instance.Role;
        string roleTitle = role.Name;
        string roleDesc = role.Description;
        roleImage = role.Image;

        if (role.CardName==RolesManager.CardName.Nyx 
            || role.CardName==RolesManager.CardName.Hemera) {
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

        TimerManager.Instance.StartInitialTimer("Next",10f,nextButton);
    }

    void Update(){
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
}
