using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GamePlayer : NetworkBehaviour
{
    //-------Fields-------
    private Role role;
    private Role nextRole;
    private Role fallbackRole=null;
    private Player playerObject;

    //-------Properties-------
    public Role Role{
        get {return role;}
        set {role=value;}
    }

    public Role NextRole{
        get {return nextRole;}
        set {nextRole=value;}
    }

    public Role FallbackRole{
        get{return fallbackRole;}
        set {fallbackRole=value;}
    }
    
    public string Name{
        get {
            if (playerObject==null) return "";
            return playerObject.Data["PlayerName"].Value;
        }
    }

    public string Id{
        get {
            if (playerObject==null) return "";
            return playerObject.Id;
        }
    }
 
    //-------For singleton-------
    static GamePlayer instance;

    static public GamePlayer Instance {
        get { return instance; }
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

    //------------------------------------------------------------
    //Helper methods
    //------------------------------------------------------------

    public void SetPlayerObject(Player po){
        playerObject = po;
    }

    public string GamePlayerString(){
        return "Role: "+Role.Name+"; \nNext role: "+NextRole.Name+"; \nName: "+Name+"; \nId: "+Id;
    }

}
