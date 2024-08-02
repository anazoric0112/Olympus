using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayersInLobbyDisplay : MonoBehaviour
{    
    private ConnectionManager connectionManager;
    
    [SerializeField] GameObject playerNamePrefab;
    [SerializeField] GameObject scrollableList;
    List<GameObject> playersDisplayed = new List<GameObject>();
    private List<string> playersIds = new List<string>();

    private const float refreshTimerMax = 1f;
    private float refreshTimer = 1f;

    void Awake(){
        connectionManager = FindObjectOfType<ConnectionManager>();
    }

    void Start()
    {
        AddPlayers();
    }

    void Update()
    {
        refreshTimer-= Time.deltaTime;
        if (refreshTimer<0f){
            refreshTimer= refreshTimerMax;
            
            DeletePlayers();
            AddPlayers();
        }

    }

    public void AddPlayer(string name, string id){
        GameObject newPlayer = Instantiate(playerNamePrefab);
        newPlayer.GetComponentInChildren<TextMeshProUGUI>().text = name;

        newPlayer.transform.parent = scrollableList.transform;
        newPlayer.transform.localScale = new Vector3(1,1,1);

        playersDisplayed.Add(newPlayer);
        playersIds.Add(id);
    }
    
    public void AddPlayers(){
        List<Player> playersIn = connectionManager.Players;
        foreach(Player p in playersIn){
            AddPlayer(p.Data["PlayerName"].Value, p.Id);
        }

        int playersY = playersIn.Count*90+10;
        scrollableList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Math.Max(480, playersY+100));
    }

    public void DeletePlayers(){
        foreach (GameObject o in playersDisplayed) Destroy(o);

        playersDisplayed.Clear();
        playersIds.Clear();
    }
}
