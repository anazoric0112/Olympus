using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PegasusBehaviour : RoleBehaviour
{
    public PegasusBehaviour(){
        roleName=RolesManager.CardName.Pegasus;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Plotter;
        moveIndexes=1<<4;
        
        moveInstructionText = "Choose a player to protect them from being voted out in voting for Elders by Tartarus in this round:";
    }
    
    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;
        Debug.Log(Name.ToString()+" ToNextScene called with index"+indexToInt(moveIndex));

        if (moveIndex==1<<4){
            SceneManager.LoadScene((int)DisplayManager.Scenes.ChoosePlayers);
        }else {
            ToNextVotingScene(moveIndex);
        }
    }

    public override void DoActive(string name1 = "", string name2 = "")
    {
        string id1 = GameManager.Instance.FindPlayerByName(name1);

        RPCsManager.Instance.ProtectServerRpc(id1);
    }
}
