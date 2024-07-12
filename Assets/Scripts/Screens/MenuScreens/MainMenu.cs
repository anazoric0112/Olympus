using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button createGame;
    [SerializeField] Button joinGame;
    [SerializeField] Button goToHelp;
    [SerializeField] Button goToCards;
    void Awake(){

        createGame.onClick.AddListener(()=>{
            SceneManager.LoadScene((int)DisplayManager.Scenes.HostJoin);
        });
        joinGame.onClick.AddListener(()=>{
            SceneManager.LoadScene((int)DisplayManager.Scenes.ClientJoin);
        });
        goToHelp.onClick.AddListener(()=>{
            SceneManager.LoadScene((int)DisplayManager.Scenes.RulesHelp);
        });
        goToCards.onClick.AddListener(()=>{
            SceneManager.LoadScene((int)DisplayManager.Scenes.CardsHelp);
        });
    }
}
