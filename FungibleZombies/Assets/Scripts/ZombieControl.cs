using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieControl : MonoBehaviour
{
    // game object variables
    private CharacterController controller;
    private ZombieAnimation actions;
    private GameObject target;

    // controller variables
    private bool groundedZombie;
    private bool deadZombie = false;
    private bool playerIsDead = false;
    private float attackTimeRemaining = 0.0f;
    private SoldierControl attackTargetController;

    // speed and jump variables
    private float zombieSpeed = 0.0f;
    private float zombieMaxSpeed = 1.0f;
    private float jumpHeight = 0.5f;
    private float gravityValue = -9.81f;
    private Vector3 zombieVelocity;

    // wander variables
    private Vector3 lastWanderMove;
    private float averageWanderChangesPerSecond = 0.3f;
    private float timeToChangeDirection;

    // merge variables
    private bool isMergeTarget = false;
    private GameObject mergingWith = null;
    private float mergeSizeIncrease = 0.0f;
    private float mergeTotalTime = 0.0f;
    private float mergeTimeRemaining = 0.0f;
    private Vector3 initialMergePosition;
    private Vector3 initialMergeScale;

    // Start is called before the first frame update
    void Start()
    {
      controller = gameObject.GetComponent<CharacterController>();
      actions = gameObject.GetComponent<ZombieAnimation>();
      target = GameObject.Find("Soldier");
      lastWanderMove = Vector3.zero;
    }

    // collision controller
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
      string hitObjectName = hit.collider.gameObject.name.Split('(')[0];
      switch (hitObjectName)
      {
        case "RedRock":
          actions.Death();
          deadZombie = true;
          controller.detectCollisions = false;
          break;
        case "Soldier":
          if (!playerIsDead)
          {
            actions.Attack();
            attackTimeRemaining = 0.3f;
            attackTargetController = hit.collider.gameObject.GetComponent<SoldierControl>();
          }
          break;
        default:
          break;
      }
    }

    // merge controller
    void UpdateMerge()
    {
      mergeTimeRemaining -= Time.deltaTime;
      if (mergingWith != null)
      {
        if (mergeTimeRemaining > 0)
        {
          actions.Idle();
          float mergeRatio = mergeTimeRemaining / mergeTotalTime;
          Vector3 finalMergePosition = mergingWith.transform.position;
          finalMergePosition.y += mergingWith.transform.localScale.y / 2.0f;
          transform.position = finalMergePosition - mergeRatio * (finalMergePosition - initialMergePosition);
          transform.localScale = mergeRatio * initialMergeScale;
        }
        else
        {
          Destroy(gameObject);
        }
      }
      else if (isMergeTarget && mergeTimeRemaining <= 0)
      {
        Transform parent = transform.parent;
        Vector3 newScale = transform.localScale;
        newScale *= Mathf.Pow(mergeSizeIncrease, 1.0f / 3.0f);
        transform.SetParent(null);
        transform.localScale = newScale;
        transform.SetParent(parent, true);
        isMergeTarget = false;
        float largestScale = Mathf.Max(newScale.x, newScale.z);
        zombieMaxSpeed = Mathf.Max(1.0f, largestScale / 0.5f);
        jumpHeight = Mathf.Max(0.5f, Mathf.Sqrt(newScale.y));
      }
    }

    // attack controller
    void UpdateAttack()
    {
      attackTimeRemaining -= Time.deltaTime;
      float attackRadius = Mathf.Sqrt(Mathf.Max(transform.localScale.x, transform.localScale.z)) * 0.8f;
      if (
        attackTargetController != null &&
        attackTimeRemaining <= 0 &&
        Vector3.Distance(target.transform.position, transform.position) <= attackRadius
      ) {
        attackTargetController.Damage(GetDamage());
      }
    }

    // wander controller
    void UpdateWander()
    {
      timeToChangeDirection -= Time.deltaTime;
 
      if (timeToChangeDirection <= 0) {
        Vector3 newDirection = new Vector3(-0.5f + Random.value, 0, -0.5f + Random.value);
        newDirection.Normalize();
        transform.forward = newDirection;
        timeToChangeDirection = Random.Range(0.5f / averageWanderChangesPerSecond, 1.5f / averageWanderChangesPerSecond);
      }

      controller.Move(transform.forward * Time.deltaTime * zombieMaxSpeed * 0.25f);
      actions.Walk();
    }

    // gravity controller
    void Gravity()
    {
      groundedZombie = controller.isGrounded;
      if (groundedZombie && zombieVelocity.y < 0)
      {
        zombieVelocity.y = 0f;
      }
      zombieVelocity.y += gravityValue * Time.deltaTime;
      controller.Move(zombieVelocity * Time.deltaTime);
    }

    float GetDamage()
    {
      Vector3 s = transform.localScale;
      float size = s.x * s.y * s.z;
      return size * 10.0f * (1.0f + Random.value);
    }

    // Update is called once per frame
    void Update()
    {
      // first check for merging zombies
      if (IsMerging() && mergeTimeRemaining > 0)
      {
        UpdateMerge();
        if (mergingWith != null)
        {
          return;
        }
      }

      // then process gravity
      Gravity();

      // then check for dead zombies (no actions available)
      if (deadZombie)
      {
        return;
      }

      // then check if attacking (block moving while attacking)
      if (attackTimeRemaining > 0)
      {
        UpdateAttack();
        return;
      }

      // then check for dead player (wander instead of following player)
      if (playerIsDead)
      {
        UpdateWander();
        return;
      }

      // calculate move direction
      Vector3 move = lastWanderMove;
      float moveY = 0.0f;
      move = (target.transform.position - transform.position);
      moveY = move.y;
      move.y = 0.0f;
      move.Normalize();
      if(move.magnitude > 0)
      {
        float speedDelta = Time.deltaTime * zombieMaxSpeed * 0.5f;
        zombieSpeed = Mathf.Min(zombieMaxSpeed, zombieSpeed + speedDelta);
      }
      else
      {
        float speedDelta = Time.deltaTime * zombieMaxSpeed * 2.0f;
        zombieSpeed = Mathf.Max(0.0f, zombieSpeed - speedDelta);
      }

      controller.Move(move * Time.deltaTime * zombieSpeed);

      if (move != Vector3.zero)
      {
        gameObject.transform.forward = move;
        actions.Run();
      }

      // zombie jump
      if (move.magnitude > 0 && moveY / move.magnitude >= 0.3 && groundedZombie)
      {
        zombieVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        if (zombieSpeed == 0.0f)
        {
          actions.Jump();
        }
      }
    }

    public void setPlayerIsDead(bool isDead)
    {
      playerIsDead = isDead;
    }

    public float GetMergeSize()
    {
      Vector3 s = transform.localScale;
      return s.x * s.y * s.z;
    }

    public bool IsMerging()
    {
      return (isMergeTarget || mergingWith != null);
    }

    public void InitiateMerge(GameObject source, GameObject target, float sizeIncrease)
    {
      bool isSource = (gameObject == source);
      if (isSource)
      {
        isMergeTarget = false;
        mergingWith = target;
        controller.detectCollisions = false;
        controller.enabled = false;
      }
      else
      {
        isMergeTarget = true;
        mergingWith = null;
      }
      mergeSizeIncrease = sizeIncrease;
      mergeTotalTime = 1.0f;
      mergeTimeRemaining = mergeTotalTime;
      initialMergePosition = transform.position;
      initialMergeScale = transform.localScale;
    }
}
