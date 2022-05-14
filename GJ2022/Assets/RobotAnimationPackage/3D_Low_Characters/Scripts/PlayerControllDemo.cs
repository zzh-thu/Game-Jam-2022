using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerControllDemo : MonoBehaviour{
    [SerializeField]
    public Animator animator;
    [SerializeField]
    Rigidbody rigidbody;
    [SerializeField]
    private float jumpForce = 7f;
    bool isGrounded = false;
    Dictionary<string, int> actions = new Dictionary<string, int>();
    [HideInInspector]
    public int actionID = 0;
    [Header("Actions Names")]
    string NONE = "_none";
    string IDLE_1 = "idle_01";
    string WALK_1 = "walk_01";
    string RUN_1 = "run_01";
    string JUMP_1 = "jump_01";
    string JUMP_1_FALL = "jump_01_fall";
    string JUMP_1_LAND = "jump_01_land";
    string SPIN_1 = "spin_01";//NOT USED
    string ATTACK_1 = "attack_01";
    string HIT_1 = "hit_01";
    string LOSE_1 = "lose_01";
    string DIE_1 = "die_01";
    string WIN_DANCE_1 = "win_dance_01";
    [Header("Actions ID")]
    int NONE_ID = 0;
    int IDLE_1_ID = 1;
    int WALK_1_ID = 2;
    int RUN_1_ID = 3;
    int JUMP_1_ID = 4;
    int JUMP_1_FALL_ID = 5;
    int JUMP_1_LAND_ID = 6;
    int SPIN_1_ID = 7;//NOT USED
    int ATTACK_1_ID = 8;
    int HIT_1_ID = 9;
    int LOSE_1_ID = 10;
    int DIE_1_ID = 11;
    int WIN_DANCE_1_ID = 12;
    string backActionName = "idle_01";
    int backActionID = 1;
    private void Awake(){
        gameObject.transform.position = new Vector3(0, 5, 0);
        FindComponents();
        actions[NONE] = NONE_ID;
        actions[IDLE_1] = IDLE_1_ID;
        actions[WALK_1] = WALK_1_ID;
        actions[RUN_1] = RUN_1_ID;
        actions[JUMP_1] = JUMP_1_ID;
        actions[JUMP_1_FALL] = JUMP_1_FALL_ID;
        actions[JUMP_1_LAND] = JUMP_1_LAND_ID;
        actions[SPIN_1] = SPIN_1_ID;//NOT USED
        actions[ATTACK_1] = ATTACK_1_ID;
        actions[HIT_1] = HIT_1_ID;
        actions[LOSE_1] = LOSE_1_ID;
        actions[DIE_1] = DIE_1_ID;
        actions[WIN_DANCE_1] = WIN_DANCE_1_ID;
        backActionName = IDLE_1;
        backActionID = IDLE_1_ID;
        UpdateAnimationAction();
    }
    void Start(){
        UpdateAnimationAction();
    }
    public void FindComponents(){ 
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        if (animator == null){
            animator = GetComponentInChildren<Animator>();
        }
    }
    public void Jump(){
        if (isGrounded == true){
            rigidbody.velocity = new Vector3(0f, jumpForce, 0f);
            isGrounded = false;
            SetActionName(JUMP_1);
        }else if (actionID == (int)actions[JUMP_1]){
			// SetActionName(SPIN_1);//NOTE USED
        }
    }
    void Update(){
        UpdateAnimationAction();
    }
    public void SetActionInt(int _actionID = -1){
        StopCoroutine("ReturnToActionCoroutine");
        actionID = _actionID;
        animator.SetInteger("actionID", actionID);
        if (actionID == (int)actions[ATTACK_1] || actionID == (int)actions[HIT_1] || actionID == (int)actions[LOSE_1]){  
            if (FindObjectOfType<CharactersSwipeMenu>().smoothSpeedAnimation == true){
                ReturnToAction(IDLE_1, 3.5f);
            }else{
                ReturnToAction(IDLE_1, 1.5f);
            }
        }
        UpdateAnimationAction();
    }
    public void SetActionName(string _actionName = "idle_01"){
        StopCoroutine("ReturnToActionCoroutine");
        actionID = (int)actions[_actionName];
        animator.SetInteger("actionID", actionID);
        if (actionID == (int)actions[ATTACK_1] || actionID == (int)actions[HIT_1] || actionID == (int)actions[LOSE_1]){
            if (FindObjectOfType<CharactersSwipeMenu>().smoothSpeedAnimation == true){
                ReturnToAction(IDLE_1, 3.5f);
            }else{
                ReturnToAction(IDLE_1, 1.5f);
            }
        }
        UpdateAnimationAction();
    }
    public void SetAnimatorSpeed(float _speed = 1){
        animator.speed = _speed;
    }
    private void UpdateAnimationAction(){
        if (rigidbody.velocity.y > .1f){
            if (actionID != (int)actions[JUMP_1]){
                actionID = (int)actions[JUMP_1];
                SetActionName(JUMP_1);
            }
        }else if (rigidbody.velocity.y < -.1f){
            if (actionID != (int)actions[JUMP_1_FALL]){
                SetActionName(JUMP_1_FALL);
            }
        }
        if (actionID == (int)actions[SPIN_1]){
			//NOTE USED
        }
        if (transform.position.y <= -10){
            RestarLevel();
        }
    }
    void ReturnToAction(string _actionName = "idle_01", float _returnTime = 2.0f){
        backActionName = _actionName;
        backActionID = (int)actions[_actionName];
        StopCoroutine("ReturnToActionCoroutine");
        StartCoroutine("ReturnToActionCoroutine", _returnTime);
    }
    IEnumerator ReturnToActionCoroutine(float _returnTime = 3.0f){
        yield return new WaitForSeconds(_returnTime);
        if (backActionName != ""){
            SetActionName(backActionName);
        }else if (backActionID != -1){
            SetActionInt(backActionID);
        }
    }
    private void OnCollisionEnter(Collision collision){
        if (collision.transform.CompareTag("Ground")){
            isGrounded = true;
            if (actionID == (int)actions[JUMP_1_FALL]){
                SetActionName(JUMP_1_LAND);
                ReturnToAction(IDLE_1, 1.0f);
            }
        }
    }
    private void OnCollisionExit(Collision collision){
        if (collision.transform.CompareTag("Ground")){
            isGrounded = false;
        }
    }
    public void RestarLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
