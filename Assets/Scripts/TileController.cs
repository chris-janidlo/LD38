using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

	public static GameControl MainControlScript;
	public Vector2 position;
	public TileController m_northTile;
	public TileController m_eastTile;
	public TileController m_southTile;
	public TileController m_westTile;

	private SpriteRenderer spriteR;

	public void Initialize (Vector2 position, int n) {
		MainControlScript = GameObject.Find ("GameController").GetComponent<GameControl> ();
		this.position = position;
		transform.position = new Vector3 (position.x, position.y);
		spriteR = gameObject.GetComponent<SpriteRenderer> ();
		m_northTile = null;
     	m_eastTile = null;
		m_southTile = null;
		m_westTile = null;
		CheckSurroundings ();
		if (n > 0)
			BranchingGrowth ();
	}
	
	// Update is called once per frame
	void Update () {
		CheckSurroundings ();
	}


	//check if there are any new neighboring tiles, and update this one appropriately 
	void CheckSurroundings () {
		bool change = false;
		if (m_northTile == null) {
			m_northTile = MainControlScript.GetTileAtPosition (new Vector2 (position.x, position.y + 1));
			if (m_northTile != null)
				change = true;
		}
		if (m_eastTile == null) {
			m_eastTile = MainControlScript.GetTileAtPosition (new Vector2 (position.x + 1, position.y));
			if (m_eastTile != null)
				change = true;
		}
		if (m_southTile == null) {
			m_southTile = MainControlScript.GetTileAtPosition (new Vector2 (position.x, position.y - 1));
			if (m_southTile != null)
				change = true;
		}
		if (m_westTile == null) {
			m_westTile = MainControlScript.GetTileAtPosition (new Vector2 (position.x - 1, position.y));
			if (m_westTile != null)
				change = true;
		}
		if (change) {
			spriteR.sprite = MainControlScript.GetTileSpriteByDirection (m_northTile != null, m_eastTile != null, m_southTile != null, m_westTile != null);
		}
			
	}

	void BranchingGrowth (int n) {

	}

}
