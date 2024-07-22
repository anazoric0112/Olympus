using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchillesBehaviour : RoleBehaviour
{    
    public AchillesBehaviour(){
        roleName=RolesManager.CardName.Achilles;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Vindictive;
    }

    public override void DoDeathEffect(RolesManager.Team votingFor)
    {
        if (votingFor!=RolesManager.Team.Olympus) return;

        if (!FindObjectOfType<ConnectionManager>().IsServer) return;

        string rng = GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Olympus);
        while (GameManager.Instance.roleInstances[GameManager.Instance.playerCards[rng]].Behaviour.CardClass!=RolesManager.CardClass.Elder){
            rng = GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Olympus);
        }

        RPCsManager.Instance.SwapPlayersFallbackServerRpc(rng, GameManager.Instance.FindPlayerByCard(Name));
    }
}
