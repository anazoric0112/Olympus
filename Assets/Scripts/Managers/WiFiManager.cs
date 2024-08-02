using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WiFiManager : MonoBehaviour
{
    //-------Fields-------
    [SerializeField] GameObject wifiErrorModalPrefab;
    List<Selectable> interactables = new List<Selectable>();
    List<Selectable> toEnable = new List<Selectable>();
    private bool wasConnected = true;
    private bool nowConnected = true;
    private bool rejoining = false;
    private GameObject modal = null;

    //------------------------------------------------------------
    //Code for detecting connection
    //------------------------------------------------------------

    async void Update()
    {
        if (rejoining) return;

        nowConnected=Application.internetReachability != NetworkReachability.NotReachable;
        
        if(!nowConnected && wasConnected){
            DisableEnabled();
            modal = InstantiateToCanvas(wifiErrorModalPrefab);

            // LATER RELEASE
            //### supposed to disable all game functions if this one disconnects
            //### additional action required if it is a host

        } else if (nowConnected && !wasConnected) {
            rejoining = true;
            
            // LATER RELEASE
            //### enable game functions back
            //### additional action required if it is a host
            EnableBack();
            Destroy(modal);
            modal = null;

            rejoining = false;
        }
        wasConnected = nowConnected;
    }

    static public bool IsConnected(){
        // return Application.internetReachability != NetworkReachability.NotReachable;
        return FindObjectOfType<WiFiManager>().wasConnected;
    }

    //------------------------------------------------------------
    //Showing error on screen
    //------------------------------------------------------------

    private GameObject InstantiateToCanvas(GameObject prefab){
        GameObject o = Instantiate(prefab);
        Canvas parent = FindAnyObjectByType<Canvas>();
        o.transform.parent = parent.transform;
        o.transform.localScale = new Vector3(1,1,1);
        o.GetComponent<RectTransform>().anchoredPosition=new Vector2(0, 0);
        return o;
    }

    //------------------------------------------------------------
    //Methods for manipulating interactable graphics
    //------------------------------------------------------------

    public void AddToInteractables(Selectable s){
        interactables.Add(s);
    }

    public void ClearInteractables(){
        interactables.Clear();
    }

    private void DisableEnabled(){
        foreach(Selectable s in interactables){
            if(!s.interactable) continue;

            toEnable.Add(s);
            s.interactable = false;
        }
    }

    private void EnableBack(){
        foreach(Selectable s in toEnable){
            s.interactable = true;
        }
        toEnable.Clear();
    }

}
