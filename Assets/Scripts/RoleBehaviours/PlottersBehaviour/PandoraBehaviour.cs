using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PandoraBehaviour : RoleBehaviour
{
    public PandoraBehaviour(){
        roleName=RolesManager.CardName.Pandora;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Plotter;
        moveIndexes=1<<5;
        
        moveInstructionText = "Choose 2 players to swap their cards (they won't be notified):";
    }
    
    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;

        if (moveIndex==1<<5){
            SceneManager.LoadScene((int)DisplayManager.Scenes.ChoosePlayers);
        }else {
            ToNextVotingScene(moveIndex);
        }
    }
    
    public override void DoActive(string name1 = "", string name2 = "")
    {
        string id1 = GameManager.Instance.FindPlayerByName(name1);
        string id2 = GameManager.Instance.FindPlayerByName(name2);

        RPCsManager.Instance.SwapPlayersServerRpc(id1, id2);
    }
}
