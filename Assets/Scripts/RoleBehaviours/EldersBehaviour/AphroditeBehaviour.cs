using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AphroditeBehaviour : RoleBehaviour
{
    public AphroditeBehaviour(){
        roleName=RolesManager.CardName.Aphrodite;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Elder;
        moveIndexes = 1<<2;
        moveInstructionText=eldersRevealInstructionText;
    }

    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;

        if (moveIndex==1<<2){
            FillRevealing();
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);
        }else {
            ToNextVotingScene(moveIndex);
        }
    }
    
    protected override void FillRevealing(){
        cardsToReveal.Clear();
        AddToRevealing(RolesManager.CardName.Athena);
        AddToRevealing(RolesManager.CardName.Zeus);
        AddToRevealing(RolesManager.CardName.Dionysis);
    }
    public override void DoDeathEffect(RolesManager.Team votingFor){
        lives-=1;
    }
}
