using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AthenaBehaviour : RoleBehaviour
{
    private bool firstMove=true;
    private string secondMoveInstructionText = "Below is shown one random player from Tartarus and their exact card:";

    public AthenaBehaviour(){
        roleName=RolesManager.CardName.Athena;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Elder;
        moveIndexes = (1<<2) | (1<<3);
    }

    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;
        Debug.Log(Name.ToString()+" ToNextScene called with index"+indexToInt(moveIndex));
        
        if (moveIndex==1<<2){
            moveInstructionText=eldersRevealInstructionText;
            FillRevealing();
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);

        } else if (moveIndex==1<<3){
            moveInstructionText=secondMoveInstructionText;

            cardsToReveal.Clear();
        
            string id = GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Tartarus);
            if (id!=""){
                RolesManager.CardName card = GameManager.Instance.playerCards[id];
                Role role = GameManager.Instance.roleInstances[card];
                string name = GameManager.Instance.playerNames[id];
                cardsToReveal.Add(new RevealingCard(role.GetImage(),name));
            }
            
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);
            
        } else {
            ToNextVotingScene(moveIndex);
        }
    }

    protected override void FillRevealing(){
        cardsToReveal.Clear();
        AddToRevealing(RolesManager.CardName.Aphrodite);
        AddToRevealing(RolesManager.CardName.Zeus);
        AddToRevealing(RolesManager.CardName.Dionysis);
    }
    
    public override bool HasMove(int moveIndex)
    {
        return (moveIndex!=1<<3 && (moveIndex&moveIndexes)!=0) || 
                (moveIndex==1<<3 && (GameManager.Instance.RoundNumber-2)%3==0);
    }
    
    public override void DoDeathEffect(RolesManager.Team votingFor){
        Debug.Log(Name.ToString()+"DoDeathEffect called");
        lives-=1;
    }
}
