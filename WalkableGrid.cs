using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkableGrid : MonoBehaviour
{
    public static Dictionary<Vector2Int, Node> gridNodes = new Dictionary<Vector2Int, Node>();
    [SerializeField] private Tile background_prefab;
    [SerializeField] private Tilemap tilemap;

    void Start(){
        GenerateGrid();
		SetWalkableArea();
    }

	public void GenerateGrid(){
        BoundsInt bounds = tilemap.cellBounds;

		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int localPlace = new Vector3Int(x, y, (int)tilemap.transform.position.z);
				Vector3 worldPosition = tilemap.CellToWorld(localPlace);

                if (tilemap.HasTile(localPlace)){
					var spawnedTile = Instantiate(background_prefab, worldPosition, UnityEngine.Quaternion.identity);
					spawnedTile.name = "Tile:"+x+","+y;
					Node node = new Node(spawnedTile.transform.position, true, spawnedTile);
					if (!gridNodes.ContainsKey(new Vector2Int((int)node.getX(), (int)node.getY())))
					{
                        gridNodes.Add(new Vector2Int((int)node.getX(), (int)node.getY()), node);
                    }
					else
					{
						gridNodes.Remove(new Vector2Int((int)node.getX(), (int)node.getY()));
                        gridNodes.Add(new Vector2Int((int)node.getX(), (int)node.getY()), node);
                    }
                }
			}
		}
	}

	public void SetWalkableArea()
	{
		LayerMask obstacleLayer = LayerMask.GetMask("Obstacles");
		foreach (KeyValuePair<Vector2Int, Node> entry in gridNodes){
			Vector2 worldPosition = new Vector2(entry.Value.getX(), entry.Value.getY());
			Vector2 boxSize = new Vector2(1, 1);

			Collider2D[] hits = Physics2D.OverlapBoxAll(worldPosition, boxSize, 0, obstacleLayer);

			bool isWalkable = true;

			foreach (var hit in hits){
				if (hit != null && hit.tag != "Player" && hit.tag != "Enemy"){
					isWalkable = false;
					break;
				}
			}

			if (!isWalkable){
				entry.Value.setTileColor(Color.red);
				entry.Value.setWalkable(false);
			}
		}
	}
	


    public static Node GetNodeFromWorldPosition(Vector2 worldPosition){
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y);
        Vector2Int gridPosition = new Vector2Int(x, y);

        if (gridNodes.TryGetValue(gridPosition, out Node node)){
            return node;
        }
        return null;
    }
}
