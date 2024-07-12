using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Role", menuName = "New Role SO")]
public class Role : ScriptableObject
{
    [TextArea(2,10)]
    [SerializeField] string description;
    [SerializeField] string roleName;
    [SerializeField] RoleBehaviour behaviour;
    [SerializeField] Sprite image;

    public RoleBehaviour Behaviour {
        get { return behaviour; }
        set { behaviour=value; }
    }

    public Sprite GetImage(){
        return image;
    }

    public string GetDescription(){
        return description;
    }

    public string GetName(){
        return roleName;
    }
    public RolesManager.CardName GetCardName(){
        return (RolesManager.CardName)Enum.Parse(typeof(RolesManager.CardName),roleName);
    }

    public override bool Equals(object other)
    {
        if (other==null) return false;

        var item = other as Role;
        return item.GetName()==GetName();
    }

    public override int GetHashCode()
    {
        return roleName.GetHashCode();
    }

    public void SetRoleBehaviour(RoleBehaviour b){
        behaviour=b;
    }
}
