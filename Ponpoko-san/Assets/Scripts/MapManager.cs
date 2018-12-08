using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using SFB;

public class MapManager : MonoBehaviour
{
	public static MapManager instance;

	public bool inMapEdit;
	public bool isCleared;
	public GameObject[] tileOrigins;
	public GameObject cursorObj;
	int currentTileType;

	public Map currentMap;
	List<GameObject> tileObjects = new List<GameObject>();

	public Image editButtonImage;
	public Sprite playIcon;
	public Sprite editIcon;

	public GameObject clearPanel;

	void Awake()
	{
		instance = this;
	}

	public void Initialize()
	{
		foreach(GameObject obj in tileObjects)
		{
			Destroy(obj);
		}

		tileObjects.Clear();

		isCleared = false;
	}

	public void NewMap()
	{
		Initialize();
		currentMap = new Map();
	}

	public void EditButton()
	{
		if (inMapEdit)
		{
			CloseMapEditor();
			editButtonImage.sprite = editIcon;
		}
		else
		{
			OpenMapEditor();
			editButtonImage.sprite = playIcon;
		}

		UpdateMap(currentMap);
	}

	public void UpdateMap(Map map)
	{
		Initialize();

		int targetScore = 0;

		float mapWidth;
		float mapHeight;

		Vector2 min = new Vector2();
		Vector2 max = new Vector2();

		foreach(Tile tile in map.tiles)
		{
			GameObject obj = Instantiate(tileOrigins[tile.type]);
			obj.transform.position = new Vector3(tile.x, tile.y, 0) * 0.32f + Vector3.one * 0.16f;

			if(tile.type == 5)
			{
				targetScore++;
			}

			if(min.x > tile.x)
			{
				min.x = tile.x;
			}
			if(min.y > tile.y)
			{
				min.y = tile.y;
			}
			if(max.x < tile.x)
			{
				max.x = tile.x;
			}
			if(max.y < tile.y)
			{
				max.y = tile.y;
			}

			tileObjects.Add(obj);
		}

		currentMap.targetScore = targetScore;
		mapWidth = max.x - min.x + 5;
		mapHeight = max.y - min.y + 5;

		float xPos = (max.x + min.x) * 0.5f * 0.32f;
		float yPos = (max.y + min.y) * 0.5f * 0.32f;

		Camera.main.transform.position = new Vector3(xPos, yPos, -10);

		if (!inMapEdit)
		{
			if (mapWidth / mapHeight > 16f / 9f)
			{
				Camera.main.orthographicSize = mapWidth * (9f / 16f) * 0.16f;
			}
			else
			{
				Camera.main.orthographicSize = mapHeight * 0.16f;
			}
			if(Camera.main.orthographicSize < 1)
			{
				Camera.main.orthographicSize = 1;
			}

			for (int i = 0; i < currentMap.tiles.Count; i++)
			{
				if (currentMap.tiles[i].type == 7) // 데드존 안보임
				{
					tileObjects[i].GetComponent<SpriteRenderer>().enabled = false;
				}
			}
		}
		else
		{
			Camera.main.orthographicSize = 3.6f;

			for (int i = 0; i < currentMap.tiles.Count; i++)
			{
				if (currentMap.tiles[i].type == 7) // 데드존 보임
				{
					tileObjects[i].GetComponent<SpriteRenderer>().enabled = true;
				}
			}
		}
	}

	public void SaveCurrentMap()
	{
		BinaryFormatter bf = new BinaryFormatter();
		string path = StandaloneFileBrowser.SaveFilePanel("맵 파일", "", "map", "nanoda");
		FileStream file = File.Open(path, FileMode.OpenOrCreate);

		bf.Serialize(file, currentMap);
		file.Close();
	}

	public void LoadMap()
	{
		string path = StandaloneFileBrowser.OpenFilePanel("맵 파일", "", "nanoda", false)[0];
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Open(path, FileMode.Open);

		Map map = (Map)bf.Deserialize(file);
		currentMap = map;

		file.Close();

		UpdateMap(map);
	}

	public void OpenMapEditor()
	{
		inMapEdit = true;
		
		cursorObj.SetActive(true);
	}

