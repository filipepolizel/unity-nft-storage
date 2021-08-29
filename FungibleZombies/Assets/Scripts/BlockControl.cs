using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockControl : MonoBehaviour
{
    private BoxCollider colliderUp;
    private BoxCollider colliderDown;
    private BoxCollider colliderForward;
    private BoxCollider colliderBack;
    private BoxCollider colliderLeft;
    private BoxCollider colliderRight;
    private RaycastHit ObjectHitByMouseRaycast;
    private Color startColor;

    // Start is called before the first frame update
    void Start()
    {
      // initialize original color
      startColor = GetComponent<Renderer>().material.color;

      // initialize internal colliders (one per side)

      colliderUp = gameObject.AddComponent<BoxCollider>();
      colliderUp.center = new Vector3(0.0f, 0.5f, 0.0f);
      colliderUp.size = new Vector3(0.99f, 0.01f, 0.99f);
      colliderUp.isTrigger = true;

      colliderDown = gameObject.AddComponent<BoxCollider>();
      colliderDown.center = new Vector3(0.0f, -0.5f, 0.0f);
      colliderDown.size = new Vector3(0.99f, 0.01f, 0.99f);
      colliderDown.isTrigger = true;

      colliderForward = gameObject.AddComponent<BoxCollider>();
      colliderForward.center = new Vector3(0.0f, 0.0f, 0.5f);
      colliderForward.size = new Vector3(0.99f, 0.99f, 0.01f);
      colliderForward.isTrigger = true;

      colliderBack = gameObject.AddComponent<BoxCollider>();
      colliderBack.center = new Vector3(0.0f, 0.0f, -0.5f);
      colliderBack.size = new Vector3(0.99f, 0.99f, 0.01f);
      colliderBack.isTrigger = true;

      colliderLeft = gameObject.AddComponent<BoxCollider>();
      colliderLeft.center = new Vector3(-0.5f, 0.0f, 0.0f);
      colliderLeft.size = new Vector3(0.01f, 0.99f, 0.99f);
      colliderLeft.isTrigger = true;

      colliderRight = gameObject.AddComponent<BoxCollider>();
      colliderRight.center = new Vector3(0.5f, 0.0f, 0.0f);
      colliderRight.size = new Vector3(0.01f, 0.99f, 0.99f);
      colliderRight.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnMouseEnter()
    {
      GameObject canvasObject = GameObject.FindWithTag("Canvas");
      CanvasSelection selection = canvasObject.GetComponent<CanvasSelection>();
      if (selection.GetSelectionGameObject() != null)
      {
        GetComponent<Renderer>().material.color = Color.green;
      }
      else
      {
        GetComponent<Renderer>().material.color = startColor;
      }
    }

    void OnMouseExit()
    {
      GetComponent<Renderer>().material.color = startColor;
    }

    void OnMouseOver()
    {
      GameObject canvasObject = GameObject.FindWithTag("Canvas");
      CanvasSelection selection = canvasObject.GetComponent<CanvasSelection>();
      if (selection.GetSelectionGameObject() != null)
      {
        if (Input.GetMouseButtonDown(1)) {
          Destroy(this.gameObject);
        }
      }
    }

    void CheckSelectionAndCreateBlock(Vector3 deltaPosition)
    {
      GameObject canvas = GameObject.FindWithTag("Canvas");
      CanvasSelection selection = canvas.GetComponent<CanvasSelection>();
      GameObject template = selection.GetSelectionGameObject();
      if (template != null)
      {
        Instantiate(template, transform.position + deltaPosition, Quaternion.identity);
      }
    }

    void OnMouseDown()
    {
      GameObject canvasObject = GameObject.FindWithTag("Canvas");
      CanvasSelection selection = canvasObject.GetComponent<CanvasSelection>();
      if (selection.GetSelectionGameObject() == null)
      {
        return;
      }
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray.origin, ray.direction, out ObjectHitByMouseRaycast, 2000))
      {
        if (ObjectHitByMouseRaycast.collider.gameObject == this.gameObject)
        {
          if (ObjectHitByMouseRaycast.collider == colliderUp)
          {
            CheckSelectionAndCreateBlock(Vector3.up);
          }
          else if (ObjectHitByMouseRaycast.collider == colliderDown)
          {
            CheckSelectionAndCreateBlock(Vector3.down);
          }
          else if (ObjectHitByMouseRaycast.collider == colliderForward)
          {
            CheckSelectionAndCreateBlock(Vector3.forward);
          }
          else if (ObjectHitByMouseRaycast.collider == colliderBack)
          {
            CheckSelectionAndCreateBlock(Vector3.back);
          }
          else if (ObjectHitByMouseRaycast.collider == colliderLeft)
          {
            CheckSelectionAndCreateBlock(Vector3.left);
          }
          else if (ObjectHitByMouseRaycast.collider == colliderRight)
          {
            CheckSelectionAndCreateBlock(Vector3.right);
          }
        }
      }
    }
}
