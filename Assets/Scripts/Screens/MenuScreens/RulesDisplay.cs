using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulesDisplay : MonoBehaviour
{
    [SerializeField] Button backButton;
    
    void Awake(){

        backButton.onClick.AddListener(()=>{
            DisplayManager.BackToStart();
        });
    }
}