	public void CloseMapEditor()
	{
		inMapEdit = false;

		cursorObj.SetActive(false);
	}

	void Start () 
	{
		currentMap = new Map();
	}
	
	void IncreaseTileType()
	{
		int length = tileOrigins.Length - 1;
		if (currentTileType < length)
		{
			currentTileType++;
		}
		else
		{
			currentTileType = 0;
		}
	}

	void DecreaseTileType()
	{
		int length = tileOrigins.Length - 1;
		if (currentTileType > 0)
		{
			currentTileType--;
		}
		else
		{
			currentTileType = length;
		}
	}

	void Update () 
	{
		if (inMapEdit)
		{
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * 10;

			Vector3 griddedPos = GetGriddedPosition(mousePos, 0.32f);

			cursorObj.transform.parent.position = griddedPos * 0.32f;
			
			if(Input.mouseScrollDelta.y == 1)
			{
				IncreaseTileType();
				cursorObj.GetComponent<SpriteRenderer>().sprite = tileOrigins[currentTileType].GetComponent<SpriteRenderer>().sprite;
			}

			if (Input.mouseScrollDelta.y == -1)
			{
				DecreaseTileType();
				cursorObj.GetComponent<SpriteRenderer>().sprite = tileOrigins[currentTileType].GetComponent<SpriteRenderer>().sprite;
			}

			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				if(currentMap.GetTileAtPos(griddedPos) == null || currentMap.GetTileAtPos(griddedPos).type != currentTileType)
				{
					Tile tile = new Tile();
					tile.x = (int)griddedPos.x;
					tile.y = (int)griddedPos.y;
					tile.type = currentTileType;
					if(currentTileType == 5)
					{
						currentMap.targetScore++;
					}

					currentMap.tiles.Add(tile);

					GameObject obj = Instantiate(tileOrigins[tile.type]);
					obj.transform.position = new Vector3(tile.x, tile.y, 0) * 0.32f + Vector3.one * 0.16f;

					tileObjects.Add(obj);
				}
			}

			if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject())
			{
				if (currentMap.GetTileAtPos(griddedPos) != null)
				{
					if(currentMap.GetTileAtPos(griddedPos).type == 5)
					{
						currentMap.targetScore--;
					}

					Destroy(tileObjects[currentMap.GetTileIDAtPos(griddedPos)]);
					tileObjects.RemoveAt(currentMap.GetTileIDAtPos(griddedPos));
					currentMap.tiles.Remove(currentMap.GetTileAtPos(griddedPos));
				}
			}
		}
	}

	public Vector3 GetGriddedPosition(Vector3 position, float gridSize)
	{
		position.x /= gridSize;
		position.x = Mathf.FloorToInt(position.x);

		position.y /= gridSize;
		position.y = Mathf.FloorToInt(position.y);

		return position;
	}

	[System.Serializable]
	public class Map
	{
		public string name;
		public int targetScore = 0;
		public List<Tile> tiles = new List<Tile>();

		public int GetTileIDAtPos(int x, int y)
		{
			int id = -1;

			for (int i = 0; i < tiles.Count; i++)
			{
				if (tiles[i].x == x && tiles[i].y == y)
				{
					id = i;
				}
			}

			return id;
		}

		public int GetTileIDAtPos(Vector3 pos)
		{
			int id = -1;

			for (int i = 0; i < tiles.Count; i++)
			{
				if (tiles[i].x == pos.x && tiles[i].y == pos.y)
				{
					id = i;
				}
			}

			return id;
		}

		public Tile GetTileAtPos(int x, int y)
		{
			Tile found = null;

			foreach(Tile tile in tiles)
			{
				if(tile.x == x && tile.y == y)
				{
					found = tile;
				}
			}

			return found;
		}

		public Tile GetTileAtPos(Vector3 pos)
		{
			Tile found = null;

			foreach (Tile tile in tiles)
			{
				if (tile.x == pos.x && tile.y == pos.y)
				{
					found = tile;
				}
			}

			return found;
		}
	}

	[System.Serializable]
	public class Tile
	{
		public int x, y;
		public int type;
	}
}
