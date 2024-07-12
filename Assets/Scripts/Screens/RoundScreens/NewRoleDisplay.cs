using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewRoleDisplay : MonoBehaviour
{
    [SerializeField] GameObject image;
    [SerializeField] Button toggleImageButton;
    [SerializeField] Sprite cardBack;
    [SerializeField] Button nextButton;
    [SerializeField] GameObject instructionText;
    [SerializeField] Sprite nyxHemeraImage;

    private bool showed = false;
    private bool runOut = false;
    private Sprite roleImage;

    void Start()
    {
        TimerManager.Instance.StartMove();
        TimerManager.Instance.RestoreSavedTime();
        
        WiFiManager wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(nextButton);
        
        Role role = GamePlayer.Instance.NextRole;
        if (role.GetCardName()==RolesManager.CardName.Nyx 
            || role.GetCardName()==RolesManager.CardName.Hemera) roleImage = nyxHemeraImage;
        else roleImage = role.GetImage();

        Debug.Log("New role display "+role.GetName());
        
        image.GetComponentInChildren<Image>().color = new Color32(255,255,255,255);
        image.GetComponentInChildren<Image>().sprite = cardBack;

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });

        toggleImageButton.onClick.AddListener(()=>{
            ToggleImage();
        });

        TimerManager.Instance.StartMove();
    }

    void Update(){
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()) {
            runOut = true;
            DisplayManager.GoToNextScene();
        }
    }

    private void ToggleImage(){
        if (showed){
            image.GetComponentInChildren<Image>().sprite = cardBack;
            instructionText.SetActive(true);
        } else {
            image.GetComponentInChildren<Image>().sprite = roleImage;
            instructionText.SetActive(false);
        }
        showed = !showed;
    }
}
