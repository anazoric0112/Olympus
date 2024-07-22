using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MedusaBehaviour : RoleBehaviour
{
    public MedusaBehaviour(){
        roleName=RolesManager.CardName.Medusa;
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
        AddToRevealing(RolesManager.CardName.Siren, false);
        AddToRevealing(RolesManager.CardName.Hydra, false);
    }
    
    public override void DoDeathEffect(RolesManager.Team votingFor){
        if(votingFor!=RolesManager.Team.Tartarus) return;
                
        string playerId = GameManager.Instance.FindPlayerByCard(Name);
                
        foreach(KeyValuePair<string,string> kp in GameManager.Instance.whoVotedForWho){
            string player=kp.Key;
            string hisVote=kp.Value;

            Role r =GameManager.Instance.roleInstances[GameManager.Instance.playerCards[player]];

            if (hisVote!=playerId && r.Behaviour.Team==RolesManager.Team.Olympus) return;
        }

        lives-=1;
    }
}
