using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RolesManager : MonoBehaviour
{
    //-------Collection of Role Scriptable Objects-------
    [SerializeField] List<Role> roles = new List<Role>();
    [SerializeField] List<Role> elders = new List<Role>();
    [SerializeField] List<Role> plotters = new List<Role>();
    [SerializeField] List<Role> vindictives = new List<Role>();
    [SerializeField] List<Role> deceivers = new List<Role>();
    [SerializeField] List<Role> cursed = new List<Role>();

    //-------Getter properties-------
    public List<Role> AllRoles{
        get {return roles;}
    }
    public List<Role> Elders{
        get {return elders;}
    }
    public List<Role> Plotters{
        get {return plotters;}
    }
    public List<Role> Vindictives{
        get {return vindictives;}
    }
    public List<Role> Deceivers{
        get {return deceivers;}
    }
    public List<Role> Cursed{
        get {return cursed;}
    }

    //-------Enums for role properties-------
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

}
