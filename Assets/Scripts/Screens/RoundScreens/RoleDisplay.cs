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
    [SerializeField] Sprite cardBack;
    [SerializeField] Button nextButton;
    [SerializeField] GameObject instructionText;
    [SerializeField] Sprite nyxHemeraImage;

    private bool showed = false;
    private bool runOut = false;
    private bool initialOver = false;
    private Sprite roleImage;
    private float initialTimer = 10;

    void Start()
    {
        TimerManager.Instance.StartMove();
        TimerManager.Instance.RestoreSavedTime();
        
        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(nextButton);
        
        Role role = GamePlayer.Instance.Role;
        if (role.GetCardName()==RolesManager.CardName.Nyx 
            || role.GetCardName()==RolesManager.CardName.Hemera) roleImage = nyxHemeraImage;
        else roleImage = role.GetImage();
        
        image.GetComponentInChildren<Image>().color = new Color32(255,255,255,255);
        image.GetComponentInChildren<Image>().sprite = cardBack;

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });

        toggleImageButton.onClick.AddListener(()=>{
            ToggleImage();
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
            added = DisplayManager.RotateCard(image.gameObject, image.GetComponentInChildren<Image>(), cardBack);
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
