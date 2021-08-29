using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NFTStorage;
using NFTStorage.JSONSerialization;

namespace FungibleZombiesNamespace
{
    [Serializable]
    public class FungibleZombiesBlock {
      public int x;
      public int y;
      public int z;
      public string type;
    }

    [Serializable]
    public class FungibleZombiesMap {
      public string cid;
      public string name;
      public string createdAt;
      public int minX;
      public int maxX;
      public int minY;
      public int maxY;
      public List<FungibleZombiesBlock> blocks;
    }

    public class FungibleZombies : MonoBehaviour
    {
        public Camera mapEditorCamera;
        public Camera playerCamera;
        public GameObject playerGameObject;
        public GameObject zombieGameObject;
        public GameObject gridCell;
        public GameObject redRock;
        public GameObject blockCrate01;
        public GameObject blockCrate02;
        public GameObject blockCrate03;
        public GameObject blockCrate04;
        public GameObject blockFloor01;
        public GameObject blockFloor02;
        public GameObject blockFloor03;
        public GameObject blockFloor04;
        public GameObject blockFantasyWall01;
        public GameObject blockRock01;
        public GameObject blockRock02;
        public GameObject blockRock03;
        public GameObject blockRock04;
        public GameObject blockScifiPanel01;
        public GameObject blockScifiPanel02;
        public GameObject blockScifiPanel03;
        public GameObject blockScifiWall01;
        public GameObject blockMetalBasic01;
        public GameObject mapLoaderPanel;
        public GameObject mapRow;
        private string mapName = "Unnamed";
        private int boardXMin = -5;
        private int boardXMax = 5;
        private int boardYMin = -5;
        private int boardYMax = 5;
        private float baseAverageZombiesPerSecond = 1.0f;
        private string[] blockTypes = {
          "Crate01", "Crate02", "Crate03", "Crate04", "Floor01", "Floor02", "Floor03", "Floor04",
          "FantasyWall01", "Rock01", "Rock02", "Rock03", "Rock04", "ScifiPanel01", "ScifiPanel02",
          "ScifiPanel03", "ScifiWall01", "MetalBasic01"
        };
        private Dictionary<string, GameObject> blockTemplates = new Dictionary<string, GameObject>();
        private List<FungibleZombiesMap> loadedMapsData = null;
        private string[] threatLevelNames = {
          "Paradise", "Relaxing", "Boring", "Very Low", "Low", "Somewhat Low", "Almost Medium", "Medium",
          "Somewhat High", "High", "Very High", "Angustiating", "Nightname", "Apocalyptical", "Infernal"
        };
        private int threatLevel;
        private int zombieCount;
        private float score;
        private float time;

        // Start is called before the first frame update
        void Start()
        {
          // setup block templates
          blockTemplates.Add("Crate01", blockCrate01);
          blockTemplates.Add("Crate02", blockCrate02);
          blockTemplates.Add("Crate03", blockCrate03);
          blockTemplates.Add("Crate04", blockCrate04);
          blockTemplates.Add("Floor01", blockFloor01);
          blockTemplates.Add("Floor02", blockFloor02);
          blockTemplates.Add("Floor03", blockFloor03);
          blockTemplates.Add("Floor04", blockFloor04);
          blockTemplates.Add("FantasyWall01", blockFantasyWall01);
          blockTemplates.Add("Rock01", blockRock01);
          blockTemplates.Add("Rock02", blockRock02);
          blockTemplates.Add("Rock03", blockRock03);
          blockTemplates.Add("Rock04", blockRock04);
          blockTemplates.Add("ScifiPanel01", blockScifiPanel01);
          blockTemplates.Add("ScifiPanel02", blockScifiPanel02);
          blockTemplates.Add("ScifiPanel03", blockScifiPanel03);
          blockTemplates.Add("ScifiWall01", blockScifiWall01);
          blockTemplates.Add("MetalBasic01", blockMetalBasic01);

          // setup cameras
          mapEditorCamera.enabled = true;
          playerCamera.enabled = false;
          playerGameObject.SetActive(false);

          // setup empty board
          ResetBoard();
        }

