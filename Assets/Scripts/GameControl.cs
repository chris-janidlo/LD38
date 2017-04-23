using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameControl : MonoBehaviour {

	public Dictionary<Vector2, TileController> tiles; //map positions to tiles
	public Texture2D TileAtlas;
	public Texture2D LetterAtlas;
	public Sprite[] TileSprites;
	public Sprite[] LetterSprites;
	public List<int> CurrentLetters;

	private GameObject TilePrefab;

	// Use this for initialization
	void Start () {
		IEnumerable<int> enumerable = Enumerable.Range(0, 25);
		CurrentLetters = enumerable.ToList();
		tiles = new Dictionary<Vector2, TileController> ();
		TileSprites = Resources.LoadAll<Sprite> (TileAtlas.name);
		LetterSprites = Resources.LoadAll<Sprite> (LetterAtlas.name);
		TilePrefab = (GameObject) Resources.Load ("Tile");

		CreateTile (Vector2.zero, 6);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//choose a letter for the Tile at this position
	public Sprite AssignLetter (Vector2 position) {
		int choice = Random.Range (0, CurrentLetters.Count);
		Sprite spr = LetterSprites [CurrentLetters[choice]];
		CurrentLetters.RemoveAt (choice);
		return spr;
	}

	public void CreateTile (Vector2 position, int n) {
		if (CurrentLetters.Count > 0) {
			TileController tile = Instantiate (TilePrefab).GetComponent<TileController>();
			tiles.Add (position, tile);
			tile.Initialize (position, n);
		}
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
