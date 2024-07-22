using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerseusBehaviour : RoleBehaviour
{
    public PerseusBehaviour(){
        roleName=RolesManager.CardName.Perseus;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Vindictive;
    }

    public override void DoDeathEffect(RolesManager.Team votingFor)
    {
        if (votingFor!=RolesManager.Team.Olympus) return;

        GameManager.Instance.PlayerOut(GameManager.Instance.FindPlayerByCard(RolesManager.CardName.Medusa));
    }
}
