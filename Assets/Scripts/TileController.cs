using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

	public static GameControl MainControlScript;
	public TileController m_northTile;
	public TileController m_eastTile;
	public TileController m_southTile;
	public TileController m_westTile;
	public Vector2 m_position;
	public Animator m_playerAnim;
	public float DoorChance = 0.5f; //chance to spawn a door for any given tile at the end of the generation sequence (when n=0)
	public float HealChance = 0.1f; //chance any tile spawns a healing enemy
	public float ReplaceChance = 0.2f; //chance any tile spawns a replace enemy
	public int BigN; //generation n that this tile was given

	private SpriteRenderer spriteR;
	private GameObject letterPart;
	private GameObject playerPart;
	private GameObject enemyPart;
	private Animator enemyAnim;
	private int letter;
	private string letterS;

	public const int ENEMY_NONE = 0;
	public const int ENEMY_DOOR = 1;
	public const int ENEMY_HEAL = 2;
	public const int ENEMY_REPLACE = 3;
	private int enemyLevel;
	private int enemyType;

	public void Initialize (Vector2 position, int n) {
		BigN = n;
		MainControlScript = GameObject.Find ("GameController").GetComponent<GameControl> ();

		transform.position = new Vector3 (position.x, position.y);
		m_position = position;
		gameObject.name = "Tile" + position.ToString ();

		letterPart = transform.Find ("Letter").gameObject;
		playerPart = transform.Find ("PlayerPart").gameObject;
		enemyPart = transform.Find ("Enemy").gameObject;
		spriteR = gameObject.GetComponent<SpriteRenderer> ();
		m_playerAnim = playerPart.GetComponent<Animator> ();
		enemyAnim = enemyPart.GetComponent<Animator> ();
		enemyType = ENEMY_NONE;

		letter = MainControlScript.AssignLetter (position);
		letterPart.GetComponent<SpriteRenderer> ().sprite = MainControlScript.LetterSprites [letter];
		letterS = ((char)(letter + "a"[0])).ToString();
		//Debug.Log (letterS);
			
		m_northTile = null;
     	m_eastTile = null;
		m_southTile = null;
		m_westTile = null;
		CheckSurroundings ();

		if (n > 0)
			MakeBranches (n);
	}
	
	// Update is called once per frame
	void Update () {
		PlayerControl ();
		CheckSurroundings ();
	}


	//check if there are any new neighboring tiles, and update this one appropriately 
	void CheckSurroundings () {
		bool change = false;
		if (m_northTile == null) {
			m_northTile = MainControlScript.GetTileAtPosition (new Vector2 (m_position.x, m_position.y + 1));
			if (m_northTile != null)
				change = true;
		}
		if (m_eastTile == null) {
			m_eastTile = MainControlScript.GetTileAtPosition (new Vector2 (m_position.x + 1, m_position.y));
			if (m_eastTile != null)
				change = true;
		}
		if (m_southTile == null) {
			m_southTile = MainControlScript.GetTileAtPosition (new Vector2 (m_position.x, m_position.y - 1));
			if (m_southTile != null)
				change = true;
		}
		if (m_westTile == null) {
			m_westTile = MainControlScript.GetTileAtPosition (new Vector2 (m_position.x - 1, m_position.y));
			if (m_westTile != null)
				change = true;
		}
		if (change) {
			spriteR.sprite = MainControlScript.GetTileSpriteByDirection (m_northTile != null, m_eastTile != null, m_southTile != null, m_westTile != null);
		}
			
	}

	//generate branching structure to level
	void MakeBranches (int n) {
		//Debug.Log ("here");
		//first, make a list of the position of every open neighboring tile
		CheckSurroundings (); //probably unnecessary, but calling this to be 
		List<Vector2> neighbors = new List<Vector2> ();
		if (m_northTile == null)
			neighbors.Add (new Vector2(m_position.x, m_position.y+1));
		if (m_eastTile == null)
			neighbors.Add (new Vector2(m_position.x+1, m_position.y));
		if (m_southTile == null)
			neighbors.Add (new Vector2(m_position.x, m_position.y-1));
		if (m_westTile == null)
			neighbors.Add (new Vector2(m_position.x-1, m_position.y));
		
		//generate n/2 neighboring tiles
		//first, reduce the list to at most n/2 spots
		while (neighbors.Count > n/2) {
			int discard = Random.Range (0, neighbors.Count - 1);
			neighbors.RemoveAt (discard);
		}
		neighbors = Shuffle (neighbors);
		//then make a new tile for each spot that's still open
		foreach (Vector2 pos in neighbors) {
			//first check to see if one of your child branches already took this spot
			if (MainControlScript.GetTileAtPosition(pos) == null)
				MainControlScript.CreateTile (pos, n - 1);
		}

		if (n < MainControlScript.GenerationSeed) //so you can't make an enemy on the home tile
			SpawnEnemy (n, false);
	}

	//fisher-yates shuffle a list of Vector2s. one-off utility function for MakeBranches
	List<Vector2> Shuffle(List<Vector2> unshuffled) {
		int j;
		Vector2 temp;
		List<Vector2> a = new List<Vector2> (unshuffled);
		for (int i = a.Count - 1; i > 1; i--) {
			j = Random.Range (0, i);
			temp = a [j];
			a [j] = a [i];
			a [i] = temp;
		}
		return a;
	}

	void PlayerControl () {
		if (MainControlScript.IsDestroyed())
			return;
		if (Input.GetKeyDown (letterS)) {
			//when the player initially occupies a tile
			//playerAnim.SetBool ("exists", true);
			MainControlScript.AddToPlayer (m_position);
			if (enemyType != ENEMY_NONE) {
				if (MainControlScript.PlayerLength () <= enemyLevel)
					MainControlScript.Damage (false);
				else
					GiveBenefits (enemyType);
				DestroyEnemy ();
			}
		} else if (Input.GetKey(letterS)) {
			//while the player holds down on a tile
		} else if (Input.GetKeyUp(letterS)) {
			//when the player releases from a tile
			m_playerAnim.SetBool ("exists", false);
			MainControlScript.Damage (true);
		}
	}

	void DestroyEnemy () {
		enemyType = ENEMY_NONE;
		enemyAnim.SetInteger ("type", enemyType);
		enemyAnim.SetTrigger ("dead");
	}

	//given generation n and knowing what the MainControlScript does, generate and animate an enemy
	public void SpawnEnemy (int n, bool isDoor) {
		bool chosen = false;
		float rand = Random.Range (0.0f, 1.0f);

		if (isDoor || (!chosen && n == 1 && DoorChance > rand)) {
			enemyType = ENEMY_DOOR;
			chosen = true;
			MainControlScript.ExitExists = true;
		}

		rand = Random.Range (0.0f, 1.0f);
		if (!chosen && HealChance > rand) {
			enemyType = ENEMY_HEAL;
			chosen = true;
		}

		rand = Random.Range (0.0f, 1.0f);
		if (!chosen && ReplaceChance > rand) {
			enemyType = ENEMY_REPLACE;
			chosen = true;
		}

		if (chosen) {
			int weight = Random.Range (MainControlScript.GenerationSeed - n - 2, MainControlScript.GenerationSeed - n);
			if (weight < 3)
				enemyLevel = 3;
			else if (weight > 5)
				enemyLevel = 5;
			else
				enemyLevel = weight;
				
			//Debug.Log (enemyType + " " + enemyLevel);
			enemyAnim.SetInteger ("type", enemyType);
			enemyAnim.SetInteger ("level", enemyLevel);
		}
	}

	public void GiveBenefits (int type){}
}
