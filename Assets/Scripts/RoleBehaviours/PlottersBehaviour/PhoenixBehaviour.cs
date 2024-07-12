using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoenixBehaviour : RoleBehaviour
{
    public PhoenixBehaviour(){
        roleName=RolesManager.CardName.Phoenix;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Plotter;
        moveIndexes=1<<1;
        
        moveInstructionText = "Your card for this round:";
    }

    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;
        Debug.Log(Name.ToString()+" ToNextScene called with index"+indexToInt(moveIndex));

        if (moveIndex==1<<1){
            string card = GameManager.Instance.GetRandomTableCard();
            RolesManager.CardName cardName = (RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName), card);
            Role newrole = GameManager.Instance.roleInstances[cardName];
            
            GamePlayer.Instance.FallbackRole = GamePlayer.Instance.Role;
            GamePlayer.Instance.Role=newrole;
            GamePlayer.Instance.NextRole=newrole;

            RPCsManager.Instance.PhoenixRoleChangeServerRpc(GamePlayer.Instance.Id, card);
            SceneManager.LoadScene((int)DisplayManager.Scenes.NewRoleDisplay);
        } else {
            ToNextVotingScene(moveIndex);
        }
    }
}
