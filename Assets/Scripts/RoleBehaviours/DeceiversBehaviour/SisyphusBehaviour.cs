using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SisyphusBehaviour : RoleBehaviour
{
    public SisyphusBehaviour(){
        roleName=RolesManager.CardName.Sisyphus;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Deceiver;
    }

    public override void DoDeathEffect(RolesManager.Team votingFor)
    {
        if (votingFor==RolesManager.Team.Tartarus && Team==RolesManager.Team.Tartarus){
            lives-=1;
        }
    }

    public override void DoRoundEndPassive()
    {
        GameManager.Instance.ChangeTeam(Name);
    }
}
