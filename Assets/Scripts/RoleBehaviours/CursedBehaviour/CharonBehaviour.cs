using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharonBehaviour : RoleBehaviour
{
    private string secondMoveInstructionText="Choose a player from Olympus to see their card:";
    
    public CharonBehaviour(){
        roleName=RolesManager.CardName.Charon;
        team=RolesManager.Team.Tartarus;
        cardClass=RolesManager.CardClass.Cursed;
        moveIndexes = 1<<2 | 1<<9;
    }

    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;

        Debug.Log(Name.ToString()+" ToNextScene called with index"+indexToInt(moveIndex));

        if (moveIndex==1<<2){
            moveInstructionText=cursedRevealInstructionText;
            FillRevealing();
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);

        } else if (moveIndex==1<<9){
            moveInstructionText=secondMoveInstructionText;
            FillWithOlympus();
            SceneManager.LoadScene((int)DisplayManager.Scenes.InfoPeek);
        } else {
            ToNextVotingScene(moveIndex);
        }
    }
    protected override void FillRevealing(){
        cardsToReveal.Clear();
        AddToRevealing(RolesManager.CardName.Hades, false);
        AddToRevealing(RolesManager.CardName.Medusa, false);
        AddToRevealing(RolesManager.CardName.Siren, false);
        AddToRevealing(RolesManager.CardName.Hydra, false);
    }

    private void FillWithOlympus(){
        cardsToReveal.Clear();
        foreach(KeyValuePair<string,RolesManager.CardName> kp in GameManager.Instance.playerCards){
            if (kp.Key==GamePlayer.Instance.Id) continue;

            RolesManager.CardName card = kp.Value;
            if (GameManager.Instance.roleInstances[card].Behaviour.Team==RolesManager.Team.Olympus){
                AddToRevealing(card, true);
            }
        }

        if (cardsToReveal.Count==0) return;
        
        for(int i=0;i<50;i++){
            int rng1=UnityEngine.Random.Range(0,cardsToReveal.Count);
            int rng2=UnityEngine.Random.Range(0,cardsToReveal.Count);

            RevealingCard t = cardsToReveal[rng1];
            cardsToReveal[rng1]=cardsToReveal[rng2];
            cardsToReveal[rng2]=t;
        }
    }    
    
    public override void DoDeathEffect(RolesManager.Team votingFor){
        Debug.Log(Name.ToString()+"DoDeathEffect called");
        if(votingFor==RolesManager.Team.Tartarus) lives-=1;
    }
}
