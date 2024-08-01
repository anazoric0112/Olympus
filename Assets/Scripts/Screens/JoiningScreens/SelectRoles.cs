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
    [SerializeField] GameObject scrollableCards;
    [SerializeField] GameObject roleCardPrefab;
    [SerializeField] GameObject cardGroupPrefab;

    [SerializeField] GameObject descModal;
    [SerializeField] TMP_Text descText;
    [SerializeField] TMP_Text descTitle;
    [SerializeField] Button descCloseButton;

    [SerializeField] GameObject errorModal;
    [SerializeField] TMP_Text errorText;

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
            leaving = true;
            DisplayManager.PressButtonAndWait(startGame);

            try{
                CheckSelection(selectedRoles);
                List<Role> assigned=RandomShuffle(selectedRoles);

                await connectionManager.StartGame(assigned);
            } catch(Exception e){
                errorText.text=e.Message;
                errorModal.SetActive(true);

                // Debug.Log(e);
                DisplayManager.UnpressButton(startGame);
                leaving = false;
            }
        });
        
        descCloseButton.onClick.AddListener(()=>{
            descModal.SetActive(false);
        });
        errorModal.GetComponentInChildren<Button>().onClick.AddListener(()=>{
            errorModal.SetActive(false);
        });
    }

    void Start(){
        myPlayerId = AuthenticationService.Instance.PlayerId;

        wiFiManager = FindObjectOfType<WiFiManager>();
        wiFiManager.AddToInteractables(startGame);

        FillCards();
    }

    void Update()
    {
        UpdateInteractables();
    }

    private void UpdateInteractables(){
        if (!WiFiManager.IsConnected() || leaving) return;
        
        bool isHost = myPlayerId==connectionManager.HostId;
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

    private List<Role> RandomShuffle(List<Role> l, int rngCnt=40){
        int n = l.Count;
        if (n==0) return l;
        
        for (int i=0;i<rngCnt;i++){
            int rng1 = UnityEngine.Random.Range(0,n);
            int rng2 = UnityEngine.Random.Range(0,n);
            Role temp = l[rng1];
            l[rng1]=l[rng2];
            l[rng2]=temp;
        }

        int pCnt = connectionManager.GetPlayers().Count;
        bool e=false, t=false;

        for(int i=0;i<pCnt;i++){
            if (l[i].Behaviour.CardClass==RolesManager.CardClass.Elder) e=true;
            if (l[i].Behaviour.CardClass==RolesManager.CardClass.Cursed) t=true;
        }

        if (e!=t){ //samo jedan treba da se zameni
            int rng = UnityEngine.Random.Range(0,pCnt);
            RolesManager.CardClass toFind = e ? RolesManager.CardClass.Cursed:RolesManager.CardClass.Elder;

            for(int i=pCnt;i<n;i++){
                if (l[i].Behaviour.CardClass!=toFind) continue;

                Role temp = l[i];
                l[i]=l[rng];
                l[rng]=temp;
                break;
            }
        } else if (e==false && t==false){ //oba treba da se zamene
            int rngt = UnityEngine.Random.Range(0,pCnt);
            int rnge = UnityEngine.Random.Range(0,pCnt);

            while (rngt==rnge) rnge = UnityEngine.Random.Range(0,pCnt);

            for(int i=pCnt;i<n;i++){
                if (l[i].Behaviour.CardClass==RolesManager.CardClass.Cursed && !t) {
                    Role temp = l[i];
                    l[i]=l[rngt];
                    l[rngt]=temp;
                    t=true;
                }
                if (l[i].Behaviour.CardClass==RolesManager.CardClass.Elder && !e) {
                    Role temp = l[i];
                    l[i]=l[rnge];
                    l[rnge]=temp;
                    e=true;
                }
                if (e && t) break;
            }
        }

        return l;
    }

    private bool CheckSelection(List<Role> roles){

        int pCnt = connectionManager.GetPlayers().Count;
        int t=0, e=0, o=0;
        int table = roles.Count-pCnt;
        List<RolesManager.CardName> selected = new List<RolesManager.CardName>();

        if (roles.Count<pCnt) throw new Exception("You selected fewer cards than there are players in game.");
        
        foreach(Role r in roles){
            selected.Add(r.GetCardName());
            if (r.Behaviour.CardClass==RolesManager.CardClass.Cursed) t++;
            if (r.Behaviour.CardClass==RolesManager.CardClass.Elder) e++;
            if (r.Behaviour.Team==RolesManager.Team.Olympus) o++;
        }

        int maxCursed = (int)Math.Max(1,Math.Round(((double)o)/3));
        
        if (t<1 || t>maxCursed) throw new Exception("Number of Cursed selected must be between 1 and number of all others devided by 3 and rounded (both limits included).");
        if (e!=t && e!=t-1) throw new Exception("Number of Elders selected must be equal to or 1 less than number of Cursed selected.");

        if (selected.Contains(RolesManager.CardName.Dionysis) && t<3) throw new Exception("You need at least 3 Cursed selected in order to select Dionysis.");
        if (selected.Contains(RolesManager.CardName.Dracaena) && table<2) throw new Exception("You need at least 2 table cards in order to select Dracaena.");
        if (selected.Contains(RolesManager.CardName.Phoenix) && table<3) throw new Exception("You need at least 3 table cards in order to select Phoenix.");
        if (selected.Contains(RolesManager.CardName.Cassandra) && table<3) throw new Exception("You need at least 3 table cards in order to select Cassandra.");
        if (selected.Contains(RolesManager.CardName.Phoenix) 
            && selected.Contains(RolesManager.CardName.Cassandra)
            && table<4) throw new Exception("You need at least 4 table cards in order to select both Phoenix and Cassandra.");
        if (selected.Contains(RolesManager.CardName.Nyx)!=selected.Contains(RolesManager.CardName.Hemera)) throw new Exception("You must select both Nyx and Hemera to select either of them.");
        if (selected.Contains(RolesManager.CardName.Perseus)!=selected.Contains(RolesManager.CardName.Medusa)) throw new Exception("You must select both Perseus and Medusa to select either of them.");

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
