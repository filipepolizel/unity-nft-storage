using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FungibleZombiesNamespace;

public class MapLoader : MonoBehaviour, IPointerUpHandler
{
    private FungibleZombiesMap map;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerUp(PointerEventData data)
    {
      GameObject mapEditorCamera = GameObject.Find("MapEditorCamera");
      FungibleZombies fz = mapEditorCamera.GetComponent<FungibleZombies>();
      fz.LoadMap(map);
    }

    public void SetMap(FungibleZombiesMap newMap)
    {
      map = newMap;
    }
}
