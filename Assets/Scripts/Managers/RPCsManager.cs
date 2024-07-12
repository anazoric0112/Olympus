using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RPCsManager : NetworkBehaviour
{      
    static RPCsManager instance;
    static public RPCsManager Instance {
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
    
    [ServerRpc(RequireOwnership = false)]
    public void SwapPlayersServerRpc(string p1, string p2){
        SwapPlayersClientRpc(p1,p2);
    }
        
    [ServerRpc(RequireOwnership = false)]
    public void SwapPlayersFallbackServerRpc(string p1, string p2){
        SwapPlayersFallbackClientRpc(p1,p2);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TableSwapPlayersServerRpc(string p1, string p2, string c){
        TableSwapPlayersClientRpc(p1,p2,c);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeFromTableServerRpc(string c, bool totable){
        TakeFromTableClientRpc(c,totable);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoNextServerRpc(string id){
        GoNextClientRpc(id);
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteServerRpc(string vName, string oneVoting, bool forElders){
        VoteClientRpc(vName,oneVoting,forElders);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ProtectServerRpc(string p){
        ProtectClientRpc(p);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PhoenixRoleChangeServerRpc(string p, string c){
        PhoenixRoleChangeClientRpc(p,c);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SwapVotingMovesServerRpc(){
        SwapVotingMovesClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void SwapPlayersClientRpc(string p1, string p2){
        GameManager.Instance.PlayersSwap(p1,p2);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SwapPlayersFallbackClientRpc(string p1, string p2){
        GameManager.Instance.PlayersSwapWithFallback(p1,p2);
    }


    [ClientRpc(RequireOwnership = false)]
    private void TableSwapPlayersClientRpc(string p1, string p2, string c){
        GameManager.Instance.TablePlayersSwap(p1,p2,c);
    }

    [ClientRpc(RequireOwnership = false)]
    private void TakeFromTableClientRpc(string c, bool totable){
        GameManager.Instance.TakeFromTable(c, totable);
    }

    [ClientRpc(RequireOwnership = false)]
    private void GoNextClientRpc(string id){
        Debug.Log("GoNextClientRpc called");
        GameManager.Instance.GoNextForPlayer(id);
    }

    [ClientRpc(RequireOwnership = false)]
    private void VoteClientRpc(string vName, string oneVoting, bool elders){
        GameManager.Instance.VoteForPlayer(vName, oneVoting, elders);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ProtectClientRpc(string p){
        GameManager.Instance.Protect(p);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PhoenixRoleChangeClientRpc(string p, string c){
        GameManager.Instance.PhoenixCardChange(p,c);
    }
    [ClientRpc(RequireOwnership = false)]
    private void SwapVotingMovesClientRpc(){
        GameManager.Instance.SwapVotingMoves();
    }

}
