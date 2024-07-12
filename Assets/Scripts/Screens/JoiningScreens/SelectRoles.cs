using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectRoles : MonoBehaviour
{    
    [SerializeField] Button startGame;
    [SerializeField] Button backButton;
    [SerializeField] GameObject scrollableCards;
    [SerializeField] GameObject roleCardPrefab;
    [SerializeField] GameObject cardGroupPrefab;

    [SerializeField] GameObject descModal;
    [SerializeField] TMP_Text descText;
    [SerializeField] TMP_Text descTitle;
    [SerializeField] Button descCloseButton;

    private List<Role> selectedRoles = new List<Role>();
    private List<Toggle> checkButtons = new List<Toggle>();
    
    private ConnectionManager connectionManager;
    private RolesManager rolesManager;
    private WiFiManager wiFiManager;
    private string myPlayerId = "";
    private bool leaving = false;

    void Awake(){

        connectionManager = FindObjectOfType<ConnectionManager>();
        rolesManager = FindObjectOfType<RolesManager>();

        startGame.onClick.AddListener(async ()=>{
            backButton.interactable=false;
            leaving = true;
            DisplayManager.PressButtonAndWait(startGame);

            List<Role> assigned=AssignRoles();
            try{
                if (!CheckSelection(assigned)) throw new Exception("Not enough cards selected");
                await connectionManager.StartGame(assigned);
            } catch(Exception e){
                Debug.Log(e);
                DisplayManager.UnpressButton(startGame);
                leaving = false;
                backButton.interactable=true;
            }
        });
        backButton.onClick.AddListener(async ()=>{
            leaving = true;
            startGame.interactable=false;
            DisplayManager.PressButtonAndWait(backButton);
            await GoBack();
            DisplayManager.UnpressButton(backButton);
            startGame.interactable=true;
            leaving = false;
        });
        descCloseButton.onClick.AddListener(()=>{
            descModal.SetActive(false);
        });
    }

    void Start(){
        myPlayerId = AuthenticationService.Instance.PlayerId;

        wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(startGame);
        wiFiManager.AddToInteractables(backButton);

        FillCards();
    }

    void Update()
    {
        UpdateInteractables();
    }
    
    private async Task GoBack(){
        await connectionManager.LeaveLobby();
        DisplayManager.BackToStart();
    }

    private void UpdateInteractables(){
        if (!WiFiManager.IsConnected() || leaving) return;
        
        bool isHost = myPlayerId==connectionManager.GetHostId();
        startGame.interactable = isHost;
        foreach(Toggle b in checkButtons)
            b.interactable = isHost;
    }

    private void ToggleValue(Role role){
        foreach(Role r in selectedRoles){
            if(r.GetName() == role.GetName()){
                selectedRoles.Remove(r);
                return;
            }
        }
        selectedRoles.Add(role);
    }

    private void SetDescription(Role role){
        descText.text=role.GetDescription();
        descTitle.text=role.GetName();
        descModal.SetActive(true);
    }

    public List<Role> GetSelectedRoles(){
        return selectedRoles;
    }

    public List<Role> AssignRoles(){
        return RandomShuffle(selectedRoles);
    }

    private List<Role> RandomShuffle(List<Role> l, int cnt=30){
        int n = l.Count;
        for (int i=0;i<cnt;i++){
            int rng1 = UnityEngine.Random.Range(0,n);
            int rng2 = UnityEngine.Random.Range(0,n);
            Role t = l[rng1];
            l[rng1]=l[rng2];
            l[rng2]=t;
        }
        return l;
    }

    private bool CheckSelection(List<Role> roles){
        if(roles.Count==0) return false;

        //### ovde se na kraju dodaje full provera selekcije
        //za sad ne treba zbog testiranja

        return true;
    }

    private void FillCards(){
        List<Role> roles = rolesManager.GetAllRoles();
        int rowNumber = roles.Count/2 + roles.Count%2;

        GameObject group = DisplayManager.InstantiateWithParent(cardGroupPrefab, scrollableCards);
        group.GetComponent< RectTransform >( ).SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, 470*rowNumber+100);
        scrollableCards.GetComponent< RectTransform >( ).SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, 470*rowNumber+100);

        foreach (Role role in roles){
            GameObject roleCard = DisplayManager.InstantiateWithParent(roleCardPrefab, group);
            roleCard.GetComponentInChildren<Image>().sprite = role.GetImage();

            Toggle checkbox = roleCard.GetComponentInChildren<Image>().GetComponentInChildren<Toggle>();
            checkbox.onValueChanged.AddListener(delegate {
                ToggleValue(role);
            });
            checkButtons.Add(checkbox);

            Button infoButton = roleCard.GetComponentInChildren<Image>().GetComponentInChildren<Button>();

            infoButton.onClick.AddListener(()=>{
                SetDescription(role);
            });
            wiFiManager.AddToInteractables(infoButton);
        }
    }
}
