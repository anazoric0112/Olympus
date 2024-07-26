using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DionysisBehaviour : RoleBehaviour
{
    public DionysisBehaviour(){
        roleName=RolesManager.CardName.Dionysis;
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
        AddToRevealing(RolesManager.CardName.Aphrodite);
        AddToRevealing(RolesManager.CardName.Zeus);
        AddToRevealing(RolesManager.CardName.Athena);
    }

    public override void DoDeathEffect(RolesManager.Team votingFor){
        lives-=1;
        if (votingFor!=RolesManager.Team.Olympus) return;
        
        string playerId = GameManager.Instance.FindPlayerByCard(Name);

        foreach(KeyValuePair<string,string> kp in GameManager.Instance.whoVotedForWho){
            string player=kp.Key;
            string hisVote=kp.Value;

            Role r = GameManager.Instance.roleInstances[GameManager.Instance.playerCards[player]];
            RolesManager.CardClass cardClass=r.Behaviour.CardClass;

            if (hisVote!=playerId && cardClass==RolesManager.CardClass.Cursed) {
                GameManager.Instance.PlayerOut(player);
            }
        }
    }
}
