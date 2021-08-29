using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellControl : MonoBehaviour
{
    private Color startColor;

    // Start is called before the first frame update
    void Start()
    {
      startColor = GetComponent<Renderer>().material.color;
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

    void OnMouseDown()
    {
      GameObject canvas = GameObject.FindWithTag("Canvas");
      CanvasSelection selection = canvas.GetComponent<CanvasSelection>();
      GameObject template = selection.GetSelectionGameObject();
      if (template != null)
      {
        Instantiate(template, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
      }
    }
}