        // Update is called once per frame
        void Update()
        {
          // show map list panel if finished loading data from coroutine
          if (loadedMapsData != null && loadedMapsData.Count > 0 && !mapLoaderPanel.gameObject.activeSelf)
          {
            OpenMapList();
          }

          // check if game is on
          if (playerGameObject.activeSelf)
          {
            // check if player is alive
            if (!playerGameObject.GetComponent<SoldierControl>().IsDead())
            {
              // update game variables
              time += Time.deltaTime;
              score += Time.deltaTime * zombieCount;
              threatLevel = (int)Mathf.Max(0, Mathf.Min(14, Mathf.Floor(Mathf.Log((float)zombieCount))));

              // spawn zombies randomly
              float zombiesPerSec = baseAverageZombiesPerSecond * (1.0f + threatLevel);
              if (UnityEngine.Random.value < zombiesPerSec * Time.deltaTime)
              {
                Instantiate(
                  zombieGameObject,
                  new Vector3(
                    UnityEngine.Random.Range(boardXMin, boardXMax),
                    UnityEngine.Random.Range(1, 3),
                    UnityEngine.Random.Range(boardYMin, boardYMax)
                  ),
                  Quaternion.identity
                );
                zombieCount++;
              }

              // check if any zombies should be merged
              UpdateMergeZombies();
            }
            // check player UI
            UpdatePlayerCanvas();
          }
        }

        void UpdateMergeZombies()
        {
          ZombieControl[] zombies = GameObject.FindObjectsOfType(typeof(ZombieControl)) as ZombieControl[];
          foreach (ZombieControl z1 in zombies)
          {
            Vector3 scale = z1.gameObject.transform.localScale;
            float radius = Mathf.Max(scale.x, scale.z) * 0.5f;
            Collider[] colliders = Physics.OverlapSphere(z1.gameObject.transform.position, radius);
            for(int i = 0; i <= colliders.Length - 1; i++)
            {
              ZombieControl z2 = colliders[i].gameObject.GetComponent<ZombieControl>();
              if (z2 != null && z1.gameObject != z2.gameObject && !z1.IsMerging() && !z2.IsMerging())
              {
                float s1 = z1.GetMergeSize();
                float s2 = z2.GetMergeSize();
                float newSize = (s1 + s2);
                float d1 = Vector3.Distance(playerGameObject.transform.position, z1.gameObject.transform.position);
                float d2 = Vector3.Distance(playerGameObject.transform.position, z2.gameObject.transform.position);
                float p1 = z1.gameObject.transform.position.magnitude;
                float p2 = z1.gameObject.transform.position.magnitude;
                if (s1 > s2 || (s1 == s2 && d1 < d2) || (s1 == s2 && d1 == d2 && p1 < p2))
                {
                  z1.InitiateMerge(z2.gameObject, z1.gameObject, newSize / s1);
                  z2.InitiateMerge(z2.gameObject, z1.gameObject, newSize / s1);
                }
                else
                {
                  z1.InitiateMerge(z1.gameObject, z2.gameObject, newSize / s2);
                  z2.InitiateMerge(z1.gameObject, z2.gameObject, newSize / s2);
                }
              }
            }
          }
        }

        void UpdatePlayerCanvas()
        {
          GameObject playerCanvas = GameObject.Find("PlayerCanvas");

          foreach (Transform child in playerCanvas.transform)
          {
            switch (child.name)
            {
              case "HealthBar":
                SoldierControl sc = playerGameObject.GetComponent<SoldierControl>();
                Transform healthBar = child.transform.Find("HealthBarInner");
                RectTransform healthBarRect = healthBar.gameObject.GetComponent<RectTransform>();
                float relativeHealth = sc.GetHealth() / 100.0f;
                Color32 healthColor = new Color32(
                  (byte)(175 - (int)(100 * relativeHealth)),
                  (byte)(75 + (int)(100 * relativeHealth)),
                  (byte)75,
                  (byte)255
                );
                healthBar.gameObject.GetComponent<UnityEngine.UI.Image>().color = healthColor;
                int healthBarWidth = (int)Mathf.Round(300.0f * relativeHealth);
                healthBarRect.sizeDelta = new Vector2(healthBarWidth, 13);
                break;
              case "ThreatLevel":
                string threatLevelText = "<b>Threat Level:</b> " + threatLevel.ToString() + " (" + threatLevelNames[threatLevel] + ")";
                child.GetComponent<UnityEngine.UI.Text>().text = threatLevelText;
                break;
              case "Zombies":
                string zombiesText = "<b>Zombies:</b> " + zombieCount.ToString();
                child.GetComponent<UnityEngine.UI.Text>().text = zombiesText;
                break;
              case "Time":
                string minutes = Mathf.Floor(time / 60).ToString().PadLeft(2, '0');
                string seconds = ((int)time % 60).ToString().PadLeft(2, '0');
                string goTimeText = "<b>Time:</b> " + minutes + ":" + seconds;
                child.GetComponent<UnityEngine.UI.Text>().text = goTimeText;
                break;
              case "Score":
                string scoreText = "<b>Score:</b> " + Mathf.Floor(score).ToString();
                child.GetComponent<UnityEngine.UI.Text>().text = scoreText;
                break;
              default:
                break;
            }
          }
        }

