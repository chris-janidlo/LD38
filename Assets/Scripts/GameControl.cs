using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameControl : MonoBehaviour {

	public Dictionary<Vector2, TileController> tiles; //map positions to tiles
	public Texture2D TileAtlas;
	public Texture2D LetterAtlas;
	public Texture2D MiscAtlas;
	public Sprite[] TileSprites;
	public Sprite[] LetterSprites;
	public Sprite[] MiscSprites;
	public List<int> CurrentLetters;
	public int GenerationSeed = 8;
	public Vector2 PlayerEndOnePos;
	public Vector2 PlayerEndTwoPos;
	public Vector2 StartPos;
	public Vector2 Null = new Vector2 (10000, 10000); //some dummy value we don't expect to run in to when testing for nullity
	public float ShakeTime = 0.5f; //how long to screen shake
	public float ShakeAmount = 1.0f; //how far away from center the screen will shake at max
	public float CameraZ = -10.0f;
	public float IFrameTime = 1.0f; //time after taking damage when you are invincible
	[HideInInspector] public int Exits;
	[HideInInspector] public int Releases;
	public int MaxHearts = 3;
	public int StartingReleases = 3;
	public int MaxReleases = 9;

	private GameObject TilePrefab;
	private Camera cam;
	private SpriteRenderer replaceRenderer;
	public List<Vector2> TilesWithPlayer;
	private float shakeCount = 0.0f;
	private Vector3 center; //where the camera was most recently centered
	private bool centered; //whether or not the camera is already centered
	private bool destroyed; //in a damaged state where you need to release all buttons before pressing any again
	private float heartCount; //can only be integer or half integer
	private SpriteRenderer heart1;
	private SpriteRenderer heart2;
	private SpriteRenderer heart3;
    private bool gameOver;

    void Start() { Init (); }

	void Init () {
        gameOver = false;
		Releases = StartingReleases;
		heartCount = MaxHearts;
		cam = gameObject.GetComponentInChildren<Camera> ();

		TileSprites = Resources.LoadAll<Sprite> (TileAtlas.name);
		LetterSprites = Resources.LoadAll<Sprite> (LetterAtlas.name);
		MiscSprites = Resources.LoadAll<Sprite> (MiscAtlas.name);
		TilePrefab = (GameObject) Resources.Load ("Tile");
		replaceRenderer = transform.Find ("UI/ReplaceComponent/Number").gameObject.GetComponent<SpriteRenderer> ();
		heart1 = transform.Find ("UI/LifeComponent/Heart1").gameObject.GetComponent<SpriteRenderer> ();
		heart2 = transform.Find ("UI/LifeComponent/Heart2").gameObject.GetComponent<SpriteRenderer> ();
		heart3 = transform.Find ("UI/LifeComponent/Heart3").gameObject.GetComponent<SpriteRenderer> ();

		MakeWorld (Vector2.zero);
	}

	void MakeWorld (Vector2 pos) {
		Exits = 0;
		if (Releases < StartingReleases)
			Releases = StartingReleases;
		IEnumerable<int> enumerable = Enumerable.Range(0, 25);
		CurrentLetters = enumerable.ToList();
		tiles = new Dictionary<Vector2, TileController> ();

		CreateTile (pos, GenerationSeed);
		StartPos = pos;
		if (Exits == 0)
			SpawnExit ();
	
		CenterCamera ();

		InitializePlayer ();
	}

	void DestroyWorld () {
		List<TileController> toDestroy = new List<TileController> (tiles.Values);
		foreach (TileController t in toDestroy) {
			Destroy (t.gameObject);
		}
	}

	public void InitializePlayer () {
        destroyed = true;
        foreach (Vector2 pos in TilesWithPlayer) {
            TileController t = GetTileAtPosition(pos);
            if (t != null)
            t.m_playerAnim.SetBool("exists", false);
        }
        destroyed = true; //so that the player isn't damaged when they release the keys after spawning a new world
        PlayerEndOnePos = Null;
		PlayerEndTwoPos = Null;
		TilesWithPlayer = new List<Vector2> ();
	}

	void AnimateHearts () {
		if (heartCount == 3.0f) {
			heart3.sprite = MiscSprites [5];
			heart2.sprite = MiscSprites [5];
			heart1.sprite = MiscSprites [5];
		}
		if (heartCount == 2.5f) {
			heart3.sprite = MiscSprites [6];
			heart2.sprite = MiscSprites [5];
			heart1.sprite = MiscSprites [5];
		}
		if (heartCount == 2.0f) {
			heart3.sprite = MiscSprites [7];
			heart2.sprite = MiscSprites [5];
			heart1.sprite = MiscSprites [5];
		}
		if (heartCount == 1.5f) {
			heart3.sprite = MiscSprites [7];
			heart2.sprite = MiscSprites [6];
			heart1.sprite = MiscSprites [5];
		}

		if (heartCount == 1.0f) {
			heart3.sprite = MiscSprites [7];
			heart2.sprite = MiscSprites [7];
			heart1.sprite = MiscSprites [5];
		}

		if (heartCount == 0.5f) {
			heart3.sprite = MiscSprites [7];
			heart2.sprite = MiscSprites [7];
			heart1.sprite = MiscSprites [6];
		}

		if (heartCount == 0.0f) {
			heart3.sprite = MiscSprites [7];
			heart2.sprite = MiscSprites [7];
			heart1.sprite = MiscSprites [7];
		}
	}

	void FixedUpdate () {
        if (gameOver) {
            if (Input.GetKey("space")) {
                cam.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                Init ();
            }
            return;
        }
        if (heartCount <= 0.0f && shakeCount <= 0.0f) {
            DestroyWorld();
            cam.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            gameOver = true;
        }

		AnimateHearts ();
		if (Releases < 0)
			Releases = 0; 
		if (Releases > MaxReleases)
			Releases = MaxReleases;
		replaceRenderer.sprite = MiscSprites [Releases + 9];
		if (shakeCount > 0) {
			centered = false;
			transform.position = new Vector3 (center.x + Random.Range (-1 * ShakeAmount * shakeCount, ShakeAmount * shakeCount), center.y + Random.Range (-1 * ShakeAmount * shakeCount, ShakeAmount * shakeCount), CameraZ);
			shakeCount -= Time.deltaTime;
		} 
		if (!centered && shakeCount <= 0) {
			CenterCamera ();
		}
		if (destroyed && !(Input.anyKey))
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
		destroyed = false;
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
		transform.position = new Vector3 (x / count, y / count, CameraZ);
		center = transform.position;
		cam.transform.position = center;
		centered = true;
	}

	//apply damage and clear the player
	public void Damage (bool clearSnake) {
		if (destroyed)
			return;
		shakeCount = ShakeTime;
		heartCount -= 0.5f;
		Debug.Log (heartCount);
		if (clearSnake) {
			InitializePlayer ();
		}
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
		//Debug.Log (endOne + " " + endTwo);
		if (!ValidPlayerPosition (pos, endOne, endTwo)) {
			Damage (true);
			GetTileAtPosition (pos).m_playerAnim.SetBool ("exists", false);
			return;
		}
		GetTileAtPosition (pos).m_playerAnim.SetBool ("exists", true);
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
				tileOne.m_playerAnim.SetTrigger ("north");
				newTile.m_playerAnim.SetTrigger ("south");
			} else if (pos.y == PlayerEndOnePos.y - 1) { //new player is south
				tileOne.m_playerAnim.SetTrigger ("south");
				newTile.m_playerAnim.SetTrigger ("north");
			} else if (pos.x == PlayerEndOnePos.x + 1) { //new player is east
				tileOne.m_playerAnim.SetTrigger ("east");
				newTile.m_playerAnim.SetTrigger ("west");
			} else {//if (pos.x == PlayerEndOnePos.x - 1) new player is west
				tileOne.m_playerAnim.SetTrigger ("west");
				newTile.m_playerAnim.SetTrigger ("east");
			}
			if (!dontSetTwo) PlayerEndOnePos = pos;
		} else { //if (endTwo)
			if (pos.y == PlayerEndTwoPos.y + 1) { //new player is north
				tileTwo.m_playerAnim.SetTrigger ("north");
				newTile.m_playerAnim.SetTrigger ("south");
			} else if (pos.y == PlayerEndTwoPos.y - 1) { //new player is south
				tileTwo.m_playerAnim.SetTrigger ("south");
				newTile.m_playerAnim.SetTrigger ("north");
			} else if (pos.x == PlayerEndTwoPos.x + 1) { //new player is east
				tileTwo.m_playerAnim.SetTrigger ("east");
				newTile.m_playerAnim.SetTrigger ("west");
			} else {//if (pos.x == PlayerEndTwoPos.x - 1) new player is west
				tileTwo.m_playerAnim.SetTrigger ("west");
				newTile.m_playerAnim.SetTrigger ("east");
			}
			PlayerEndTwoPos = pos;
		}
		TilesWithPlayer.Add (pos);
		//Debug.Log (PlayerEndOnePos);
		//Debug.Log (PlayerEndTwoPos);
		return;
	}

	public bool IsDestroyed () {
		return destroyed;
	}

	public int PlayerLength () {
		return TilesWithPlayer.Count ();
	}

	void SpawnExit () {
		Dictionary<Vector2, TileController> grabBag = new Dictionary<Vector2, TileController>(tiles);
		List<Vector2> posers = new List<Vector2> (grabBag.Keys);
		grabBag.Remove(StartPos);

		TileController choice = grabBag [posers [Random.Range (0, posers.Count - 1)]];
		//while (choice.BigN > 2)
		//	choice = grabBag [posers [Random.Range (0, posers.Count - 1)]];

		choice.SpawnEnemy (choice.BigN, true);
		Exits++;
	}

	public void GiveBenefits (int enemyType, Vector2 enemyPos) {
		Debug.Log ("benefits" + enemyType);
		
		if (enemyType == 1) {
			DestroyWorld ();
			MakeWorld (enemyPos);
		}
			
		if (enemyType == 2) {
			if (heartCount < MaxHearts)
				heartCount += 0.5f;
			Debug.Log (heartCount);
		}
		
		if (enemyType == 3)
			Releases++;
			

	}
}
