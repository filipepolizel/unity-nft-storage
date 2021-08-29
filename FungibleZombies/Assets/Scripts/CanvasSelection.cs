using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSelection : MonoBehaviour
{
    private GameObject selectedObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelection(GameObject go)
    {
      if (selectedObject != null)
      {
        selectedObject.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 1);
      }
      if (go != null)
      {
        go.GetComponent<MeshRenderer>().material.color = new Color(0.25f, 1, 0.25f, 1);
      }
      selectedObject = go;
    }

    public GameObject GetSelectionGameObject()
    {
      if (selectedObject != null)
      {
        BlockSelect bs = selectedObject.GetComponent<BlockSelect>();
        return bs.blockTemplateObject;
      }
      else
      {
        return null;
      }
    }
}
