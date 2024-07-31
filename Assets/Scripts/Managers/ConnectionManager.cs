using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ConnectionManager : NetworkBehaviour
{
    public class WrongGameCodeException : Exception {
        public WrongGameCodeException(){}

        public WrongGameCodeException(string message)
            : base(message){}

        public WrongGameCodeException(string message, Exception inner)
            : base(message, inner){}
    }

    enum PreGamePhase{
        Lobby,  //0
        Select, //1
        Game    //2
    }

    private const string lobbyName="OlimpGameLobby";
    private const string key_game_code = "#KeyGameCode";
    private const string pre_game_phase = "#PreGamePhase";
    private const string key_table_cards = "#TableCards";
    private const string key_assigned_cards = "#AssignedCards";
    private const int lobbyMaxN=24;
    private const float heartbeatMaxTime=20;
    private float heartbeatTimer=20;
    private const float reloadMaxTime=1;
    private float reloadTimer=1;


    static ConnectionManager instance;
    private GameManager gameManager=null;
    private Lobby lobby=null;
    private bool signedIn=false;
    private Player player;
    private bool asyncOngoing = false;
    private string current = PreGamePhase.Lobby.ToString();
    private string gameCode ="";

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (instance){
            instance.gameManager = FindObjectOfType<GameManager>();
            gameObject.SetActive(false);
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        try{
            if (signedIn) return;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            signedIn=true;
            Debug.Log("Authenticated: "+AuthenticationService.Instance.PlayerId);
        } catch(AuthenticationException e){
            Debug.Log("Error: ConnectionManager.Start() "+ e);
        }
    }

    void Update()
    {
        HeartbeatLobby();
        ReloadLobby();
    }

    private async void HeartbeatLobby(){
        if (lobby==null) return;

        heartbeatTimer  -= Time.deltaTime;
        if (heartbeatTimer<0f){
            heartbeatTimer = heartbeatMaxTime;
            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
        }
    }
        
    private async void ReloadLobby(){
        if (lobby==null) return;
        reloadTimer-=Time.deltaTime;
        if (asyncOngoing) return;

        if (reloadTimer<0f){
            reloadTimer=reloadMaxTime;
            
            try{
                lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                string phase = lobby.Data[pre_game_phase].Value;

                if (phase==PreGamePhase.Select.ToString() && current!=phase){
                    current = phase;
                    DisplayManager.ToSelect();
                }

                if (phase==PreGamePhase.Game.ToString() && current!=phase){

                    if (!IsLobbyHost()){
                        await JoinRelay(lobby.Data[key_game_code].Value);
                        gameCode = lobby.Data[key_game_code].Value;
                    }
                    current = phase;

                    gameManager.FillPlayers(lobby.Players);
                    gameManager.FillPlayerRoles(lobby.Data[key_assigned_cards].Value);
                    gameManager.SetTableCards(lobby.Data[key_table_cards].Value);

                    lobby = null;
                    
                    DisplayManager.ToGameStart();
                }
            }catch(LobbyServiceException e){
                Debug.Log("Error: ConnectionManager.ReloadLobby() "+ e);
            }
        }
    }

    public async Task CreateLobby(string hostName){
        asyncOngoing=true;

        try{
            player = new Player{
                Data = new Dictionary<string,PlayerDataObject>{
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,  hostName)}
                }
            };
            CreateLobbyOptions opt =new CreateLobbyOptions{
                IsPrivate = false,
                Player = player,
                Data = new Dictionary<string, DataObject>{
                    {key_game_code, new DataObject(DataObject.VisibilityOptions.Member,"0")},
                    {pre_game_phase, new DataObject(DataObject.VisibilityOptions.Member, PreGamePhase.Lobby.ToString())}
                }
            };

            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, lobbyMaxN, opt);

            Debug.Log("Created lobby "+lobby.Id + " " + lobby.LobbyCode +" by "+ GetHostId());
        } catch (LobbyServiceException e) {
            Debug.Log("Error: ConnectionManager.CreateLobby() "+ e);
            throw e;
        }

        asyncOngoing=false;
    }

    public async Task JoinLobbyByCode(string playerName, string gameCode){
        asyncOngoing=true;

        try {
            player =new Player{
                Data = new Dictionary<string, PlayerDataObject>{
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                }
            };
            JoinLobbyByCodeOptions opt = new JoinLobbyByCodeOptions{
                Player = player
            };
            string gameCodeUpperCase = "";
            foreach(char c in gameCode) gameCodeUpperCase+=Char.ToUpper(c);
            lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(gameCodeUpperCase, opt);

            List<Player> ps = lobby.Players;
            int cntWithSameName=0;
            foreach(Player p in ps){
                if (p.Data["PlayerName"].Value==playerName) cntWithSameName++;
            }

            if (cntWithSameName!=1){
                await LeaveLobby();
                throw new Exception("Player with that name already exists");
            }

            Debug.Log("Joined to "+lobby.Id+ " with code " +gameCode);
        } catch (Exception e){
            Debug.Log("Error: ConnectionManager.JoinLobbyByCode() "+ e);
            throw new Exception(e.ToString());
        }
        asyncOngoing=false;
    }

    public async Task LeaveLobby(){
        asyncOngoing=true;

        try {
            string playerId = AuthenticationService.Instance.PlayerId;
            
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerId);
            lobby = null;
            current = PreGamePhase.Lobby.ToString();

            Debug.Log("Left lobby "+playerId);
        } catch (LobbyServiceException e){
            Debug.Log("Error: ConnectionManager.LeaveLobby() "+ e);
        }

        asyncOngoing=false;
    }

    private async Task<string> CreateRelay(){
        asyncOngoing=true;
        string relayCode = null;

        try{
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(lobbyMaxN);
            relayCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            RelayServerData data = new RelayServerData(alloc, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            NetworkManager.Singleton.StartHost();
        } catch (RelayServiceException e) {
            Debug.Log("Error: ConnectionManager.CreateRelay() "+ e);
        }

        asyncOngoing=false;
        gameCode=relayCode;
        return relayCode;
    }

    private async Task JoinRelay(string code){
        asyncOngoing=true;

        try{
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(code);

            RelayServerData data = new RelayServerData(alloc, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e) {
            Debug.Log("Error: ConnectionManager.JoinRelay() "+ e);
        }

        asyncOngoing=false;
    }

    public void LeaveRelay(){
        NetworkManager.Singleton.Shutdown();
    }
    
    public async Task MoveToSelect(){
        if (!IsLobbyHost() || lobby==null) return;
        asyncOngoing=true;

        try{
            lobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, 
                new UpdateLobbyOptions{
                    Data = new Dictionary<string, DataObject>{
                        {key_game_code, new DataObject(DataObject.VisibilityOptions.Member, "0")},
                        {pre_game_phase, new DataObject(DataObject.VisibilityOptions.Member, PreGamePhase.Select.ToString())}
                    }
                }
            );
        } catch (LobbyServiceException e){
            Debug.Log("Error: ConnectionManager.MoveToSelect() "+ e);
            throw e;
        }
        asyncOngoing=false;
    }

    public async Task StartGame(List<Role> assignedRoles){

        if (!IsLobbyHost() || lobby==null) return;
        asyncOngoing=true;

        try{
            if (assignedRoles.Count==0) throw new Exception("No roles chosen");
            string relayCode = await CreateRelay();

            Dictionary<string,DataObject> optData = new Dictionary<string, DataObject>{
                {key_game_code, new DataObject(DataObject.VisibilityOptions.Member, relayCode)},
                {pre_game_phase, new DataObject(DataObject.VisibilityOptions.Member, PreGamePhase.Game.ToString())}
            };

            int roleIndex=0;
            string assignedCards = "";
            string tableCards ="";

            foreach(Player p in lobby.Players){
                if (assignedCards!="") assignedCards+="#";
                assignedCards+=p.Id+":"+assignedRoles[roleIndex++].GetName();
            }
            while (roleIndex<assignedRoles.Count){
                if (tableCards!="") tableCards+="#";
                tableCards+=assignedRoles[roleIndex++].GetName();
            }
            optData[key_table_cards]=new DataObject(DataObject.VisibilityOptions.Member, tableCards);
            optData[key_assigned_cards]=new DataObject(DataObject.VisibilityOptions.Member, assignedCards);

            lobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, 
                new UpdateLobbyOptions{
                    Data = optData
                }
            );
            Debug.Log("Relay started "+relayCode);
            Debug.Log("Generated "+assignedCards+" "+tableCards);
        } catch (Exception e){
            Debug.Log("Error: ConnectionManager.StartGame() "+ e);
            throw e;
        }
        asyncOngoing=false;
    }

    public async Task RejoinRelay(){
        await JoinRelay(gameCode);
    }

    public string GetLobbyCode(){
        return lobby.LobbyCode;
    }

    public string GetHostId(){
        return lobby.HostId;
    }

    public bool IsLobbyHost(){
        return lobby.HostId==AuthenticationService.Instance.PlayerId;
    }

    public List<Player> GetPlayers(){
        return lobby.Players;
    }

    public bool GetAsyncOngoing(){
        return asyncOngoing;
    }

    public Player GetPlayer(){
        return player;
    }
}