        void ResetBoard(List<FungibleZombiesBlock> blocks = null)
        {
          // remove existing blocks from the game
          BlockControl[] existingBlocks = FindObjectsOfType(typeof(BlockControl)) as BlockControl[];
          foreach (BlockControl bc in existingBlocks)
          {
            Destroy(bc.gameObject);
          }

          // create grid cells
          for (int i = boardXMin; i <= boardXMax; i++)
          {
            for (int j = boardYMin; j <= boardYMax; j++)
            {
              Instantiate(gridCell, new Vector3(i, -0.5f, j), Quaternion.identity);
            }
          }

          // create red rock (kill on touch, in case player falls)
          GameObject newRedRock = Instantiate(redRock, new Vector3(0.0f, -2.0f, 0.0f), Quaternion.identity);
          float scaleX = 10.0f * Mathf.Max(Mathf.Abs(boardXMin), Mathf.Abs(boardXMax));
          float scaleY = 10.0f * Mathf.Max(Mathf.Abs(boardYMin), Mathf.Abs(boardYMax));
          newRedRock.transform.localScale = new Vector3(scaleX, 1.0f, scaleY);
          Renderer redRockRend = newRedRock.GetComponent<Renderer>();
          redRockRend.material.mainTextureScale = new Vector2(scaleX, scaleY);

          // create blocks
          if (blocks != null && blocks.Count > 0)
          {
            foreach (FungibleZombiesBlock b in blocks)
            {
              Vector3 newBlockPosition = new Vector3(b.x, b.y, b.z);
              GameObject newBlockTemplate = blockTemplates[b.type];
              GameObject newBlock = Instantiate(newBlockTemplate, newBlockPosition, Quaternion.identity);
              newBlock.transform.SetParent(gameObject.transform.parent, false);
            }
          }
        }

        void OpenMapList()
        {
          mapLoaderPanel.gameObject.SetActive(true);
          GameObject mapLoaderContent = GameObject.Find("MapLoaderContent");
          int rowNumber = 0;
          foreach (FungibleZombiesMap map in loadedMapsData)
          {
            Vector3 mapRowPosition = new Vector3(0, 100 - 30 * rowNumber, 0);
            GameObject newMapRow = Instantiate(mapRow, mapRowPosition, Quaternion.identity);
            newMapRow.transform.SetParent(mapLoaderContent.transform, false);
            foreach (Transform child in newMapRow.transform) {
              switch (child.name)
              {
                case "Name":
                  string shortCid = map.cid.Substring(0, 4) + "..." + map.cid.Substring(map.cid.Length - 4);
                  child.GetComponent<UnityEngine.UI.Text>().text = map.name + " (" + shortCid + ")";
                  break;
                case "CreatedAt":
                  string createdAtStr = map.createdAt.Substring(0, 10) + " " + map.createdAt.Substring(11, 5);
                  child.GetComponent<UnityEngine.UI.Text>().text = createdAtStr;
                  break;
                case "Blocks":
                  child.GetComponent<UnityEngine.UI.Text>().text = map.blocks.Count.ToString();
                  break;
                case "Actions":
                  foreach (Transform child2 in child.transform)
                  {
                    if (child2.name == "LoadMapButton")
                    {
                      child2.GetComponent<MapLoader>().SetMap(map);
                    }
                  }
                  break;
                default:
                  break;
              }
            }
            rowNumber++;
          }
        }

        string GetCurrentMapFile()
        {
          BlockControl[] blocks = (BlockControl[]) GameObject.FindObjectsOfType(typeof(BlockControl));
          List<string> blockData = new List<string>();
          foreach (BlockControl blk in blocks)
          {
            Vector3 pos = blk.gameObject.transform.position;
            string name = blk.gameObject.name.Split('(')[0];
            int typeIndex = Array.FindIndex(blockTypes, row => row == name);
            if (typeIndex >= 0) {
              string newData = pos[0].ToString() + ',' + pos[1].ToString() + ',' + pos[2].ToString() + ',' + typeIndex;
              blockData.Add(newData);
            }
          }
          blockData.Sort();
          string headers = ("FZM[" +
            mapName + "," +
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") + "Z," +
            boardXMin.ToString() + "," +
            boardXMax.ToString() + "," +
            boardYMin.ToString() + "," +
            boardYMax.ToString() + "]|"
          );
          return headers + String.Join("|", blockData.ToArray());
        }

