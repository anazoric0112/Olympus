using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DracaenaBehaviour : RoleBehaviour
{
    public DracaenaBehaviour(){
        roleName=RolesManager.CardName.Dracaena;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Plotter;
        moveIndexes=1<<7;
        revealMyCard=true;
        
        moveInstructionText = "Choose a player to take their card (they won't be notified). Your card goes to the table afterwards, and they get a random card from the table:";
    }
    
    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;
        Debug.Log(Name.ToString()+" ToNextScene called with index"+indexToInt(moveIndex));

        if (moveIndex==1<<7){
            SceneManager.LoadScene((int)DisplayManager.Scenes.ChoosePlayers);
        }else {
            ToNextVotingScene(moveIndex);
        }
    }

    public override void DoActive(string name1 = "", string name2 = "")
    {
        string id1 = GameManager.Instance.FindPlayerByName(name1);
        string id2 = GamePlayer.Instance.Id;
        
        RolesManager.CardName newCard = GameManager.Instance.playerCards[id1];
        GamePlayer.Instance.NextRole = GameManager.Instance.roleInstances[newCard];
        
        RPCsManager.Instance.TableSwapPlayersServerRpc(id2,id1,GameManager.Instance.GetRandomTableCard());
    }
}
