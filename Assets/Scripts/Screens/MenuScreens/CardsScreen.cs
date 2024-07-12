using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsScreen : MonoBehaviour
{    
    [SerializeField] Button backButton;
    [SerializeField] GameObject scrollableCards;
    [SerializeField] GameObject roleCardPrefab;
    [SerializeField] GameObject cardGroupPrefab;
    [SerializeField] GameObject cardGroupTitlePrefab;

    [SerializeField] GameObject descModal;
    [SerializeField] TMP_Text descText;
    [SerializeField] TMP_Text descTitle;
    [SerializeField] Button descCloseButton;
    
    private ConnectionManager connectionManager;
    private RolesManager rolesManager;

    void Awake(){

        connectionManager = FindObjectOfType<ConnectionManager>();
        rolesManager = FindFirstObjectByType<RolesManager>();

        backButton.onClick.AddListener(()=>{
            DisplayManager.BackToStart();
        });
        descCloseButton.onClick.AddListener(()=>{
            descModal.SetActive(false);
        });
    }

    void Start(){
        AddCardGroup(rolesManager.GetElders(), RolesManager.CardClass.Elder);
        AddCardGroup(rolesManager.GetPlotters(), RolesManager.CardClass.Plotter);
        AddCardGroup(rolesManager.GetVindictives(), RolesManager.CardClass.Vindictive);
        AddCardGroup(rolesManager.GetDeceivers(), RolesManager.CardClass.Deceiver);
        AddCardGroup(rolesManager.GetCursed(), RolesManager.CardClass.Cursed);
    }

    private void AddCardGroup(List<Role> roles, RolesManager.CardClass groupName){
        GameObject title = DisplayManager.InstantiateWithParent(cardGroupTitlePrefab, scrollableCards);
        GameObject group = DisplayManager.InstantiateWithParent(cardGroupPrefab, scrollableCards);
        title.GetComponentInChildren<TMP_Text>().text = groupName.ToString();

        int rowNumber = roles.Count/2 + roles.Count%2;
        group.GetComponent< RectTransform >( ).SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, 480*rowNumber);

        foreach (Role role in roles){
            GameObject roleCard = DisplayManager.InstantiateWithParent(roleCardPrefab, group);
            roleCard.GetComponentInChildren<Image>().sprite = role.GetImage();

            Button infoButton = roleCard.GetComponentInChildren<Image>().GetComponentInChildren<Button>();

            infoButton.onClick.AddListener(()=>{
                SetDescription(role);
            });
        }
    }

    private void SetDescription(Role role){
        descText.text=role.GetDescription();
        descTitle.text=role.GetName();
        descModal.SetActive(true);
    }
}

