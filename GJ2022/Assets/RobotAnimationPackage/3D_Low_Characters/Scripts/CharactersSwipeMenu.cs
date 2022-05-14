using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CharactersSwipeMenu : MonoBehaviour{
	
    [SerializeField]
    bool autoRotation = true;
    public bool smoothSpeedAnimation = false;
    [SerializeField]
    Toggle rotateToggle;
    [SerializeField]
    Toggle speedToggle;
    [SerializeField]
    bool hideOthersCharacters = true;
    [SerializeField]
    Text characterInfoText;
    [SerializeField]
    GameObject[] charactersList = null;
    int characterID = 0;
    float rotationAtual = -90;
    public PlayerControllDemo playerControllDemo;
    GameObject character;
    int animatorActionID = 0;
    float animatorSpeed;
    Transform characterTransform;
	
    private void Awake(){
        character = charactersList[characterID];
        playerControllDemo = charactersList[characterID].GetComponent<PlayerControllDemo>();
        getPreviousCharacterValues();
    }
    void Start(){
        getPreviousCharacterValues();
        HideCharacters();
        ShowAtualCharacter();
    }
    void HideCharacters(){
        if (charactersList != null){
            for (int i = 0; i < charactersList.Length; i++){
                GameObject character = charactersList[i];
                character.transform.parent.rotation = new Quaternion(0, rotationAtual, 0, 0);
                if (hideOthersCharacters == true){
                    character.SetActive(false);
                }
            }
        }
        ShowCharacterInfo();
    }
    void ShowAtualCharacter(){
        if (charactersList != null){
            character = charactersList[characterID];
            playerControllDemo = character.GetComponent<PlayerControllDemo>();
            playerControllDemo.FindComponents();
            if (playerControllDemo == null){
                Debug.LogWarning("playerControllDemo NOT FOUND!");
            }
            character.SetActive(true);
            character.transform.position = characterTransform.position;
            character.transform.rotation = characterTransform.rotation;
            playerControllDemo.animator.speed = animatorSpeed;
            SetActionInt(animatorActionID);
        }
        ShowCharacterInfo();
    }
    void getPreviousCharacterValues(){
        characterTransform = character.transform;
        animatorSpeed = playerControllDemo.animator.speed;
        animatorActionID = playerControllDemo.actionID;
    }
    void ShowCharacterInfo(){
        if (characterInfoText != null){
            characterInfoText.text = "Character " + (characterID + 1) + "/" + charactersList.Length;
        }
    }
    public void PreviousCharacter(){
        getPreviousCharacterValues();
        if (characterID > 0){
            characterID--;
            HideCharacters();
            ShowAtualCharacter();
        }
    }
    public void NextCharacter(){
        getPreviousCharacterValues();
        if (characterID < charactersList.Length - 1){
            characterID++;
            HideCharacters();
            ShowAtualCharacter();
        }
    }
    public void Jump(){
        playerControllDemo.Jump();
    }
    public void SetActionInt(int _actionID = -1){
        playerControllDemo.SetActionInt(_actionID);
    }
    public void SetActionName(string _actionName = "idle_01"){
        playerControllDemo.SetActionName(_actionName);
    }
    public void ToogleRotateCharacter(bool value){
        autoRotation = value;
    }
    public void ToogleSmoothSpeedAnimation(bool value){
        smoothSpeedAnimation = value;
        if (smoothSpeedAnimation == true){
            SetAnimatorSpeed(0.2f);
        }else{
            SetAnimatorSpeed(1.0f);
        }
    }
    public void RotateCharacter(float rotation = -1){
        GameObject character = charactersList[characterID];
        if (character.activeSelf == true){ 
            if (rotation != -1){
                rotationAtual = character.transform.rotation.y;
                rotationAtual += rotation;
                character.transform.RotateAround(transform.position, transform.up, rotationAtual);
            }else{
                character.transform.RotateAround(transform.position, transform.up, Time.deltaTime * -90f);
                rotationAtual = character.transform.rotation.y;
            }
        }
    }
    public void SetAnimatorSpeed(float _speed = 1){
        playerControllDemo.SetAnimatorSpeed(_speed);
    }
    void Update(){
        GetKeyBoard();
        if (autoRotation){
            RotateCharacter();
        }
    }
    void GetKeyBoard(){
        if (Input.GetKeyDown(KeyCode.LeftArrow)){
            PreviousCharacter();
        }else if (Input.GetKeyDown(KeyCode.RightArrow)){
            NextCharacter();
        }
        if (Input.GetKeyDown(KeyCode.R)){
            ToogleRotateCharacter(!autoRotation);
            if (rotateToggle != null){
                rotateToggle.isOn = autoRotation;
            }
        }
        if (Input.GetKeyDown(KeyCode.S)){
            ToogleSmoothSpeedAnimation(!smoothSpeedAnimation);
            if (speedToggle != null){
                speedToggle.isOn = smoothSpeedAnimation;
            }
        }
        if (Input.GetKeyDown(KeyCode.T)){
            SetActionName("_none");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            SetActionName("idle_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){
            SetActionName("walk_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){
            SetActionName("run_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)){
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)){
			 // SetActionName("spin_01"); //NOTE USED
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)){
            SetActionName("attack_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha7)){
            SetActionName("hit_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8)){
            SetActionName("lose_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9)){
            SetActionName("die_01");
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)){
            SetActionName("win_dance_01");
        }
    }
}


