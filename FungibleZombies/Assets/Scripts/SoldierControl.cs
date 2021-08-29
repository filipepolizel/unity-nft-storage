using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierControl : MonoBehaviour
{
    private CharacterController controller;
    private Actions actions;
    private GameObject playerCamera;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private bool deadPlayer = false;
    private float playerHealth = 100.0f;
    private float playerSpeed = 0.0f;
    private float playerMaxSpeed = 4.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    public bool movingBackwardsOrSides = false;

    // Start is called before the first frame update
    void Start()
    {
      controller = gameObject.GetComponent<CharacterController>();
      actions = gameObject.GetComponent<Actions>();
      playerCamera = GameObject.Find("PlayerCamera");
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
      string hitObjectName = hit.collider.gameObject.name.Split('(')[0];
      switch (hitObjectName)
      {
        case "RedRock":
          Kill();
          break;
        default:
          break;
      }
    }

    // Update is called once per frame
    void Update()
    {
      if (deadPlayer)
      {
        return;
      }

      groundedPlayer = controller.isGrounded;
      if (groundedPlayer && playerVelocity.y < 0)
      {
        playerVelocity.y = 0f;
      }

      // read input
      float horizontalAxis = Input.GetAxis("Horizontal");
      float verticalAxis = Input.GetAxis("Vertical");

      // camera forward and right vectors
      Vector3 forward = playerCamera.transform.forward;
      Vector3 right = playerCamera.transform.right;

      // project forward and right vectors on the horizontal plane (y = 0)
      forward.y = 0f;
      right.y = 0f;
      forward.Normalize();
      right.Normalize();

      // calculate move direction
      Vector3 move = forward * verticalAxis + right * horizontalAxis;
      if(move.magnitude > 0)
      {
        float speedDelta = Time.deltaTime * playerMaxSpeed * 0.5f;
        playerSpeed = Mathf.Min(playerMaxSpeed, playerSpeed + speedDelta);
        if (verticalAxis < 0 || horizontalAxis != 0)
        {
          movingBackwardsOrSides = true;
        }
      }
      else
      {
        float speedDelta = Time.deltaTime * playerMaxSpeed * 2.0f;
        playerSpeed = Mathf.Max(0.0f, playerSpeed - speedDelta);
      }

      controller.Move(move * Time.deltaTime * playerSpeed);

      if (move != Vector3.zero)
      {
        gameObject.transform.forward = move;
        if (groundedPlayer)
        {
          actions.Run();
        }
      }
      else if (groundedPlayer)
      {
        actions.Stay();
        movingBackwardsOrSides = false;
      }

      // Changes the height position of the player
      // if (Input.GetButtonDown("Jump") && groundedPlayer)
      if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
      {
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        if (playerSpeed == 0.0f)
        {
          actions.Jump();
        }
      }

      playerVelocity.y += gravityValue * Time.deltaTime;
      controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Damage(float damageValue)
    {
      playerHealth -= damageValue;
      actions.Damage();
      if (playerHealth <= 0)
      {
        Kill();
      }
    }

    public void Kill()
    {
      if (!deadPlayer)
      {
        actions.Death();
        deadPlayer = true;
        controller.detectCollisions = false;
        controller.enabled = false;
      }
      ZombieControl[] zombies = FindObjectsOfType(typeof(ZombieControl)) as ZombieControl[];
      foreach(ZombieControl z in zombies)
      {
          z.setPlayerIsDead(true);
      }
    }

    public bool IsDead()
    {
      return deadPlayer;
    }

    public float GetHealth()
    {
      return playerHealth;
    }

    public void SetHealth(float newHealth)
    {
      playerHealth = newHealth;
    }
}
