using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VotingResult : MonoBehaviour
{
    [SerializeField] Button nextButton;
    [SerializeField] Image votedForCard;
    [SerializeField] TMP_Text votedForName;
    [SerializeField] GameObject scroll;
    [SerializeField] GameObject playerCardPrefab;
    [SerializeField] GameObject noneOut;
    [SerializeField] GameObject noneVotedFor;
    [SerializeField] Image background;
    [SerializeField] TMP_Text titleVotedFor;


    private bool runOut = false;

    void Start()
    {
        TimerManager.Instance.StartResultScreen();

        FindObjectOfType<WiFiManager>().AddToInteractables(nextButton);

        FillVotedFor();
        FillOut();
        if (ThisPlayerIsOut()){
            background.color = DisplayManager.LossColor;
            nextButton.GetComponentInChildren<TMP_Text>().text="Leave game";
        }

        if (GameManager.Instance.forCursedVoteResultMI==GameManager.Instance.MoveIndex) titleVotedFor.text = "Voted out for Cursed:";
        else titleVotedFor.text="Voted out for Elder:";
        
        nextButton.onClick.AddListener(()=>{
            if (ThisPlayerIsOut()){
                DisplayManager.LeaveGame(background);
            } else {
                DisplayManager.GoToNextScene(nextButton);
            }
        });
    }

    private void FillVotedFor(){
        List<string> votedList = GameManager.Instance.LastVotedOutList;
        if (votedList.Count>1) {
            noneVotedFor.SetActive(true);
            votedForCard.gameObject.SetActive(false);
            votedForName.gameObject.SetActive(false);
            return;
        }

        string votedName = GameManager.Instance.LastVotedOutName;
        foreach(KeyValuePair<string, RolesManager.CardName> kp in GameManager.Instance.lastPlayersOut){
            if (kp.Key==votedName){
                votedForCard.sprite=GameManager.Instance.roleInstances[kp.Value].Image;
                votedForName.text=votedName;
                return;
            }
        }
        RolesManager.CardName card = GameManager.Instance.playerCards[votedList[0]];
        votedForCard.sprite=GameManager.Instance.roleInstances[card].Image;
        votedForName.text=GameManager.Instance.playerNames[votedList[0]];

        if (!ShowCard(card,PlayerIsOut(votedForName.text))) votedForCard.sprite=DisplayManager.QuestionBackNonClickable;
    }

    private void FillOut(){
        if (GameManager.Instance.lastPlayersOut.Count==0){
            noneOut.SetActive(true);
            return;
        }
        int cnt=0;
        foreach(KeyValuePair<string,RolesManager.CardName> kp in GameManager.Instance.lastPlayersOut){
            string playerName = kp.Key;
            RolesManager.CardName card = kp.Value;
            
            GameObject cardObject = DisplayManager.InstantiateWithParent(playerCardPrefab, scroll);
            cardObject.GetComponentInChildren<TMP_Text>().text = playerName;
            cardObject.GetComponentInChildren<Image>().sprite = GameManager.Instance.roleInstances[card].Image;
            cnt++;
        }
        
        scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(500, 500*(cnt/2 + cnt%2)+100) );
    }

    private bool ThisPlayerIsOut(){
        foreach(string name in GameManager.Instance.lastPlayersOut.Keys){
            if(name==GamePlayer.Instance.Name) return true;
        }
        return false;
    }
    private bool PlayerIsOut(string playerName){
        foreach(string name in GameManager.Instance.lastPlayersOut.Keys){
            if(name==playerName) return true;
        }
        return false;
    }


    void Update()
    {
        bool migrationOngoing = FindObjectOfType<ConnectionManager>().MigrationOngoing;
        if (nextButton.interactable && migrationOngoing) nextButton.interactable=false;
        else if (!nextButton.interactable && !migrationOngoing) StartCoroutine(EnableNext());

        if (TimerManager.Instance.IsRunOut() && !runOut && WiFiManager.IsConnected() 
            && nextButton.interactable){
            runOut=true;
            DisplayManager.GoToNextScene();
        }
    }

    private IEnumerator EnableNext(){
        yield return new WaitForSeconds(3f);
        nextButton.interactable=true;
    }

    private bool ShowCard(RolesManager.CardName card, bool isOut){
        RolesManager.Team myTeam = GamePlayer.Instance.Role.Behaviour.Team;
        RolesManager.Team votingForTeam = GameManager.Instance.forCursedVoteResultMI==GameManager.Instance.MoveIndex ? RolesManager.Team.Tartarus : RolesManager.Team.Olympus;


        if (GetNeverShowCards().Contains(card)) return false;
        if (GetShowIfOut().Contains(card)) {
            if (isOut) return true;
            else return false;
        }
        if (GetShowIfVotingForElder().Contains(card)){
            if (votingForTeam==RolesManager.Team.Olympus) return true;
            else return false;
        }
        if (GetShowIfDoneEffect(card)) {
            if (card==RolesManager.CardName.Orpheus && myTeam==votingForTeam) return false;
            else return true;
        }

        return false;
    }

    private List<RolesManager.CardName> GetShowIfVotingForElder(){
        List<RolesManager.CardName> showInVFElder = new List<RolesManager.CardName>();

        //ovi isto mogu da se sutnu u death effect ako im se doda
        showInVFElder.Add(RolesManager.CardName.Perseus);
        showInVFElder.Add(RolesManager.CardName.Basilisk);

        return showInVFElder;
    }

    private bool GetShowIfDoneEffect(RolesManager.CardName card){
        if (card!=RolesManager.CardName.Artemis &&
            card!=RolesManager.CardName.Apollo &&
            card!=RolesManager.CardName.Orpheus) return false;

        RoleBehaviour rb = GameManager.Instance.roleInstances[card].Behaviour;
        if (rb.DoneDeathEffect){
            GameManager.Instance.roleInstances[card].Behaviour.ClearDeathEffect();
            return true;
        }

        return false;
    }

    private List<RolesManager.CardName> GetNeverShowCards(){
        List<RolesManager.CardName> neverShow = new List<RolesManager.CardName>();
        neverShow.Add(RolesManager.CardName.Cassandra);
        neverShow.Add(RolesManager.CardName.Pegasus);
        neverShow.Add(RolesManager.CardName.Phoenix);
        neverShow.Add(RolesManager.CardName.Dracaena);
        neverShow.Add(RolesManager.CardName.Dryad);
        neverShow.Add(RolesManager.CardName.Pandora);
        neverShow.Add(RolesManager.CardName.Nyx);
        neverShow.Add(RolesManager.CardName.Hemera);
        neverShow.Add(RolesManager.CardName.Achilles);
        return neverShow;
    }

    private List<RolesManager.CardName> GetShowIfOut(){
        List<RolesManager.CardName> showIfOut = new List<RolesManager.CardName>();
        showIfOut.Add(RolesManager.CardName.Athena);
        showIfOut.Add(RolesManager.CardName.Zeus);
        showIfOut.Add(RolesManager.CardName.Aphrodite);
        showIfOut.Add(RolesManager.CardName.Dionysis);
        showIfOut.Add(RolesManager.CardName.Charon);
        showIfOut.Add(RolesManager.CardName.Medusa);
        showIfOut.Add(RolesManager.CardName.Hades);
        showIfOut.Add(RolesManager.CardName.Siren);
        showIfOut.Add(RolesManager.CardName.Hydra);
        showIfOut.Add(RolesManager.CardName.Sisyphus);
        return showIfOut;
    }
}
