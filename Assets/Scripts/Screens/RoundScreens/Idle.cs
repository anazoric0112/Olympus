using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Idle : MonoBehaviour
{
    [SerializeField] GameObject timer;
    [SerializeField] Image bottomFill;
    [SerializeField] Image upFill;
    
    private float stepRotation = 3f;
    private float maxTime = 2;
    private float currentTime = 2;
    private bool timerRunning = true;
    private int timerRun =1;
    private float zprev = 0;

    void Start(){
        currentTime=maxTime;
    }

    void Update()
    {
        if (timerRunning) UpdateTime();
        else UpdateRotation();
    }

    private void UpdateTime(){
        currentTime-=Time.deltaTime;
        if (timerRun%2==0){
            bottomFill.fillAmount=GetFill();
            upFill.fillAmount=1-GetFill();
        } else {
            upFill.fillAmount = GetFill();
            bottomFill.fillAmount = 1-GetFill();
        }
        
        if (currentTime<=0) {
            timerRunning=false;
            currentTime=maxTime;
        }
    }
    
    private void UpdateRotation(){
        timer.transform.Rotate(0,0,stepRotation);
        float z=timer.transform.rotation.z;
        
        if (z==1.00 || z==-1.00 || z==0 || (zprev*z<0)){
            timerRun++;
            if (timerRun%2==0){
                upFill.fillOrigin=(int)Image.OriginVertical.Top;
                upFill.fillAmount=0;

                bottomFill.fillOrigin=(int)Image.OriginVertical.Top;
                bottomFill.fillAmount=1;
            } else{
                upFill.fillOrigin=(int)Image.OriginVertical.Bottom;
                upFill.fillAmount=1;

                bottomFill.fillOrigin=(int)Image.OriginVertical.Bottom;
                bottomFill.fillAmount=0;
            }
            timerRunning=true;
        }
        zprev=z;
    }

    private float GetFill(){
        if (currentTime<0) return 0;
        return currentTime/maxTime;
    }
}
