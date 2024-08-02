using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardManager : MonoBehaviour
{
    //-------Keyboard and tasters-------
    [SerializeField] GameObject keyboard;
    [SerializeField] Button backspace;
    [SerializeField] Button uppercase;
    [SerializeField] Button submit;

    //-------Click sound-------
    [SerializeField] AudioSource audioSource;
    [SerializeField] float volume = 0.5f;

    //-------Fields-------
    [SerializeField] List<Sprite> upperSprites = new List<Sprite>();
    private TMP_InputField selectedInput = null;
    private List<Button> letters=new List<Button>();
    private CaseState caseState = CaseState.Lower;

    //------------------------------------------------------------
    //Code for realization of typing
    //------------------------------------------------------------
    
    private enum CaseState{
        Lower,
        UpperOnce,
        Upper
    }

    void Awake()
    {
        InitializeKeyboard();
    }

    private void InitializeKeyboard(){
        foreach(Button b in keyboard.GetComponentsInChildren<Button>()){
            if (b.GetComponentInChildren<TMP_Text>().text!="") letters.Add(b);
        }

        foreach(Button b in letters){
            b.onClick.AddListener(()=>{
                SoundEffect();
                if (selectedInput==null) return;
                selectedInput.text = selectedInput.text + b.GetComponentInChildren<TMP_Text>().text;

                if (caseState==CaseState.UpperOnce){
                    foreach(Button b in letters){
                        string letter = b.GetComponentInChildren<TMP_Text>().text;
                        if (letter[0]>='A' && letter[0]<='Z')
                            b.GetComponentInChildren<TMP_Text>().text =letter.ToLower();
                    }
                    caseState = CaseState.Lower;
                    uppercase.GetComponent<Image>().sprite = upperSprites[0];
                }
            });
        }

        uppercase.onClick.AddListener(()=>{
            SoundEffect();
            NextCaseState();
            foreach(Button b in letters){
                string letter = b.GetComponentInChildren<TMP_Text>().text;
                if (caseState==CaseState.Lower){
                    if (letter[0]>='A' && letter[0]<='Z') b.GetComponentInChildren<TMP_Text>().text = letter.ToLower();
                }
                else {
                    if (letter[0]>='a' && letter[0]<='z') b.GetComponentInChildren<TMP_Text>().text = letter.ToUpper();
                }
            }
        });

        backspace.onClick.AddListener(()=>{
            SoundEffect();
            string s = selectedInput.text;
            if (s!="") selectedInput.text = s.Remove(s.Length-1);
        });

        submit.onClick.AddListener(()=>{
            SoundEffect();
            Hide();
        });

        Hide();
    }
    
    private void NextCaseState(){
        switch(caseState){
            case CaseState.Lower:
                caseState=CaseState.UpperOnce;
                break;
            case CaseState.UpperOnce:
                caseState=CaseState.Upper;
                break;
            case CaseState.Upper:
                caseState=CaseState.Lower;
                break;
        }
        uppercase.GetComponent<Image>().sprite = upperSprites[(int)caseState];
    }

    //------------------------------------------------------------
    //Methods for graphic and audio manipulation
    //------------------------------------------------------------
    
    public void SelectInput(TMP_InputField inp){
        selectedInput = inp;
        Show();
    }

    private void Hide(){
        keyboard.gameObject.transform.localScale = new Vector3(0, 0, 0);
        selectedInput=null;
    }

    private void Show(){
        keyboard.gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    private void SoundEffect(){
        audioSource.PlayOneShot(audioSource.clip, volume);
    }
}
