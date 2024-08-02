using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CassandraBehaviour : RoleBehaviour
{    
    public CassandraBehaviour(){
        roleName=RolesManager.CardName.Cassandra;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Plotter;
        moveIndexes=1<<8;
        
        int tableCardsCnt = GameManager.Instance.TableCardsCnt;
        int cnt = tableCardsCnt<5 ? 1:2;
        moveInstructionText = "Choose " + cnt +" cards from the table to see them:";
    }
    
    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;

        if (moveIndex==1<<8){
            FillRevealing();
            SceneManager.LoadScene((int)DisplayManager.Scenes.InfoPeek);
        }else {
            ToNextVotingScene(moveIndex);
        }
    }

    protected override void FillRevealing(){
        cardsToReveal.Clear();
        
        foreach(Role role in GameManager.Instance.tableCards){
            Sprite img = role.Image;
            cardsToReveal.Add(new RevealingCard(img, role.Name, true));
        }
        for(int i=0;i<10;i++){
            int rng1=UnityEngine.Random.Range(0,cardsToReveal.Count);
            int rng2=UnityEngine.Random.Range(0,cardsToReveal.Count);

            RevealingCard t = cardsToReveal[rng1];
            cardsToReveal[rng1]=cardsToReveal[rng2];
            cardsToReveal[rng2]=t;
        }
    }
}
