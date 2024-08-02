using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtemisBehaviour : RoleBehaviour
{
    public ArtemisBehaviour(){
        roleName=RolesManager.CardName.Artemis;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Vindictive;
        moveIndexes = 1<<2;
        moveInstructionText="Below is shown one random player from Tartarus and their exact card:";
    }

    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;

        if (moveIndex==1<<2){
            cardsToReveal.Clear();
            
            string id = GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Tartarus);
            if (id!=""){
                RolesManager.CardName card = GameManager.Instance.playerCards[id];
                Role role = GameManager.Instance.roleInstances[card];
                string name = GameManager.Instance.playerNames[id];
                cardsToReveal.Add(new RevealingCard(role.Image, name));
            }

            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);
        } else {
            ToNextVotingScene(moveIndex);
        }

    }
    
    public override void DoVotedForPassive(RolesManager.Team votingFor)
    {
        doneDeathEffect = false;
        if (votingFor!=RolesManager.Team.Tartarus) return;
        
        string playerId = GameManager.Instance.FindPlayerByCard(Name);

        foreach(KeyValuePair<string,string> kp in GameManager.Instance.whoVotedForWho){
            string player=kp.Key;
            string hisVote=kp.Value;

            Role r = GameManager.Instance.roleInstances[GameManager.Instance.playerCards[player]];

            if (hisVote!=playerId && 
                r.Behaviour.Team==RolesManager.Team.Olympus && 
                r.Behaviour.Name!=Name) {
                return;
            }
        }
        doneDeathEffect=true;
        GameManager.Instance.SetOnlyVoteCounts(GameManager.Instance.FindPlayerByCard(Name));
    }
}
