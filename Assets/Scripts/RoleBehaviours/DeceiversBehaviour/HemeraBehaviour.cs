using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HemeraBehaviour : RoleBehaviour
{
    public HemeraBehaviour(){
        roleName=RolesManager.CardName.Hemera;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Deceiver;
        moveIndexes = 1<<2;
        moveInstructionText=nyxHemeraInstructionText;
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
                cardsToReveal.Add(new RevealingCard(role.Image,name));
            }
            
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);
        }else {
            ToNextVotingScene(moveIndex);
        }

    }
}