        public void OnChangeMapName(string newValue)
        {
          if (newValue != null && newValue.Length > 0)
          {
            mapName = newValue;
          }
          else
          {
            mapName = "Unnamed";
          }
        }

        public void SaveMap()
        {
          try
          {
            NFTStorageClient nftStorage = this.gameObject.GetComponent<NFTStorageClient>();
            string data = GetCurrentMapFile();
            Task.Run(async () => await Task.Factory.StartNew(async () => {
              var response = await nftStorage.UploadDataFromString(data);
              Debug.Log(response);
            }));
          }
          catch(Exception e)
          {
            Debug.Log("Error occurred while saving map: " + e.Message);
          }
        }

        public void LoadMapList()
        {
          try
          {
            NFTStorageClient nftStorage = this.gameObject.GetComponent<NFTStorageClient>();
            Task.Run(async () => await Task.Factory.StartNew(async () => {
              NFTStorageListFilesResponse listFilesResponse = await nftStorage.ListFiles();
              NFTStorageNFTObject[] nftObjects = listFilesResponse.value;
              List<FungibleZombiesMap> loadedMaps = new List<FungibleZombiesMap>();
              foreach (NFTStorageNFTObject nft in nftObjects)
              {
                string cid = nft.cid;
                string fileData = await nftStorage.GetFileData(cid);
                string[] rows = fileData.Split('|');
                if (rows.Length > 1)
                {
                  string rawHeaders = rows[0];
                  if (
                    String.Equals(rawHeaders.Substring(0, 4), "FZM[") &&
                    String.Equals(rawHeaders.Substring(rawHeaders.Length - 1), "]")
                  )
                  {
                    // instantiate new map
                    FungibleZombiesMap newMap = new FungibleZombiesMap();
                    newMap.cid = cid;

                    // parse headers
                    string[] rawHeaderValues = rawHeaders.Substring(4, rawHeaders.Length - 5).Split(',');
                    string name = rawHeaderValues[0];
                    string createdAt = rawHeaderValues[1];
                    newMap.name = name;
                    newMap.createdAt = createdAt;
                    newMap.minX = Int32.Parse(rawHeaderValues[2]);
                    newMap.maxX = Int32.Parse(rawHeaderValues[3]);
                    newMap.minY = Int32.Parse(rawHeaderValues[4]);
                    newMap.maxY = Int32.Parse(rawHeaderValues[5]);

                    // parse blocks
                    newMap.blocks = new List<FungibleZombiesBlock>();
                    for (int i = 1; i < rows.Length; i++)
                    {
                      FungibleZombiesBlock newBlock = new FungibleZombiesBlock();
                      string[] rawBlockValues = rows[i].Split(',');
                      newBlock.x = Int32.Parse(rawBlockValues[0]);
                      newBlock.y = Int32.Parse(rawBlockValues[1]);
                      newBlock.z = Int32.Parse(rawBlockValues[2]);
                      newBlock.type = blockTypes[Int32.Parse(rawBlockValues[3])];
                      newMap.blocks.Add(newBlock);
                    }

                    // append new map to list of all maps
                    loadedMaps.Add(newMap);
                  }
                }
              }
              loadedMapsData = loadedMaps;
            }));
          }
          catch(Exception e)
          {
            Debug.Log("Error occurred while loading maps: " + e.Message);
          }
        }

        public void CloseMapList()
        {
          mapLoaderPanel.gameObject.SetActive(false);
          loadedMapsData = null;
        }

        public void LoadMap(FungibleZombiesMap map)
        {
          // update map name
          GameObject nameGameObject = GameObject.Find("MapNameInputField");
          UnityEngine.UI.InputField inputField = nameGameObject.GetComponent<UnityEngine.UI.InputField>();
          inputField.text = map.name;
          mapName = map.name;

          // update board limits
          boardXMin = map.minX;
          boardXMax = map.maxX;
          boardYMin = map.minY;
          boardYMax = map.maxY;

          // setup new board
          ResetBoard(map.blocks);

          // close map list panel
          CloseMapList();
        }

        public void PlayMap()
        {
          // reset map stats
          playerGameObject.GetComponent<SoldierControl>().SetHealth(100.0f);
          threatLevel = 0;
          zombieCount = 0;
          time = 0.0f;
          score = 0.0f;

          // spawn player
          playerGameObject.SetActive(true);

          // change camera settings
          mapEditorCamera.enabled = false;
          playerCamera.enabled = true;

          // clear canvas block selection + hide it
          GameObject canvasObject = GameObject.FindWithTag("Canvas");
          CanvasSelection selection = canvasObject.GetComponent<CanvasSelection>();
          selection.SetSelection(null);
        }
    }
}