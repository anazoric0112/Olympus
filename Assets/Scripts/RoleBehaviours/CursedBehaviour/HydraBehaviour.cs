using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraBehaviour : RoleBehaviour
{
    public HydraBehaviour(){
        roleName=RolesManager.CardName.Hydra;
        team=RolesManager.Team.Tartarus;
        cardClass=RolesManager.CardClass.Cursed;
        moveIndexes = 0;
    }
    
    public override void DoDeathEffect(RolesManager.Team votingFor){
        Debug.Log(Name.ToString()+"DoDeathEffect called");
        if(votingFor==RolesManager.Team.Tartarus) lives-=1;
    }
}
