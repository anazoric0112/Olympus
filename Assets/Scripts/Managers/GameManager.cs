using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{ 
    public List<Role> tableCards = new List<Role>();
    public Dictionary<RolesManager.CardName, Role> roleInstances=new Dictionary<RolesManager.CardName, Role>();
    public Dictionary<string, string> playerNames = new Dictionary<string, string>();
    public Dictionary<string, RolesManager.CardName> playerCards = new Dictionary<string, RolesManager.CardName>();
    public List<string> playersOut = new List<string>();
    public List<string> playersIdsList = new List<string>();
    public Dictionary<string, RolesManager.CardName> lastPlayersOut = new Dictionary<string, RolesManager.CardName>(); //ovde se cuva ime-karta, samo da se prikaze posle


    private Dictionary<string, int> votesForElders = new Dictionary<string, int>();
    private Dictionary<string, int> votesForCursed = new Dictionary<string, int>();
    public Dictionary<string, string> whoVotedForWho = new Dictionary<string, string>();
    private Dictionary<string,RolesManager.CardName> fallbackRoles = new Dictionary<string, RolesManager.CardName>();

    private static GameManager instance;
    private int roundNumber=1;
    private int moveIndex=0;
    private int playersWentNext=0;
    private int playersToWait=0;
    private Role onlyElder = null;

    //move indexes for voting round
    public int singleElderPreGameMI = 1<<1;
    public int lastRoundMove = 1<<10;
    public int endChancesMI = 1<<10 | 1<<14 | 1<<17;
    public int discussionMI = 1<<11;
    public int forCursedVoteMI = 1<<12;
    public int forCursedVoteResultMI = 1<<13;
    public int forEldersVoteMI = 1<<15;
    public int forEldersVoteResultMI = 1<<16;
    public int elderShowMI = 1<<18;
    private const int lastVotingMove = 1<<19;

    private string hasDoubleVote="";
    private string protectedPlayer="";
    private string dedicatedNextElderId = "";
    private List<string> lastVotedOutList = new List<string>();
    private string lastVotedOut = "";
    private string lastVotedOutName = "";

    public int TableCardsCnt{
        get{return tableCards.Count;}
    }
    public int MoveIndex{
        get{return moveIndex;}
    }    
    public int RoundNumber{
        get{return roundNumber;}
    }
    public bool IsCursedVote {
        get { return (moveIndex & forCursedVoteMI)!=0;}
    }
    public bool IsEldersVote {
        get { return (moveIndex & forEldersVoteMI)!=0;}
    }
    public RolesManager.Team WinnerTeam{
        get {
            if (TeamWon(RolesManager.Team.Olympus)) return RolesManager.Team.Olympus;
            if (TeamWon(RolesManager.Team.Tartarus)) return RolesManager.Team.Tartarus;
            return RolesManager.Team.None;
        }
    }
    public List<string> LastVotedOutList{
        get {return lastVotedOutList;}
    }
    public string LastVotedOutName{
        get{return lastVotedOutName;}
    }
    static public GameManager Instance {
        get {return instance;}
    }

    void Awake(){

        if (instance){
            gameObject.SetActive(false);
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start(){
        if (roleInstances.Count==0) InstantiateRoleBehaviours();
    }
    
    //------------------------------------------------------------
    //Called when starting the game
    //------------------------------------------------------------

    public void FillPlayers(List<Player> players){
        foreach(Player player in players){
            playerNames[player.Id]=player.Data["PlayerName"].Value;
            playersIdsList.Add(player.Id);
            
            if (player.Id==AuthenticationService.Instance.PlayerId){
                GamePlayer.Instance.SetPlayerObject(player);
            }
        }
        playersToWait=players.Count;
    }

    public void FillPlayerRoles(string dataString){
        string[] data = dataString.Split("#");

        foreach (string pair in data){
            string[] playerAndRole = pair.Split(":");
            string playerId = playerAndRole[0];
            string roleName = playerAndRole[1];

            RolesManager.CardName card =(RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName),roleName);
            playerCards[playerId] = card;
            
            if (playerId==AuthenticationService.Instance.PlayerId){
                GamePlayer.Instance.Role=roleInstances[card];
                GamePlayer.Instance.NextRole=roleInstances[card];
            }
        }
    }

    public void SetTableCards(string dataString){
        AssignOnlyElderMove();
        if (dataString=="") return;

        string[] data = dataString.Split("#");

        foreach(string name in data){
            tableCards.Add(roleInstances[(RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName),name)]);
        }
        PrintGameState();
    }

    //------------------------------------------------------------
    //Called after NetworkVariableChange or from RPC
    //------------------------------------------------------------

    public void PlayersSwap(string p1, string p2){
        // Debug.Log("Players swap called with "+p1+" and "+p2); //###
        RolesManager.CardName c1=playerCards[p1];
        RolesManager.CardName c2=playerCards[p2];
        playerCards[p1] = c2;
        playerCards[p2] = c1;

        if (p1==AuthenticationService.Instance.PlayerId){
            GamePlayer.Instance.NextRole=roleInstances[c2];
        }
        if (p2==AuthenticationService.Instance.PlayerId){
            GamePlayer.Instance.NextRole=roleInstances[c1];
        }
        PrintGameState();
    }

    public void PlayersSwapWithFallback(string p1, string p2){
        // Debug.Log("Players swap with fallback called with "+p1+" and "+p2); //###
        RolesManager.CardName c1=playerCards[p1];
        RolesManager.CardName c2=playerCards[p2];
        fallbackRoles[p1]=c2;
        fallbackRoles[p2]=c1;

        if (p1==AuthenticationService.Instance.PlayerId){
            GamePlayer.Instance.FallbackRole=roleInstances[c2];
        }
        if (p2==AuthenticationService.Instance.PlayerId){
            GamePlayer.Instance.FallbackRole=roleInstances[c1];
        }
        PrintGameState();
    }
    
    public void TablePlayersSwap(string p1, string p2, string c){
        // Debug.Log("Table swap called with "+p1+" " +p1+ " and "+c); //###
        RolesManager.CardName c1=playerCards[p1];
        RolesManager.CardName c2=playerCards[p2];
        RolesManager.CardName c3=(RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName),c);

        playerCards[p1]=c2;
        playerCards[p2]=c3;
        
        if (p1==AuthenticationService.Instance.PlayerId){
            GamePlayer.Instance.NextRole=roleInstances[c2]; 
        }
        if (p2==AuthenticationService.Instance.PlayerId){
            GamePlayer.Instance.NextRole=roleInstances[c3]; 
        }

        tableCards.Remove(roleInstances[c3]);
        tableCards.Add(roleInstances[c1]);

        Debug.Log("Initial state: ");
        PrintGameState();
    }

    public void TakeFromTable(string c, bool totable){
        // Debug.Log("Take from table called with "+c+", totable: "+totable); //###
        RolesManager.CardName card= (RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName),c);
        if (totable) tableCards.Add(roleInstances[card]);
        else tableCards.Remove(roleInstances[card]);
    }

    public void GoNextForPlayer(string id){
        playersWentNext++;
        Debug.Log("Go next for player "+id+": "+playersWentNext+" / "+playersToWait);

        if (playersWentNext==playersToWait) {
            NextMove();
            Debug.Log("Loaded move "+Math.Log10(moveIndex*2)/Math.Log10(2));
        }
    }

    public void VoteForPlayer(string playerName, string oneVoting, bool voteForElders){
        // Debug.Log("Vote for player "+playerName+" by "+oneVoting+", elders: "+voteForElders);
        int inc = (oneVoting==hasDoubleVote && !voteForElders) ? 2:1;

        string playerId = FindPlayerByName(playerName);

        if (voteForElders) {
            if (votesForElders.ContainsKey(playerId)) votesForElders[playerId]+=inc;
            else votesForElders[playerId]=inc;
        }
        else {
            if (votesForCursed.ContainsKey(playerId)) votesForCursed[playerId]+=inc;
            else votesForCursed[playerId]=inc;
        }
    }

    public void Protect(string player){
        // Debug.Log("Protect called for "+player);
        protectedPlayer=player;
    }
    
    public void PhoenixCardChange(string player, string card){
        // Debug.Log("Phoenix card change called");
        
        RolesManager.CardName cardName = (RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName), card);
        Role newrole = roleInstances[cardName];

        TakeFromTable(card,false);

        playerCards[player]=newrole.GetCardName();
        fallbackRoles[player]=RolesManager.CardName.Phoenix;

        // Debug.Log("Phoenix got his card");
    }
    
    public void SwapVotingMoves(){
        // Debug.Log("Voting rounds swap called");
        foreach(Role r in roleInstances.Values){
            r.Behaviour.RemoveMove(forCursedVoteMI);
            r.Behaviour.RemoveMove(forEldersVoteMI);
            r.Behaviour.RemoveMove(forEldersVoteResultMI);
            r.Behaviour.RemoveMove(forCursedVoteResultMI);
        }
        int t = forCursedVoteMI;
        forCursedVoteMI=forEldersVoteMI;
        forEldersVoteMI=t;

        t=forCursedVoteResultMI;
        forCursedVoteResultMI=forEldersVoteResultMI;
        forEldersVoteResultMI=t;

        foreach(Role r in roleInstances.Values){
            if (r.Behaviour.Team==RolesManager.Team.Tartarus 
                && r.GetCardName()!=RolesManager.CardName.Charon) {
                r.Behaviour.AddMove(forEldersVoteMI);
            }
            r.Behaviour.AddMove(forCursedVoteMI);
            r.Behaviour.AddMove(forEldersVoteResultMI);
            r.Behaviour.AddMove(forCursedVoteResultMI);
        }
        PrintMoveIndexes();
    }

    //------------------------------------------------------------
    //Helper public methods
    //------------------------------------------------------------

    public string GetNextElderShow(){
        return dedicatedNextElderId;
    }

    public string GetRandomPlayerIdFromTeam(RolesManager.Team  team){
        // Debug.Log("Get random player called for team "+team.ToString());
        
        List<RolesManager.CardName> targetTeam = new List<RolesManager.CardName>();
        foreach(RolesManager.CardName card in playerCards.Values){
            if (roleInstances[card].Behaviour.Team==team) targetTeam.Add(card);
        }
        if (targetTeam.Count==0) return "";

        int rng = UnityEngine.Random.Range(0,targetTeam.Count);
        return FindPlayerByCard(targetTeam[rng]); 
    }

    public string GetRandomTableCard(){
        // Debug.Log("Get random table card called");
        if (tableCards.Count==0) return "";
        int rng = UnityEngine.Random.Range(0,tableCards.Count);
        return tableCards[rng].GetName();
    }

    public string FindPlayerByCard(RolesManager.CardName card){

        foreach(string player in playerCards.Keys){
            if (playerCards[player]==card) return player;
        }
        return "";
    }

    public string FindPlayerByName(string name){
        foreach(string player in playerNames.Keys){
            if (playerNames[player]==name) return player;
        }
        return "";
    }

    private void NextMove(){
        Debug.Log("Next move called");
        PrintMoveIndexes();
        playersWentNext=0;
        int prevmove = moveIndex;
        MoveIndexForward();

        if ((moveIndex & endChancesMI) !=0){
            if (IsOver()){
                SceneManager.LoadScene((int)DisplayManager.Scenes.EndGame);
                return;
            } else {
                MoveIndexForward();
            }
        }

        if (prevmove<=forEldersVoteMI && moveIndex>forEldersVoteMI) hasDoubleVote="";

        // ovaj if sam dodala zbog charon-a ### i menjala PlayersHAveMove(sad ima mi argument)
        if ((moveIndex&forEldersVoteResultMI)!=0 
        && PlayersHaveMove(forEldersVoteMI)==0){
            MoveIndexForward();
        }

        if ((moveIndex & lastVotingMove)!=0){
            NextRound();
        } else {
            GamePlayer.Instance.Role.Behaviour.ToNextScene(moveIndex);
            Debug.Log("State after player went to next move: ");
            PrintGameState();
        }
    }

    //to be tested VERY MUCH
    public void ProcessVotes(bool voteForElders){
        Debug.Log("Process votes called");
        lastPlayersOut.Clear();
        CountVotes(voteForElders);

        if (lastVotedOut=="") return;
        
        //treba nesto da se pozove prvo efekat ako je izglasan
        RolesManager.Team votingForTeam = voteForElders? RolesManager.Team.Olympus : RolesManager.Team.Tartarus;
        roleInstances[playerCards[lastVotedOut]].Behaviour.DoVotedForPassive(votingForTeam);
        
        //onda treba da se odradi death effect
        roleInstances[playerCards[lastVotedOut]].Behaviour.DoDeathEffect(votingForTeam);
        if (lastVotedOut==protectedPlayer) {
            roleInstances[playerCards[lastVotedOut]].Behaviour.ProtectFromDying();
        }
        
        lastVotedOutName=playerNames[lastVotedOut];
        if (voteForElders && roleInstances[playerCards[lastVotedOut]].Behaviour.CardClass!=RolesManager.CardClass.Elder){
            hasDoubleVote = lastVotedOut;
        }
        if (roleInstances[playerCards[lastVotedOut]].Behaviour.IsDead){
            PlayerOut(lastVotedOut);
            if (voteForElders) DedicateShowingElder();
        }
        playersToWait-=lastPlayersOut.Count; //ovo sam dodala nakon sto je radilo ###
    }

    private void CountVotes(bool voteForElders){
        lastVotedOutList.Clear();
        int maxvotes=0;
        Dictionary<string,int> votes = voteForElders? votesForElders : votesForCursed;

        foreach(KeyValuePair<string,int> kp in votes){
            if (kp.Value>maxvotes) {
                lastVotedOutList.Clear();
                lastVotedOutList.Add(kp.Key);
                maxvotes=kp.Value;
            } else if (kp.Value==maxvotes){
                lastVotedOutList.Add(kp.Key);
            }
        }

        if (lastVotedOutList.Count==1) lastVotedOut=lastVotedOutList[0];
        else lastVotedOut="";
    }

    public void PlayerOut(string player){
        Debug.Log("Player out called for "+player);
        lastPlayersOut[playerNames[player]]=playerCards[player];

        playersOut.Add(player);
        playerNames.Remove(player);
        playerCards.Remove(player);
        playersIdsList.Remove(player);
    }
    
    public void ChangeTeam(RolesManager.CardName card){
        Debug.Log("Change team called for "+card.ToString());
        RolesManager.Team current = roleInstances[card].Behaviour.Team;
        RolesManager.Team next = current==RolesManager.Team.Olympus ? RolesManager.Team.Tartarus : RolesManager.Team.Olympus;
        roleInstances[card].Behaviour.Team=next;

        if(next==RolesManager.Team.Tartarus){
            roleInstances[card].Behaviour.AddMove(forEldersVoteMI);
        } else {
            roleInstances[card].Behaviour.RemoveMove(forEldersVoteMI);
        }
    }

    public void SetOnlyVoteCounts(string player){
        Debug.Log("Set only vote counds called for "+playerNames[player]);
        lastVotedOut=whoVotedForWho[player];
        lastVotedOutName=playerNames[lastVotedOut];
    }


    //------------------------------------------------------------
    //Helper private methods
    //------------------------------------------------------------

    private void MoveIndexForward(){
        // Debug.Log("Index move forward called");
        if (moveIndex==0) moveIndex=1;
        else moveIndex<<=1;

        while (PlayersHaveMove()==0 && moveIndex<lastVotingMove) moveIndex<<=1;
        playersToWait=PlayersHaveMove();
        Debug.Log("Players have move: "+playersToWait);
    }

    private bool IsOver(){
        // Debug.Log("IsOver called");
        return TeamWon(RolesManager.Team.Olympus)
                || TeamWon(RolesManager.Team.Tartarus);
    }

    private bool TeamWon(RolesManager.Team winningTeam){
        foreach(RolesManager.CardName card in playerCards.Values){
            RolesManager.Team team = roleInstances[card].Behaviour.Team;
            RolesManager.CardClass cls = roleInstances[card].Behaviour.CardClass;
            
            if (winningTeam==RolesManager.Team.Tartarus && cls==RolesManager.CardClass.Elder) return false;

            if (winningTeam==RolesManager.Team.Olympus && team==RolesManager.Team.Tartarus) return false;
        }

        return true;
    }

    private int PlayersHaveMove(int mi=-1){
        // Debug.Log("Players have move called");
        if (mi==-1) mi=moveIndex;
        int cnt=0;
        foreach(string player in playerCards.Keys){

            RolesManager.CardName card = playerCards[player];
            Role role = roleInstances[card];
            if (role.Behaviour.HasMove(mi)) cnt++;
        }
        return cnt;
    }

    private void DedicateShowingElder(){
        Debug.Log("Dedicate showing elder called");
        List<string> elders = new List<string>();

        foreach(KeyValuePair<string, RolesManager.CardName> kp in playerCards){
            if (roleInstances[kp.Value].Behaviour.CardClass==RolesManager.CardClass.Elder){
                elders.Add(kp.Key);
            }
            if (roleInstances[kp.Value].Behaviour.Team==RolesManager.Team.Tartarus){
                roleInstances[kp.Value].Behaviour.AddMove(elderShowMI);
            }
        }
        if (elders.Count==0) return;
        int playersCnt = playersIdsList.Count;
        int rng = (playersCnt*11 + 200%playersCnt) % elders.Count;
        dedicatedNextElderId = elders[rng];

    }
    
    private void NextRound(){
        Debug.Log("Next round called");
        RoundStatsReset();
        FallBackRolesFix();

        foreach(Role r in roleInstances.Values){
            r.Behaviour.DoRoundEndPassive();
            r.Behaviour.RemoveMove(elderShowMI);
        }

        Debug.Log("Went to round "+ roundNumber +", playerstowait "+playersToWait);
        DisplayManager.ToGameStart();
    }

    private void RoundStatsReset(){
        votesForCursed.Clear();
        votesForElders.Clear();
        whoVotedForWho.Clear();
        lastPlayersOut.Clear();

        roundNumber++;
        moveIndex=0;
        playersWentNext=0;
        playersToWait=playersIdsList.Count;

        protectedPlayer="";

        if (onlyElder!=null) onlyElder.Behaviour.RemoveMove(singleElderPreGameMI);
    }

    private void FallBackRolesFix(){
        GamePlayer p = GamePlayer.Instance;

        if (p.FallbackRole!=null) {
            //phoenix edgecase
            p.Role = p.FallbackRole;
            RPCsManager.Instance.TakeFromTableServerRpc(p.NextRole.GetName(),true); 
            p.NextRole=p.FallbackRole;
            playerCards[p.Id]=p.Role.GetCardName();
        }
        else p.Role=p.NextRole;

        foreach(KeyValuePair<string,RolesManager.CardName> kp in fallbackRoles){
            playerCards[kp.Key]=kp.Value;
        }
        fallbackRoles.Clear();
    }

    private void InstantiateRoleBehaviours(){
        RolesManager rolesManager = FindObjectOfType<RolesManager>();
        foreach (Role r in rolesManager.GetAllRoles()){
            roleInstances[r.GetCardName()]=r;
        }
        roleInstances[RolesManager.CardName.Athena].SetRoleBehaviour(new AthenaBehaviour());
        roleInstances[RolesManager.CardName.Zeus].SetRoleBehaviour(new ZeusBehaviour());
        roleInstances[RolesManager.CardName.Aphrodite].SetRoleBehaviour(new AphroditeBehaviour());
        roleInstances[RolesManager.CardName.Dionysis].SetRoleBehaviour(new DionysisBehaviour());
        roleInstances[RolesManager.CardName.Cassandra].SetRoleBehaviour(new CassandraBehaviour());
        roleInstances[RolesManager.CardName.Dracaena].SetRoleBehaviour(new DracaenaBehaviour());
        roleInstances[RolesManager.CardName.Dryad].SetRoleBehaviour(new DryadBehaviour());
        roleInstances[RolesManager.CardName.Pandora].SetRoleBehaviour(new PandoraBehaviour());
        roleInstances[RolesManager.CardName.Pegasus].SetRoleBehaviour(new PegasusBehaviour());
        roleInstances[RolesManager.CardName.Phoenix].SetRoleBehaviour(new PhoenixBehaviour());
        roleInstances[RolesManager.CardName.Apollo].SetRoleBehaviour(new ApolloBehaviour());
        roleInstances[RolesManager.CardName.Artemis].SetRoleBehaviour(new ArtemisBehaviour());
        roleInstances[RolesManager.CardName.Perseus].SetRoleBehaviour(new PerseusBehaviour());
        roleInstances[RolesManager.CardName.Basilisk].SetRoleBehaviour(new BasiliskBehaviour());
        roleInstances[RolesManager.CardName.Achilles].SetRoleBehaviour(new AchillesBehaviour());
        roleInstances[RolesManager.CardName.Nyx].SetRoleBehaviour(new NyxBehaviour());
        roleInstances[RolesManager.CardName.Hemera].SetRoleBehaviour(new HemeraBehaviour());
        roleInstances[RolesManager.CardName.Sisyphus].SetRoleBehaviour(new SisyphusBehaviour());
        roleInstances[RolesManager.CardName.Orpheus].SetRoleBehaviour(new OrpheusBehaviour());
        roleInstances[RolesManager.CardName.Medusa].SetRoleBehaviour(new MedusaBehaviour());
        roleInstances[RolesManager.CardName.Siren].SetRoleBehaviour(new SirenBehaviour());
        roleInstances[RolesManager.CardName.Hydra].SetRoleBehaviour(new HydraBehaviour());
        roleInstances[RolesManager.CardName.Charon].SetRoleBehaviour(new CharonBehaviour());
        roleInstances[RolesManager.CardName.Hades].SetRoleBehaviour(new HadesBehaviour());

        foreach(Role r in roleInstances.Values){
            if (r.Behaviour.Team==RolesManager.Team.Tartarus 
                && r.GetCardName()!=RolesManager.CardName.Charon) {
                r.Behaviour.AddMove(forEldersVoteMI);
            }
            r.Behaviour.AddMove(forCursedVoteMI);
            r.Behaviour.AddMove(forEldersVoteResultMI);
            r.Behaviour.AddMove(forCursedVoteResultMI);
            r.Behaviour.AddMove(endChancesMI);
            r.Behaviour.AddMove(discussionMI);
        }
    }

    public void ResetGame(){
        tableCards.Clear();
        playerNames.Clear();
        playerCards.Clear();
        playersOut.Clear();
        roleInstances.Clear();
        playersIdsList.Clear();
        votesForCursed.Clear();
        votesForElders.Clear();
        lastPlayersOut.Clear();
        whoVotedForWho.Clear();
        fallbackRoles.Clear();
        lastVotedOutList.Clear();

        roundNumber=1;
        moveIndex=0;
        playersWentNext=0;
        playersToWait=0;
        protectedPlayer="";
        hasDoubleVote="";
        dedicatedNextElderId="";
        lastVotedOut="";

        onlyElder=null;

        InstantiateRoleBehaviours();
        RestoreVotingMoves();
    }

    private void AssignOnlyElderMove(){
        RolesManager.CardName elder = RolesManager.CardName.None;

        foreach(RolesManager.CardName c in playerCards.Values){
            if (roleInstances[c].Behaviour.CardClass!=RolesManager.CardClass.Elder) continue;

            if (elder != RolesManager.CardName.None) return;
            elder = c;
        }
        if (elder == RolesManager.CardName.None) return;
        roleInstances[elder].Behaviour.AddMove(singleElderPreGameMI);
        onlyElder = roleInstances[elder];
        Debug.Log("Only elder move assigned");
    }

    private void RestoreVotingMoves(){
        forCursedVoteMI = 1<<12;
        forCursedVoteResultMI = 1<<13;
        forEldersVoteMI = 1<<15;
        forEldersVoteResultMI = 1<<16;
    }

    private void PrintMoveIndexes(){
        string debugText = "";
        foreach(KeyValuePair<string, RolesManager.CardName> kp in playerCards){
            string s=playerNames[kp.Key]+": ";
            int mi = roleInstances[kp.Value].Behaviour.MoveIndexes;

            while (mi>0){
                s+=(mi%2).ToString();
                mi>>=1;
            }
            debugText+=s+"\n";
        }
        Debug.Log(debugText);
    }

    private void PrintGameState(){
        string printstr="Players and their cards:";
        foreach(string p in playerCards.Keys){
            printstr+="\n"+playerNames[p]+" "+playerCards[p];
        }
        string table="";
        foreach(Role r in tableCards){
            table+=r.GetName()+" ";
        }

        printstr+="\nCards on the table: "+table;
        printstr+="\nMove: "+Math.Log10(moveIndex*2)/Math.Log10(2)+
                    ", Round: "+roundNumber+
                    ", WentNext/ToWait: "+playersWentNext+"/"+playersToWait;
        printstr+="\n"+GamePlayer.Instance.GamePlayerString();
        Debug.Log(printstr);
    }
}
