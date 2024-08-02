using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Role", menuName = "New Role SO")]
public class Role : ScriptableObject
{
    //-------Fields-------
    [TextArea(2,10)]
    [SerializeField] string description;
    [SerializeField] string roleName;
    [SerializeField] Sprite image;
    RoleBehaviour behaviour;

    //-------Properties-------
    public RoleBehaviour Behaviour {
        get { return behaviour; }
        set { behaviour=value; }
    }

    public Sprite Image{
        get {return image;}
    }

    public string Description{
        get {return description;}
    }

    public string Name{
        get {return roleName;}
    }

    public RolesManager.CardName CardName{
        get {return (RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName),roleName);}
    }

    //------------------------------------------------------------
    //For C# Collection purposess
    //------------------------------------------------------------
    
    public override bool Equals(object other)
    {
        if (other==null) return false;

        var item = other as Role;
        return item.Name==Name;
    }

    public override int GetHashCode()
    {
        return roleName.GetHashCode();
    }

}
