using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;



public class HunterRobot : MonoBehaviour
{
	[SerializeField] GameObject player;
	Vector2 spawnLocation;
	private float speed;
    [SerializeField] private float viewAngle = 90f;
	private Node playerNode;
	private bool isMoving;
	public List<Node> path = new List<Node>();
	public static bool disable;
	private bool hasPlayedDeathAnimation;
	private bool hasPlayedDeathAudio;
	public bool hunt;
	public bool playerPassedBarrier;
	private bool audioActivation;
	public Dictionary<Vector2Int, Node> gridNodes;
    AudioSource audioSource;
	private Node spawnLocationNode;
	Node spot;
    // Start is called before the first frame update
    void Start(){
		gridNodes = WalkableGrid.gridNodes;
        speed = 100;
		spawnLocation = this.transform.position;
        audioSource = GetComponent<AudioSource>();
		hasPlayedDeathAnimation = false;
		hasPlayedDeathAudio = false;
		audioActivation = true;
    }

    // Update is called once per frame
    void Update(){
		print(this.name + hunt);
		if(!disable){
			if (hunt || playerPassedBarrier){
				if(audioActivation && !(Time.deltaTime == 0.0)){
					PlayWalkingAudio();
				}
				// print("hunter moving to player location");
				MoveTo(GetNodeAtPosition(player.transform.position));
				if (audioSource != null && !audioSource.isPlaying && path.Count <= 25){
				}
				if (Vector2.Distance(this.transform.position, player.transform.position) <= 2.5f){
					// audio logic for teh hunter
					print(this.name);
					StopWalkingAudio();
					if(audioSource != null && !audioSource.isPlaying && !hasPlayedDeathAudio){
						// play death audio
						PlayerHunterDeathAudio();
						hasPlayedDeathAudio = true;
					}
					// play death animation
					if (!hasPlayedDeathAnimation) {
						print("player dead");
						PlayerHunterDeathAnimation();
						hasPlayedDeathAnimation = true;
						hunt = false;
					}
				}
			}
			else{
				if (!(Vector2.Distance(this.transform.position, spawnLocation) <= 1.5f)){
					StopWalkingAudio();
					MoveTo(GetNodeAtPosition(spawnLocation));
				}
			}
		}
    }

	private void PlayWalkingAudio(){
		if (audioSource != null && !audioSource.isPlaying && !SentinelRobot.puzzlePrompted){
			audioSource.PlayDelayed(0.5f);
		}
	}

	private void StopWalkingAudio(){
		if (audioSource != null && audioSource.isPlaying){
			audioSource.Stop();
		}
	}

	private void PlayerHunterDeathAudio(){
		AudioManager.instance?.Play("PlayerDeath");
	}

	private void PlayerHunterDeathAnimation(){
		player.gameObject.GetComponent<PlayerAnimation>().death();
	}

    public Node GetNodeAtPosition(Vector2 position) {
        return WalkableGrid.GetNodeFromWorldPosition(position);
    }

	public List<Node> AStar(Node start, Node target) {
		List<Node> openSet = new List<Node> { start };
		HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
		start.gCost = 0;
		start.hCost = GetDistance(start, target);

		while (openSet.Count > 0) {
			Node currentNode = openSet.OrderBy(node => node.fCost).ThenBy(node => node.hCost).First();

			if (currentNode == target) {
				return RetracePath(start, target);
			}

			openSet.Remove(currentNode);
			closedSet.Add(Vector2Int.RoundToInt(currentNode.position));

            foreach (var neighborPair in GetNeighbors(currentNode)) {
                Node neighbor = neighborPair.Value;
                Vector2Int neighborPos = neighborPair.Key;

                if (closedSet.Contains(neighborPos)) continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, target);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
						
                }
            }
		}

		return null; // Path not found
	}

	public Dictionary<Vector2Int, Node> GetNeighbors(Node node) {
		Dictionary<Vector2Int, Node> neighbors = new Dictionary<Vector2Int, Node>();
		Vector2Int[] directions = {
			new Vector2Int(0, 1),    // Up
			new Vector2Int(0, -1),   // Down
			new Vector2Int(1, 0),    // Right
			new Vector2Int(-1, 0),   // Left
			new Vector2Int(1, 1),    // Top right
			new Vector2Int(1, -1),   // Bottom right
			new Vector2Int(-1, 1),   // Top left
			new Vector2Int(-1, -1)   // Bottom left
		};

		Vector2Int nodePosition = Vector2Int.RoundToInt(node.position);
		foreach (var direction in directions) {
			Vector2Int neighborPosition = nodePosition + direction;
			if (gridNodes.TryGetValue(neighborPosition, out Node neighbor) && neighbor.isWalkable()) {
				neighbors.Add(neighborPosition, neighbor);
				// Debug.Log($"Neighbor at {neighborPosition} is walkable. Position: {neighbor.position}");
			}
		}
		
		return neighbors;
	}
    public int GetDistance(Node start, Node target){
        // get the distance between current location and target location
        float dstX = Mathf.Abs(start.getX() - target.getX());
        float dstY = Mathf.Abs(start.getY() - target.getY());
        // the lower the cost the higher the movement type priority
        int diagonalCost = 14;
        int orthogonalCost = 10;
        
		if (dstX > dstY){
			return (int)(diagonalCost *dstY + orthogonalCost*(dstX-dstY));
		}
		else{
			return (int)(diagonalCost *dstX + orthogonalCost*(dstY-dstX));
		}
    }


	public List<Node> RetracePath(Node startNode, Node endNode) {
		path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			currentNode.setTileColor(Color.blue);
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		return path;
	}
    public Node GetRandomNodeInArea(Dictionary<Vector2Int, Node> area) {
        List<Node> nodes = new List<Node>(area.Values);
        return nodes[UnityEngine.Random.Range(0, nodes.Count)];
    }

	public void MoveTo(Node target){
		// recalculate the path to the player
		// playerNode = GetNodeAtPosition(player.transform.position);
		// aiNode = GetNodeAtPosition(this.transform.position);
		if(!target.isWalkable()){
			target = GetRandomNodeInArea(GetNeighbors(target));
		}
		path = AStar(GetNodeAtPosition(this.transform.position), target);
		int targetIndex = 0;
		// Move the AI along the path
		if (path != null && targetIndex < path.Count) {
			// Access next Node using First
			Node nextNode = path.First();
			Vector2 dir = (nextNode.position - (Vector2)this.transform.position).normalized;
			// Vector2 directionToNextNode = (nextNode.position - (Vector2)this.transform.position).normalized;
			// Vector2 dir;

			// // Determine whether to move horizontally or vertically
			// if (Math.Abs(directionToNextNode.x) > Math.Abs(directionToNextNode.y)) {
			// 	// Move horizontally
			// 	dir = new Vector2(Math.Sign(directionToNextNode.x), 0);
			// } else {
			// 	// Move vertically
			// 	dir = new Vector2(0, Math.Sign(directionToNextNode.y));
			// }

			// Move the AI
			this.transform.Translate(dir * speed * Time.deltaTime / 10.0f);

			if (Vector2.Distance(this.transform.position, nextNode.position) <= 1.0f) {
				// Remove the visited node
				path.Remove(nextNode);
			}
		}
	}
}
