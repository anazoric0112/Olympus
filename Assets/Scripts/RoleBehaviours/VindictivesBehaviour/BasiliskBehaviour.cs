using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasiliskBehaviour : RoleBehaviour
{
    public BasiliskBehaviour(){
        roleName=RolesManager.CardName.Basilisk;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Vindictive;
    }

    public override void DoDeathEffect(RolesManager.Team votingFor)
    {
        Debug.Log(Name.ToString()+"DoDeathEffect called");
        if (votingFor!=RolesManager.Team.Olympus) return;

        foreach(KeyValuePair<string,RolesManager.CardName> kp in GameManager.Instance.playerCards){
            Role r = GameManager.Instance.roleInstances[kp.Value];
            if (r.Behaviour.Team!=RolesManager.Team.Tartarus) continue;
            
            GameManager.Instance.PlayerOut(kp.Key);
        }
    }
}
