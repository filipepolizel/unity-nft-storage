using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSelect : MonoBehaviour
{
    public GameObject canvasObject;
    public GameObject blockTemplateObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
      CanvasSelection selection = canvasObject.GetComponent<CanvasSelection>();
      selection.SetSelection(this.gameObject);
    }
}
