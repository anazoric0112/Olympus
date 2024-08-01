using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WiFiManager : MonoBehaviour
{
    [SerializeField] GameObject wifiErrorModalPrefab;
    List<Selectable> interactables = new List<Selectable>();
    List<Selectable> toEnable = new List<Selectable>();
    private bool wasConnected = true;
    private bool nowConnected = true;
    private bool rejoining = false;
    private GameObject modal = null;

    void Start()
    {
        
    }

    async void Update()
    {
        if (rejoining) return;

        nowConnected=Application.internetReachability != NetworkReachability.NotReachable;
        
        if(!nowConnected && wasConnected){
            DisableEnabled();
            modal = InstantiateToCanvas(wifiErrorModalPrefab);

            //ako je host mora da se migrira host

        } else if (nowConnected && !wasConnected) {
            rejoining = true;
            
            await FindObjectOfType<ConnectionManager>().RejoinRelay();
            Thread.Sleep(2000);
            EnableBack();
            Destroy(modal);
            modal = null;

            rejoining = false;
        }
        wasConnected = nowConnected;
    }

    public void AddToInteractables(Selectable s){
        interactables.Add(s);
    }

    public void ClearInteractables(){
        interactables.Clear();
    }

    static public bool IsConnected(){
        // return Application.internetReachability != NetworkReachability.NotReachable;
        return FindObjectOfType<WiFiManager>().wasConnected;
    }

    private void DisableEnabled(){
        foreach(Selectable s in interactables){
            if(!s.interactable) continue;

            toEnable.Add(s);
            s.interactable = false;
        }
    }

    private GameObject InstantiateToCanvas(GameObject prefab){
        GameObject o = Instantiate(prefab);
        Canvas parent = FindAnyObjectByType<Canvas>();
        o.transform.parent = parent.transform;
        o.transform.localScale = new Vector3(1,1,1);
        o.GetComponent<RectTransform>().anchoredPosition=new Vector2(0, 0);
        return o;
    }

    private void EnableBack(){
        foreach(Selectable s in toEnable){
            s.interactable = true;
        }
    }
}
