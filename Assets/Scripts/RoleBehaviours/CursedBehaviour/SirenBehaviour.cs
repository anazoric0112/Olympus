using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SirenBehaviour : RoleBehaviour
{
    public SirenBehaviour(){
        roleName=RolesManager.CardName.Siren;
        team=RolesManager.Team.Tartarus;
        cardClass=RolesManager.CardClass.Cursed;
        moveIndexes = 1<<2;
        moveInstructionText=cursedRevealInstructionText;
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
        AddToRevealing(RolesManager.CardName.Charon, false);
        AddToRevealing(RolesManager.CardName.Hades, false);
        AddToRevealing(RolesManager.CardName.Medusa, false);
        AddToRevealing(RolesManager.CardName.Hydra, false);
    }
    
    public override void DoDeathEffect(RolesManager.Team votingFor){
        if(votingFor==RolesManager.Team.Tartarus) lives-=1;
    }
}
