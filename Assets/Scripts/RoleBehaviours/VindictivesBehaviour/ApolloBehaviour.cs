using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApolloBehaviour : RoleBehaviour
{
    public ApolloBehaviour(){
        roleName=RolesManager.CardName.Apollo;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Vindictive;
    }

    public override void DoVotedForPassive(RolesManager.Team votingFor)
    {
        doneDeathEffect=false;
        if (votingFor!=RolesManager.Team.Tartarus) return;
        
        doneDeathEffect=true;
        GameManager.Instance.SetOnlyVoteCounts(GameManager.Instance.FindPlayerByCard(Name));
    }
}
