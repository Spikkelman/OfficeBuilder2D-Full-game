using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class TileButton : MonoBehaviour
{
    // The tile asset that this button represents (assign in Inspector)
    public TileBase tile;

    private Button button;

    void Start()
    {
        // Get the Button component on this UI element
        button = GetComponent<Button>();
        // Add a listener to call OnTileButtonClicked when clicked
        button.onClick.AddListener(OnTileButtonClicked);
    }

    void OnTileButtonClicked()
    {
        // Find the TileManager in the scene
        TileManager tileManager = FindObjectOfType<TileManager>();
        if (tileManager != null)
        {
            tileManager.SetSelectedTile(tile);
        }
    }
}