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
using System.Threading;

public class ConnectionManager : NetworkBehaviour
{
    enum PreGamePhase{
        Lobby,  //0 - serves as Idle state too
        Select, //1 - selecting cards
        Game,   //2 - in game
        Rejoin, //3 - migrating relay host
    }

    //-------Data dictionary keys-------
    private const string lobbyName="OlimpGameLobby";
    private const string key_game_code = "#KeyGameCode";
    private const string pre_game_phase = "#PreGamePhase";
    private const string key_table_cards = "#TableCards";
    private const string key_assigned_cards = "#AssignedCards";
    private const string key_relay_host = "#RelayHost";
    private const string key_player_cnt = "#PlayersCnt";

    //-------Lobby properties-------
    private const int lobbyMaxN=24;
    private const float heartbeatMaxTime=20;
    private float heartbeatTimer=20;
    private float reloadMaxTime=1;
    private float reloadTimer=1;

    //-------fields-------
    static ConnectionManager instance;
    private Lobby lobby=null;
    private bool signedIn=false;
    private Player player;
    private bool asyncOngoing = false;
    private string current = PreGamePhase.Lobby.ToString();
    private string gameCode ="";
    private bool migrationOnGoing = false;
    private string relayHost = "";
    private bool pauseReload = false;
    private int playersToJoin = 0;
    private int playersJoined = 0;
    private bool imInLastOut = false;

    //-------properties-------
    public bool MigrationOngoing{
        get {return migrationOnGoing;}
    }
    public string LobbyCode{
        get {return lobby.LobbyCode;}
    }
    public string HostId{
        get {return lobby.HostId;}
    }
    private bool IsLobbyHost{
        get {return lobby.HostId==AuthenticationService.Instance.PlayerId;}
    }
    public List<Player> Players{
        get {return lobby.Players;}
    }

