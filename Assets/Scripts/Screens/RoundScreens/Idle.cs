using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Idle : MonoBehaviour
{
    private float time = 20;
    private float last = 20;
    private float safeTime = 4;

    private bool runout = false;
    private bool runoutsafe = false;

    [SerializeField] TMP_Text debugtext;


    void Update()
    {
        // UpdateScene();
        // UpdateTimeout();
    }

    void UpdateScene(){
        RoleBehaviour r = GamePlayer.Instance.Role.Behaviour;
        int move = GameManager.Instance.MoveIndex;
        safeTime-=Time.deltaTime;

        debugtext.text=r.Name + " "+move.ToString();

        if (safeTime<0 && r.HasMove(move) && !runoutsafe) {
            r.ToNextScene(move);
            runoutsafe=true;
        }
    }

    //###iskreno ne znam da li da se otarasim ove funkcije
    void UpdateTimeout(){
        time-=Time.deltaTime;
        if ((GamePlayer.Instance.Role.Behaviour.HasMove(GameManager.Instance.MoveIndex)
            || GameManager.Instance.MoveIndex==0)
            && WiFiManager.IsConnected() 
            && time<0 && !runout){

            runout=true;
            DisplayManager.GoToNextScene();
            Debug.Log("LOOOL");
        }
        if(last-time>3){
            last=time;
            Debug.Log("Time left in idle: "+time);
        }
    }
}
