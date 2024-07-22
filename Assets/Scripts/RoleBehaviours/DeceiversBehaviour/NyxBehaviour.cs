using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NyxBehaviour : RoleBehaviour
{
    public NyxBehaviour(){
        roleName=RolesManager.CardName.Nyx;
        team=RolesManager.Team.Olympus;
        cardClass=RolesManager.CardClass.Deceiver;
        moveIndexes = 1<<2;
        moveInstructionText=nyxHemeraInstructionText;
    }

    public override void ToNextScene(int moveIndex){
        if (!HasMove(moveIndex)) return;


        if (moveIndex==1<<2){
            cardsToReveal.Clear();
        
            //get someone from olympus
            string myId=AuthenticationService.Instance.PlayerId;
            string ido=myId;
            while (ido==myId){
                ido = GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Olympus);
            }
            //get someone from tartarus
            string idt = GameManager.Instance.GetRandomPlayerIdFromTeam(RolesManager.Team.Tartarus);

            if (ido!="" && idt!=""){
                //take image from tartarus and name from olympus
                RolesManager.CardName card = GameManager.Instance.playerCards[idt];
                Sprite img = GameManager.Instance.roleInstances[card].GetImage();
                string name = GameManager.Instance.playerNames[ido];
                
                cardsToReveal.Add(new RevealingCard(img,name));
            }
            
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsPeek);
        }else {
            ToNextVotingScene(moveIndex);
        }
    }
}
