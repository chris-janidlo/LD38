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
	public int GenerationSeed = 8;
	public Vector2 PlayerEndOnePos;
	public Vector2 PlayerEndTwoPos;
	public Vector2 Null = new Vector2 (100, 100); //some dummy value we don't expect to run in to when testing for nullity
	public float ShakeTime = 0.5f; //how long to screen shake
	public float ShakeAmount = 1.0f; //how far away from center the screen will shake at max
	public float CameraZ = -10.0f;
	public float IFrameTime = 1.0f; //time after taking damage when you are invincible

	private GameObject TilePrefab;
	private Camera cam;
	private List<Vector2> TilesWithPlayer;
	private float shakeCount = 0.0f;
	private Vector3 center; //where the camera was most recently centered
	private bool centered; //whether or not the camera is already centered
	private bool damaged; //in a damaged state where you need to release all buttons before pressing any again
	private float iFrameCount;

	void Start () {
		IEnumerable<int> enumerable = Enumerable.Range(0, 25);
		CurrentLetters = enumerable.ToList();
		tiles = new Dictionary<Vector2, TileController> ();

		TileSprites = Resources.LoadAll<Sprite> (TileAtlas.name);
		LetterSprites = Resources.LoadAll<Sprite> (LetterAtlas.name);
		TilePrefab = (GameObject) Resources.Load ("Tile");

		CreateTile (Vector2.zero, GenerationSeed);
		damaged = false;
	
		cam = gameObject.GetComponentInChildren<Camera> ();
		CenterCamera ();

		InitializePlayer ();
	}

	void InitializePlayer () {
		PlayerEndOnePos = Null;
		PlayerEndTwoPos = Null;
		TilesWithPlayer = new List<Vector2> ();
	}

	void FixedUpdate () {
		if (shakeCount > 0) {
			centered = false;
			cam.transform.position = new Vector3 (center.x + Random.Range (-1 * ShakeAmount * shakeCount, ShakeAmount * shakeCount), center.y + Random.Range (-1 * ShakeAmount * shakeCount, ShakeAmount * shakeCount), CameraZ);
			shakeCount -= Time.deltaTime;
		} 
		if (!centered && shakeCount <= 0) {
			CenterCamera ();
		}
		if (damaged && !(Input.anyKey))
			StartCoroutine("IFrames");
	}

	//choose a letter for the Tile at this position
	public int AssignLetter (Vector2 position) {
		int rand = Random.Range (0, CurrentLetters.Count), choice = CurrentLetters [rand];
		CurrentLetters.RemoveAt (rand);
		return choice;
	}

	IEnumerator IFrames () {
		yield return new WaitForSeconds(IFrameTime);
		damaged = false;
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

	//center the camera over the average position of the tiles
	void CenterCamera () {
		float x = 0, y = 0, count = 0;
		foreach (KeyValuePair<Vector2, TileController> entry in tiles) {
			x += entry.Key.x;
			y += entry.Key.y;
			count++;
		}
		//the camera is a child of the object this script is attached to, so we can just move the transform
		cam.transform.position = new Vector3 (x / count, y / count, CameraZ);
		center = cam.transform.position;
		centered = true;
	}

	//apply damage and clear the player
	public void Damage () {
		if (damaged)
			return;
		shakeCount = ShakeTime;
		damaged = true;
		foreach (Vector2 pos in TilesWithPlayer) {
			TileController t = GetTileAtPosition (pos);
			t.playerAnim.SetBool ("exists", false);
		}
		InitializePlayer ();
	}

	bool NextToEndOne (Vector2 testPos) {
		if (PlayerEndOnePos == Null)
			return true;
		bool sides = ((testPos.x == PlayerEndOnePos.x + 1 ^ testPos.x == PlayerEndOnePos.x - 1) && testPos.y == PlayerEndOnePos.y);
		bool updown = ((testPos.y == PlayerEndOnePos.y + 1 ^ testPos.y == PlayerEndOnePos.y - 1) && testPos.x == PlayerEndOnePos.x);
		return sides || updown;
	}

	bool NextToEndTwo (Vector2 testPos) {
		if (PlayerEndTwoPos == Null)
			return false;
		bool sides = ((testPos.x == PlayerEndTwoPos.x + 1 ^ testPos.x == PlayerEndTwoPos.x - 1) && testPos.y == PlayerEndTwoPos.y);
		bool updown = ((testPos.y == PlayerEndTwoPos.y + 1 ^ testPos.y == PlayerEndTwoPos.y - 1) && testPos.x == PlayerEndTwoPos.x);
		return sides || updown;
	}

	bool ValidPlayerPosition (Vector2 testPos, bool nextToEndOne, bool nextToEndTwo) {
		if (GetTileAtPosition (testPos) == null)
			return false; //don't expect this to happen, since this method is only called when an input is called from an existing tile, but might as well be safe
		if (TilesWithPlayer.Count == 0) return true;
		foreach (Vector2 p in TilesWithPlayer)
			if (p == testPos)
				return false;
		return nextToEndOne ^ nextToEndTwo;
	}

	public void AddToPlayer (Vector2 pos) {
		bool endOne = NextToEndOne (pos), endTwo = NextToEndTwo (pos), dontSetTwo = false;
		Debug.Log (endOne + " " + endTwo);
		if (!ValidPlayerPosition (pos, endOne, endTwo)) {
			Damage ();
			GetTileAtPosition (pos).playerAnim.SetBool ("exists", false);
			return;
		}
		GetTileAtPosition (pos).playerAnim.SetBool ("exists", true);
		if (PlayerEndOnePos == Null) {
			PlayerEndOnePos = pos;
			TilesWithPlayer.Add (pos);
			return;
		}
		if (PlayerEndTwoPos == Null) {
			PlayerEndTwoPos = pos;
			TilesWithPlayer.Add (pos);
			dontSetTwo = true;
		}
		TileController tileOne = GetTileAtPosition (PlayerEndOnePos), tileTwo = GetTileAtPosition (PlayerEndTwoPos), newTile = GetTileAtPosition (pos);
		if (endOne) {
			if (pos.y == PlayerEndOnePos.y + 1) { //new player is north
				tileOne.playerAnim.SetTrigger ("north");
				newTile.playerAnim.SetTrigger ("south");
			} else if (pos.y == PlayerEndOnePos.y - 1) { //new player is south
				tileOne.playerAnim.SetTrigger ("south");
				newTile.playerAnim.SetTrigger ("north");
			} else if (pos.x == PlayerEndOnePos.x + 1) { //new player is east
				tileOne.playerAnim.SetTrigger ("east");
				newTile.playerAnim.SetTrigger ("west");
			} else {//if (pos.x == PlayerEndOnePos.x - 1) new player is west
				tileOne.playerAnim.SetTrigger ("west");
				newTile.playerAnim.SetTrigger ("east");
			}
			if (!dontSetTwo) PlayerEndOnePos = pos;
		} else { //if (endTwo)
			if (pos.y == PlayerEndTwoPos.y + 1) { //new player is north
				tileTwo.playerAnim.SetTrigger ("north");
				newTile.playerAnim.SetTrigger ("south");
			} else if (pos.y == PlayerEndTwoPos.y - 1) { //new player is south
				tileTwo.playerAnim.SetTrigger ("south");
				newTile.playerAnim.SetTrigger ("north");
			} else if (pos.x == PlayerEndTwoPos.x + 1) { //new player is east
				tileTwo.playerAnim.SetTrigger ("east");
				newTile.playerAnim.SetTrigger ("west");
			} else {//if (pos.x == PlayerEndTwoPos.x - 1) new player is west
				tileTwo.playerAnim.SetTrigger ("west");
				newTile.playerAnim.SetTrigger ("east");
			}
			PlayerEndTwoPos = pos;
		}
		TilesWithPlayer.Add (pos);
		Debug.Log (PlayerEndOnePos);
		Debug.Log (PlayerEndTwoPos);
		return;
	}

	public bool IsDamaged () {
		return damaged;
	}
}