    //-------Unity methods-------
    void Awake()
    {   
        if (instance){
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

    //------------------------------------------------------------
    //Methods for Lobby manipulation
    //------------------------------------------------------------

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

        if (reloadTimer<0f && !pauseReload){
            reloadTimer=reloadMaxTime;
            
            try{
                lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
                string phase = lobby.Data[pre_game_phase].Value;

                if (phase==PreGamePhase.Rejoin.ToString() && current!=phase){

                    gameCode = lobby.Data[key_game_code].Value;
                    relayHost = lobby.Data[key_relay_host].Value;

                    if (relayHost!=GamePlayer.Instance.Id){
                        await JoinRelay(gameCode);
                        StartCoroutine(ReportJoinedToRelay());
                        Debug.Log("3 Joined new relay");
                    }
                    current = PreGamePhase.Lobby.ToString();
                    lobby = null;
                    reloadMaxTime = 1;
                    Debug.Log("23 migration done");
                    return;
                } 
                else if (phase==PreGamePhase.Select.ToString() && current!=phase){
                    DisplayManager.ToSelect();
                } 
                else if (phase==PreGamePhase.Game.ToString() && current!=phase){

                    gameCode = lobby.Data[key_game_code].Value;
                    relayHost = lobby.Data[key_relay_host].Value;

                    if (!IsLobbyHost) await JoinRelay(gameCode);

                    GameManager.Instance.ResetGame();
                    GameManager.Instance.FillPlayers(lobby.Players);
                    GameManager.Instance.FillPlayerRoles(lobby.Data[key_assigned_cards].Value);
                    GameManager.Instance.SetTableCards(lobby.Data[key_table_cards].Value);

                    lobby = null;
                    imInLastOut = false;
                    DisplayManager.ToGameStart();
                }
                
                current = phase;
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
        } catch (LobbyServiceException e) {
            Debug.Log("Error: ConnectionManager.CreateLobby() "+ e);
            throw e;
        }

        asyncOngoing=false;
    }

    public async Task JoinLobbyByCode(string playerName, string code){
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
            foreach(char c in code) gameCodeUpperCase+=Char.ToUpper(c);
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
        } catch (Exception e){
            Debug.Log("Error: ConnectionManager.JoinLobbyByCode() "+ e);
            throw new Exception(e.Message);
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
        } catch (LobbyServiceException e){
            Debug.Log("Error: ConnectionManager.LeaveLobby() "+ e);
        }

        asyncOngoing=false;
    }
    
    //------------------------------------------------------------
    //Methods for Relay manipulation
    //------------------------------------------------------------

    private async Task<string> CreateRelay(){
        asyncOngoing=true;
        string relayCode = null;

        try{
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(lobbyMaxN);
            relayCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            RelayServerData data = new RelayServerData(alloc, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            NetworkManager.Singleton.StartHost();
            relayHost = AuthenticationService.Instance.PlayerId;
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

    public async void LeaveRelay(string newHost, int playersLeft){
        string myId = GamePlayer.Instance.Id;
        
        GameManager gm = GameManager.Instance;
        int resultIndexes = gm.forCursedVoteResultMI | gm.forEldersVoteResultMI;

        if (myId == relayHost && (gm.MoveIndex & resultIndexes)!=0){
            CreateRejoinLobbyClientRpc(newHost, playersLeft);
        } else {
            NetworkManager.Singleton.Shutdown();
        }
    }

    public async Task RejoinRelay(){
        // currently isnt solved ### to be done
    }
        
    //------------------------------------------------------------
    //Methods for moving through pregame phases
    //------------------------------------------------------------

    public async Task MoveToSelect(){
        if (!IsLobbyHost || lobby==null) return;
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

        if (!IsLobbyHost || lobby==null) return;
        asyncOngoing=true;

        try{
            if (assignedRoles.Count==0) throw new Exception("No roles chosen");
            gameCode = await CreateRelay();

            Dictionary<string,DataObject> optData = new Dictionary<string, DataObject>{
                {key_game_code, new DataObject(DataObject.VisibilityOptions.Member, gameCode)},
                {pre_game_phase, new DataObject(DataObject.VisibilityOptions.Member, PreGamePhase.Game.ToString())},
                {key_relay_host, new DataObject(DataObject.VisibilityOptions.Member, AuthenticationService.Instance.PlayerId)}
            };

            int roleIndex=0;
            string assignedCards = "";
            string tableCards ="";

            foreach(Player p in lobby.Players){
                if (assignedCards!="") assignedCards+="#";
                assignedCards+=p.Id+":"+assignedRoles[roleIndex++].Name;
            }
            while (roleIndex<assignedRoles.Count){
                if (tableCards!="") tableCards+="#";
                tableCards+=assignedRoles[roleIndex++].Name;
            }
            optData[key_table_cards]=new DataObject(DataObject.VisibilityOptions.Member, tableCards);
            optData[key_assigned_cards]=new DataObject(DataObject.VisibilityOptions.Member, assignedCards);

            lobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, 
                new UpdateLobbyOptions{
                    Data = optData
                }
            );
        } catch (Exception e){
            Debug.Log("Error: ConnectionManager.StartGame() "+ e);
            throw e;
        }
        asyncOngoing=false;
    }
        
    //------------------------------------------------------------
    //Methods for migrating relay host
    //------------------------------------------------------------

    [ClientRpc(RequireOwnership = false)]
    private void CreateRejoinLobbyClientRpc(string newhost, int playersLeft){
        imInLastOut = GameManager.Instance.playersOut.Contains(GamePlayer.Instance.Id);
        if (!imInLastOut) migrationOnGoing = true;

        if (GamePlayer.Instance.Id!=newhost) return;
        
        playersToJoin = playersLeft;
        playersJoined = 0;
        Debug.Log("2 Players to join: "+playersToJoin);
        CreateRejoinLobby();
    }

    private async Task CreateRejoinLobby(){
        pauseReload=true;
        string myId = GamePlayer.Instance.Id;
        try{
            int cnt = GameManager.Instance.playersIdsList.Count;
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, lobbyMaxN, new CreateLobbyOptions{
                IsPrivate = false,
                Player = new Player{
                    Data = new Dictionary<string, PlayerDataObject>{
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, GamePlayer.Instance.Name)}
                    }
                },
                Data = new Dictionary<string, DataObject>{
                    {pre_game_phase, new DataObject(DataObject.VisibilityOptions.Member, PreGamePhase.Lobby.ToString())},
                    {key_relay_host,new DataObject(DataObject.VisibilityOptions.Member, myId) },
                    {key_player_cnt,new DataObject(DataObject.VisibilityOptions.Member, cnt.ToString()) }
                }
            });
            Debug.Log("2 New lobby made");
            RejoinGameServerRpc(lobby.LobbyCode,myId);
        } catch (LobbyServiceException e){
            Debug.Log("Error: ConnectionManager.CreateRejoinLobby(): "+e);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void RejoinGameServerRpc(string code, string newHost){
        RejoinGameClientRpc(code, newHost);
    }

    [ClientRpc(RequireOwnership = false)]
    private void RejoinGameClientRpc(string code, string newHost){
        RejoinGame(code, newHost);
    }

    private async Task RejoinGame(string lobbyCode, string newHost){
        NetworkManager.Singleton.Shutdown();
        Debug.Log("1 left connection");
        if (imInLastOut) return;

        string myId = GamePlayer.Instance.Id;
        if (myId!=newHost) {    // for clients
            reloadMaxTime = 2;
            reloadTimer = 2;
            await JoinLobbyByCode(GamePlayer.Instance.Name, lobbyCode);
            Debug.Log("3 Joined new lobby");
        } else {                // for host
            gameCode = await CreateRelay();
            Debug.Log("2 Created new relay");
            try{
                await UpdateRejoinCodeInLobby(gameCode);
                Debug.Log("2 Updated relay code in lobby");
                if (playersToJoin==1) migrationOnGoing = false;
            }catch(Exception e){
                Debug.Log(e);
            }
            pauseReload = false;
        }
    }

    private async Task UpdateRejoinCodeInLobby(string code){
        try{
            Dictionary<string, DataObject> lobbyData = (await LobbyService.Instance.GetLobbyAsync(lobby.Id)).Data;
            lobbyData[key_game_code] = new DataObject(DataObject.VisibilityOptions.Member, code);
            lobbyData[pre_game_phase] = new DataObject(DataObject.VisibilityOptions.Member, PreGamePhase.Rejoin.ToString());

            lobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, 
                new UpdateLobbyOptions{
                    Data = lobbyData
                }
            );
        }catch (LobbyServiceException e){
            Debug.Log("Error: ConnectionManager.UpdateRejoinCodeInLobby(): "+e);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void ReportJoinedToRelayServerRpc(){
        playersJoined ++;
        Debug.Log(playersJoined+"/"+(playersToJoin-1));
        if(playersJoined==playersToJoin-1) EndMigrationClientRpc();
    }

    [ClientRpc (RequireOwnership = false)]
    private void EndMigrationClientRpc(){
        migrationOnGoing = false;
        Debug.Log("Migration ended");
    } 

    private IEnumerator ReportJoinedToRelay(){
        yield return new WaitForSeconds(5);
        ReportJoinedToRelayServerRpc();
    }
}
