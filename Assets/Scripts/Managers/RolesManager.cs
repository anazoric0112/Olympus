using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RolesManager : MonoBehaviour
{
    [SerializeField] List<Role> roles = new List<Role>();
    [SerializeField] List<Role> elders = new List<Role>();
    [SerializeField] List<Role> plotters = new List<Role>();
    [SerializeField] List<Role> vindictives = new List<Role>();
    [SerializeField] List<Role> deceivers = new List<Role>();
    [SerializeField] List<Role> cursed = new List<Role>();

    public enum CardClass{
        None,
        Elder,
        Vindictive,
        Plotter,
        Deceiver,
        Cursed
    }

    public enum Team{
        None,
        Olympus,
        Tartarus
    }

    public enum CardName{
        None,
        Zeus,
        Athena,
        Dionysis,
        Aphrodite,
        Artemis,
        Apollo,
        Achilles,
        Perseus,
        Basilisk,
        Cassandra,
        Dryad,
        Dracaena,
        Pandora,
        Pegasus,
        Phoenix,
        Nyx,
        Hemera,
        Sisyphus,
        Orpheus,
        Charon,
        Hades,
        Medusa,
        Siren,
        Hydra
    }

    void Awake()
    {
        foreach(Role r in elders) roles.Add(r);
        foreach(Role r in plotters) roles.Add(r);
        foreach(Role r in vindictives) roles.Add(r);
        foreach(Role r in deceivers) roles.Add(r);
        foreach(Role r in cursed) roles.Add(r);
    }

    void Update()
    {
        
    }

    public Role GetRoleByName(string name){
        foreach(Role r in roles){
            if (r.GetName()==name) return r;
        }
        return null;
    }

    public List<Role> GetAllRoles(){
        return roles;
    }

    public List<Role> GetElders(){
        return elders;
    }
    public List<Role> GetPlotters(){
        return plotters;
    }
    public List<Role> GetVindictives(){
        return vindictives;
    }
    public List<Role> GetDeceivers(){
        return deceivers;
    }
    public List<Role> GetCursed(){
        return cursed;
    }
}
