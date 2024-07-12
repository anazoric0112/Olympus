using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoleBehaviour : MonoBehaviour
{
    protected const string cursedRevealInstructionText = "Below are shown all other Cursed players:";
    protected const string eldersRevealInstructionText = "Below are shown all other Elders and their exact cards:";
    protected const string nyxHemeraInstructionText ="Depending on your role (Hemera/Nyx), the player shown below may hold or may not hold the shown card.";
    
    protected int lives = 1;
    protected List<RevealingCard> cardsToReveal = new List<RevealingCard>();
    protected RolesManager.CardName roleName=RolesManager.CardName.None;
    protected RolesManager.Team team=RolesManager.Team.None;
    protected RolesManager.CardClass cardClass=RolesManager.CardClass.None;
    protected int moveIndexes=1<<25;
    protected string moveInstructionText="";
    protected bool revealMyCard = false;

    public class RevealingCard{
        Sprite image;
        string playerName;
        bool hidden;

        public string PlayerName{
            get {return playerName;}
        }
        public Sprite Image{
            get {return image;}
        }
        public bool Hidden{
            get {return hidden;}
        }

        public RevealingCard(Sprite i, string p, bool h=false){
            image=i;
            playerName = p;
            hidden=h;
        }
    }

    public int MoveIndexes{
        get {return moveIndexes;}
    }
    public RolesManager.Team Team{
        get{return team;}
        set{team=value;}
    }
    public RolesManager.CardName Name{
        get{return roleName;}
    }
    public RolesManager.CardClass CardClass{
        get{return cardClass;}
    }
    public string MoveInst{
        get {return moveInstructionText;}
    }
    public bool RevealMyCard{
        get{return revealMyCard;}
    }
    public List<RevealingCard> GetReveals(){
        return cardsToReveal;
    }
    public bool IsDead{
        get{return lives<=0;}
    }
    public virtual bool HasMove(int moveIndex){
        return (moveIndex & moveIndexes) != 0;
    }

    protected virtual void AddToRevealing(RolesManager.CardName role, bool revealCard=true){
        string player = GameManager.Instance.FindPlayerByCard(role);
        if (player=="") return;

        Sprite img = GameManager.Instance.roleInstances[role].GetImage();
        string name = GameManager.Instance.playerNames[player];

        if (revealCard) cardsToReveal.Add(new RevealingCard(img, name, !revealCard));
        else cardsToReveal.Add(new RevealingCard(DisplayManager.QuestionBack, name, !revealCard));
    }

    public virtual void ToNextScene(int moveIndex){
        if (GameManager.Instance.lastRoundMove>moveIndex){
            DisplayManager.ToWaiting();
        } else  {
            ToNextVotingScene(moveIndex);
        }
    }

    public virtual void ToNextVotingScene(int moveIndex){
        if (moveIndex==GameManager.Instance.singleElderPreGameMI){ 
            //ovaj if bi trebalo da bude u elderima ali radi pa cu da ga ostavim
            SceneManager.LoadScene((int)DisplayManager.Scenes.SingleElderPeek);

        } else if (moveIndex==GameManager.Instance.discussionMI){

            GamePlayer.Instance.Role=GamePlayer.Instance.NextRole;
            SceneManager.LoadScene((int)DisplayManager.Scenes.Discussion);

        } else if (moveIndex==GameManager.Instance.forCursedVoteMI){

            SceneManager.LoadScene((int)DisplayManager.Scenes.Voting);

        } else if (moveIndex==GameManager.Instance.forCursedVoteResultMI){

            GameManager.Instance.ProcessVotes(false);
            SceneManager.LoadScene((int)DisplayManager.Scenes.VotingResult);

        } else if (moveIndex==GameManager.Instance.forEldersVoteResultMI){

            GameManager.Instance.ProcessVotes(true);
            SceneManager.LoadScene((int)DisplayManager.Scenes.VotingResult);

        } else if (moveIndex==GameManager.Instance.forEldersVoteMI){

            if (Team==RolesManager.Team.Tartarus){
                SceneManager.LoadScene((int)DisplayManager.Scenes.Voting);
            } else {
                DisplayManager.ToWaiting();
            }

        } else if (moveIndex==GameManager.Instance.elderShowMI){

            if (Team==RolesManager.Team.Tartarus){    
                SceneManager.LoadScene((int)DisplayManager.Scenes.ElderShow);
            } else {
                DisplayManager.ToWaiting();
            }
        }
    }

    protected virtual void FillRevealing(){}

    public virtual void DoActive(string name1="", string name2=""){}

    public virtual void DoVotedForPassive(RolesManager.Team votingFor){}

    public virtual void DoRoundEndPassive(){}

    public virtual void DoDeathEffect(RolesManager.Team votingFor){}

    public virtual void ProtectFromDying(){
        if (CardClass!=RolesManager.CardClass.Elder) return;
        lives++;
    }

    public void AddMove(int mi){
        moveIndexes |= mi;
    }

    public void RemoveMove(int mi){
        moveIndexes &= ~mi;
    }
    
    protected double indexToInt(int m){
        return Math.Log(m)/Math.Log(2);
    }
}
