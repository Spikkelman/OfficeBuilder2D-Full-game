using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase selectedTile;

    [Header("Tile Config")]
    public TileBase[] availableTiles; // Drag & Drop BasicTile1 - BasicTile5
    public int worldId;

    private Dictionary<string, TileBase> tileLookup = new();

    void Start()
    {
        worldId = SelectedWorld.WorldId;
        Debug.Log("TileManager gestart voor world ID: " + worldId);

        foreach (TileBase tile in availableTiles)
        {
            if (tile != null)
                tileLookup[tile.name] = tile;
        }

        StartCoroutine(LoadTilesFromServer());
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0) && selectedTile != null)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            tilemap.SetTile(cellPos, selectedTile);
        }
    }

    public void SetSelectedTile(TileBase tile)
    {
        selectedTile = tile;
        Debug.Log("Selected tile: " + tile.name);
    }

    public void SaveWorld()
    {
        StartCoroutine(SaveTilesToServer());
    }

    IEnumerator SaveTilesToServer()
    {
        Debug.Log("Start met opslaan van tiles...");
        List<TileSaveData> tilesToSave = new();

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                tilesToSave.Add(new TileSaveData
                {
                    tileType = tile.name,
                    x = pos.x,
                    y = pos.y
                });
            }
        }

        string json = JsonUtility.ToJson(new TileSaveDataList { tiles = tilesToSave });
        Debug.Log("Te verzenden JSON (" + tilesToSave.Count + " tiles): " + json);

        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        string token = PlayerPrefs.GetString("auth_token");

        string url = $"https://localhost:7150/api/worlds/{worldId}/tiles";
        UnityWebRequest req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        Debug.Log("PUT naar: " + url);
        Debug.Log("Token (eerste 10 chars): " + (token.Length > 10 ? token.Substring(0, 10) + "..." : token));

        yield return req.SendWebRequest();

        Debug.Log("Response code: " + req.responseCode);
        Debug.Log("Result: " + req.result);
        Debug.Log("Server antwoord: " + req.downloadHandler.text);

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Tiles saved successfully.");
        }
        else
        {
            Debug.LogError("Tile save failed: " + req.downloadHandler.text);
        }
    }

    IEnumerator LoadTilesFromServer()
    {
        string url = $"http://localhost:5136/api/worlds/{worldId}/tiles";
        string token = PlayerPrefs.GetString("auth_token");

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + token);

        Debug.Log("GET van: " + url);
        Debug.Log("Token (eerste 10 chars): " + (token.Length > 10 ? token.Substring(0, 10) + "..." : token));

        yield return req.SendWebRequest();

        Debug.Log("Response code: " + req.responseCode);
        Debug.Log("Result: " + req.result);
        Debug.Log("Server antwoord: " + req.downloadHandler.text);

        if (req.result == UnityWebRequest.Result.Success)
        {
            string raw = req.downloadHandler.text;
            TileSaveData[] loadedTiles = JsonHelper.FromJson<TileSaveData>(raw);

            foreach (var tile in loadedTiles)
            {
                if (tileLookup.TryGetValue(tile.tileType, out TileBase tileBase))
                {
                    Vector3Int pos = new Vector3Int(tile.x, tile.y, 0);
                    tilemap.SetTile(pos, tileBase);
                }
            }

            Debug.Log("Tiles loaded: " + loadedTiles.Length);
        }
        else
        {
            Debug.LogError(" Failed to load tiles: " + req.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class TileSaveData
    {
        public string tileType;
        public int x;
        public int y;
    }

    [System.Serializable]
    public class TileSaveDataList
    {
        public List<TileSaveData> tiles;
    }
}