using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour {

	public Dictionary<Vector2, TileController> tiles; //map positions to tiles
	public Texture2D TileAtlas;
	public Sprite[] TileSprites;

	private GameObject TilePrefab;

	// Use this for initialization
	void Start () {
		tiles = new Dictionary<Vector2, TileController> ();
		TileSprites = Resources.LoadAll<Sprite> (TileAtlas.name);
		TilePrefab = (GameObject) Resources.Load ("Tile");

		CreateTile (Vector2.zero, 6);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CreateTile (Vector2 position, int n) {
		TileController tile = Instantiate (TilePrefab).GetComponent<TileController>();
		tile.Initialize (position, 0);
		tiles.Add (position, tile);
	}

	public TileController GetTileAtPosition (Vector2 position) {
		TileController tile;
		if (tiles.TryGetValue (position, out tile))
			return tile;
		else
			return null;
	}

	//more user-friendly way to index sprites. if a direction is set to true, we will return a sprite with a path heading in that direction
	public Sprite GetTileSpriteByDirection(bool north, bool east, bool south, bool west) {
		if (north && east && south && west)
			return TileSprites [0];
		else if (north && east && south && !west)
			return TileSprites [13];
		else if (north && east && !south && west)
			return TileSprites [12];
		else if (north && !east && south && west)
			return TileSprites [3];
		else if (!north && east && south && west)
			return TileSprites [2];
		else if (!north && !east && south && west)
			return TileSprites [4];
		else if (!north && east && !south && west)
			return TileSprites [5];
		else if (!north && east && south && !west)
			return TileSprites [6];
		else if (north && !east && !south && west)
			return TileSprites [7];
		else if (north && !east && south && !west)
			return TileSprites [15];
		else if (north && east && !south && !west)
			return TileSprites [10];
		else if (north && !east && !south && !west)
			return TileSprites [8];
		else if (!north && east && !south && !west)
			return TileSprites [9];
		else if (!north && !east && south && !west)
			return TileSprites [11];
		else if (!north && !east && !south && west)
			return TileSprites [14];
		else //if (!north && !east && !south && !west)
			return TileSprites [1];
	}
}
