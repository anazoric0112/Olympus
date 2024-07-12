using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrpheusBehaviour : RoleBehaviour
{
    public OrpheusBehaviour(){
        roleName=RolesManager.CardName.Orpheus;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Deceiver;
    }

    public override void DoDeathEffect(RolesManager.Team votingFor)
    {
        Debug.Log(Name.ToString()+" DoDeathEffect called");
        if (votingFor==Team){
            GameManager.Instance.ChangeTeam(Name);
        }
    }
}
