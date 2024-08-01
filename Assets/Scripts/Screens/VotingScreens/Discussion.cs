using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Discussion : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] TMP_Text timerText;
    
    [SerializeField] Image timerTop;
    [SerializeField] Image timerBottom;

    [SerializeField] GameObject cardScroll;
    [SerializeField] GameObject cardPrefab;

    private bool runOut=false;

    void Start(){
        FindObjectOfType<WiFiManager>().AddToInteractables(nextButton);

        TimerManager.Instance.StartDiscussion();
        UpdateTimerText();

        nextButton.onClick.AddListener(()=>{
            DisplayManager.GoToNextScene(nextButton);
        });

        List<Role> allCards = GameManager.Instance.GetAllCardsInGame();
        foreach(Role r in allCards){
            GameObject cardObject = DisplayManager.InstantiateWithParent(cardPrefab, cardScroll);
            cardObject.GetComponent<Image>().sprite = r.GetImage();
        }
        int rowNumber = allCards.Count/3 + (allCards.Count%3>0 ? 1:0);
        cardScroll.GetComponent< RectTransform >( ).SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, 370*rowNumber);
    }

    void Update(){   
        UpdateTimerText();

        timerTop.fillAmount = TimerManager.Instance.GetFill();
        timerBottom.fillAmount = 1-TimerManager.Instance.GetFill();
        
        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected()){
            runOut=true;
            SceneManager.LoadScene((int)DisplayManager.Scenes.Voting);
        }
    }

    void UpdateTimerText(){
        string seconds = TimerManager.Instance.GetSeconds().ToString();
        if (seconds.Length==1) seconds="0"+seconds;
        
        timerText.text="0"+TimerManager.Instance.GetMinutes().ToString()+":"+seconds;
    }
}
