using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SentinelRobot : MonoBehaviour{
	
	public Dictionary<Vector2Int, Node> gridNodes;
	public List<Node> path = new List<Node>();
	[SerializeField] GameObject player;
	[SerializeField] GameObject hunterRobot;
	[SerializeField] private Transform[] waypoints;
	[SerializeField] public bool isGuard;
	[SerializeField] private GameObject bookShelfSpot;
	public bool moveToBook;
	private float speed;
	[SerializeField] private Transform visionPivot;
	private bool isMoving;
	public bool robotReprogrammed;
	private int currentWaypointIndex;
	Node spot;
    bool AlertedStateInState, RoamingStateInState, HuntingStateInState;
	float maxFearLevel = FearArea.maxFearAmount;
    public static bool puzzlePrompted = false; // tracks wether puzzle is generated and prompted, so no more than one puzzle is prompted at a time
    public bool puzzleTimeRanOut = false; // set to true if puzzle is not solved within time limit
    public bool puzzleSolved = false; // set to true if prompted puzzle is solved
	public GameObject book;
	public float chaseTime;
	public float disableTime;
	private SentinelAnimation sentinelAnimation;
	public bool killRobot;
	public bool disable;
	bool waitDone;
	public bool audioActivation;
	AudioSource audioSource;
	Vector2 spawnLocation;

    [NonSerialized] private float bookTimer;
    void Start() {
		spawnLocation = this.transform.position;
		gridNodes = WalkableGrid.gridNodes;
		isMoving = false;
		speed = 20.0f;
        AlertedStateInState = false;
        HuntingStateInState = false;
        RoamingStateInState = false;
		audioActivation = false;
		currentWaypointIndex = 0;
		moveToBook = false;
		robotReprogrammed = false;
		killRobot = false;
		waitDone = false;
		disable = true;
		audioSource = GetComponent<AudioSource>();
		chaseTime = 0.0f;
		sentinelAnimation = this.GetComponent<SentinelAnimation>();
        bookTimer = 0.0f;

    }
	void Update(){
		
		if (disable){
            // if robot disabled due to time
            // SINA: i added your snippet of code for visionPivotTransform here as well but to enable it when robot no longer stunned 
            Transform visionPivotTransform = this.gameObject.transform.Find("VisionPivot");
            if (disableTime > 0.0f){
				disableTime -=Time.deltaTime;
			}
			else if (disableTime < 0.0f){
				reactivateRobot(visionPivotTransform);
				//FindObjectOfType<AudioManager>().Play("RobotActivate");
			}
		}
		float fearLevel = FearArea.currentFearAmount / maxFearLevel;
        // playerNode = WalkableGrid.GetNodeFromWorldPosition(player.transform.position);
		// PLEASE NOTE i would use a switch statement to avoid all these "if" statements but
		// C# switch does not support boolean expressions for some reason
		if(killRobot){
			this.gameObject.GetComponent<SentinelAnimation>().Death();
			HunterRobot.disable = true;
			disable = true;
			disableRobot();
			killRobot = false;
		}
		if(robotReprogrammed){
			disableRobot();
			// last puzzle solved, move to book shelf
			
			MoveTo(GetNodeAtPosition(bookShelfSpot.transform.position));
			if(Vector2.Distance(this.transform.position, bookShelfSpot.transform.position) <= 1.5f){
				this.gameObject.GetComponent<SentinelAnimation>().UnshelfBook();
			}		

		}
		if(!disable){
			if(audioActivation && !(Time.deltaTime == 0.0)){
				PlayWalkingAudio();
			}
			
			if(disableTime <= 0.0f){


				
				if(chaseTime < 0.0f){
					chaseTime = 0.0f;
				}

				if(RobotVision.CanSeePlayer && !isGuard){
					speed = 30.0f;
					chaseTime = 7.0f;
					fearLevel = 0.80f;
		
				}
				if(moveToBook){
					
					// if no book either go back to guard or wait based on waitDone
					if(book == null){
						// if done waiting, go back to guard
						if(waitDone){
							if(!(Vector2.Distance(this.transform.position, spawnLocation) <= 1.0f)){
                                // move to guard position
                                MoveTo(GetNodeAtPosition(spawnLocation));
							}
							else
							{
								// at guard position, can stop this event
								moveToBook = false;
								waitDone = false;
								// stop the book animation and go back to idle animation
								//sentinelAnimation.UnshelfBook();
                                sentinelAnimation.BackToIdle();
								// NOW THAT AT GUARD POSITION, LOOK UP SO VISION PIVOT IS ALSO FACING UP
								MoveTo(GetNodeAtPosition(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1.6f)));
                            }
						}
						// wait ~10 seconds before waitDone is set to true so robot goes back to guarding
						else{

                            bookTimer += Time.deltaTime;
                            if (bookTimer >= 10.0f)
                            {
                                waitDone = true;
                                bookTimer = 0f;
                            }
                        }
					}
					else if (!(Vector2.Distance(this.transform.position, book.transform.position) <= 1.5f)){
						MoveTo(GetNodeAtPosition(book.transform.position));
					}
					else{
						
						FindObjectOfType<AudioManager>().Play("RobotMovingToBook");
						sentinelAnimation.PickUpBook();

						Destroy(book);
						book = null;
					}
				}
				else if(!isGuard){
					speed = 20.0f;
					if (fearLevel < 0.33f && chaseTime <= 0.0f){
						if (AlertedStateInState){

							// call code when exiting alerted state
							AlertedStateInState = false;
						}

						if (RoamingStateInState){
							if(!isMoving){
								isMoving = true;
							}
							
							Node nextWaypointNode = GetNodeFromWaypoint(waypoints[currentWaypointIndex]);
							MoveTo(nextWaypointNode);
							if (Vector2.Distance(this.transform.position, nextWaypointNode.position) <= 1.5f){
								if(currentWaypointIndex == waypoints.Length-1){
									currentWaypointIndex = 0;
								}
								else{
									currentWaypointIndex+=1;
									isMoving = false;
								}
							}
							
						}

						if (!RoamingStateInState)
						{
							// call code when entering alerted state
							isMoving = false;
							RoamingStateInState = true;
						}
					}
					else if (0.33f < fearLevel && fearLevel < 0.66f && chaseTime <= 0.0f){
						if (HuntingStateInState){
							// call code when exiting hunting state
							
							HuntingStateInState = false;
						}
						if (RoamingStateInState){
							// call code when exiting roaming state
							
							RoamingStateInState = false;
						}
						if (AlertedStateInState){
							// call code when in alerted state
							
							if (!isMoving) {
								isMoving = true;

							}
							else if(!(Vector2.Distance(this.transform.position, spot.position) <= 1.0f)){
								
								MoveTo(spot);
							}
							else{
								
								isMoving = false;
								spot = GetRandomNodeInArea(GetNeighbors(spot));
							}
						}
						if (!AlertedStateInState){
							// call code when entering alerted state
							
							isMoving = false;
							AlertedStateInState = true;
							speed = 25.0f;
							spot = GetRandomNodeInArea(GetNeighbors(GetNodeAtPosition(player.transform.position)));
						}
					}
						// IMPLEMENT LATER: need to also check if noise was detected
					else if (fearLevel >= 0.66f || chaseTime > 0.0f){
						if (HuntingStateInState){
							// call code when in hunting state
							speed = 40.0f;
							if(chaseTime > 0.0f){
								chaseTime -= Time.deltaTime;
							}
							
							
							if (!(Vector2.Distance(this.transform.position, player.transform.position) <= 1.5f)){
								MoveTo(GetNodeAtPosition(player.transform.position));
							}
							else{
								// got to the player, do shit here

								// prompt puzzle
								if (!puzzlePrompted)
								{
									//print("fuk shit fuk");
									puzzlePrompted = true;
									LevelPuzzleManager.puzzleManager.GenerateNewColourPuzzle(gameObject);

								}
							}
						}

						else if (!HuntingStateInState){
							// call code when entering hunting state
							
							
							isMoving = false;
							HuntingStateInState = true;
							AlertedStateInState = false;
							RoamingStateInState = false;
						}
					}	
				}
			}

		}
	}

	// not used anymore
	IEnumerator<UnityEngine.WaitForSeconds> WaitToGoBack(){
        yield return new WaitForSeconds(5f);
        //yield return new WaitForFixedUpdate();
        //moveToBook = false;
        waitDone = true;
    }
	private void PlayWalkingAudio(){
		if (isMoving && audioSource != null && !audioSource.isPlaying){
			audioSource.PlayDelayed(0.5f);
			audioSource.volume = Vector2.Distance(this.transform.position, player.transform.position)/2;
		}
		if (!isMoving && audioSource != null && audioSource.isPlaying){
			audioSource.Stop();
		}
	}

	private void disableRobot(){
		disable = true;
		Transform visionPivotTransform = this.gameObject.transform.Find("VisionPivot");
		Transform fearArea = this.gameObject.transform.Find("FearArea");
		if (visionPivotTransform != null){
			visionPivotTransform.gameObject.SetActive(false);
		}
		if(fearArea != null){
			fearArea.gameObject.SetActive(false);
		}
	}

	private void reactivateRobot(Transform visionPivotTransform)
    {
        FindObjectOfType<AudioManager>().Play("RobotActivate");
        disable = false;
        disableTime = 0.0f;
        chaseTime = 0.0f;
        visionPivotTransform = this.gameObject.transform.Find("VisionPivot");
        Transform fearArea = this.gameObject.transform.Find("FearArea");
        if (visionPivotTransform != null)
        {
            visionPivotTransform.gameObject.SetActive(true);
        }
        if (fearArea != null)
        {
            fearArea.gameObject.SetActive(true);
        }
    }
	
    public Node GetRandomNodeInArea(Dictionary<Vector2Int, Node> area) {
        List<Node> nodes = new List<Node>(area.Values);
        return nodes[UnityEngine.Random.Range(0, nodes.Count)];
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
			if (WalkableGrid.gridNodes.TryGetValue(neighborPosition, out Node neighbor) && neighbor.isWalkable()) {
				neighbors.Add(neighborPosition, neighbor);
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
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		return path;
	}

	public void RoamAndSearch(Node start, Node target) {
		if (!isMoving) {
			isMoving = true;
			spot = GetRandomNodeInArea(WalkableGrid.gridNodes);
		} else if (GetNodeAtPosition(this.transform.position) != spot) {
			MoveTo(spot);
		} else {
			isMoving = false;
		}
	}


	public Dictionary<Vector2Int, Node> Area(Node target) {
		Dictionary<Vector2Int, Node> areaToWalk = new Dictionary<Vector2Int, Node>();

		int searchRadius = 3; // 3 units in each direction for simplicity
		Vector2Int targetPos = Vector2Int.RoundToInt(target.position);

		for (int x = -searchRadius; x <= searchRadius; x++) {
			for (int y = -searchRadius; y <= searchRadius; y++) {
				Vector2Int searchPos = new Vector2Int(targetPos.x + x, targetPos.y + y);
				if (WalkableGrid.gridNodes.TryGetValue(searchPos, out Node node) && node.isWalkable()) {
					areaToWalk.Add(searchPos, node);
				}
			}
		}

		return areaToWalk;
	}


	public void MoveTo(Node target){
		if(!target.isWalkable()){
			target = GetRandomNodeInArea(GetNeighbors(target));
		}
		path = AStar(GetNodeAtPosition(this.transform.position), target);
		int targetIndex = 0;
		//move the AI along the path
		if (path != null && targetIndex < path.Count) {
        	//access next Node using First
			Node nextNode = path.First();
			if(nextNode == null){
				path.Remove(nextNode);
				nextNode = path.First();
			}
			Vector2 dir = (nextNode.position - (Vector2)this.transform.position).normalized;
			this.transform.Translate(dir * speed * Time.deltaTime/10.0f);
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			visionPivot.rotation = Quaternion.Euler(0, 0, angle);

			if (Vector2.Distance(this.transform.position, nextNode.position) <= 1.0f)
			{
				//remove the visited node
				path.Remove(nextNode);
				
            }

        }
	}

	private Node GetNodeFromWaypoint(Transform waypoint){
		return GetNodeAtPosition(waypoint.position);
	}

	/*
	 * SINA
	 * This method is called from ColourPuzzle.cs when a prompted puzzle is successfully solved or if time runs out
	 */
    public void puzzleCompleted(){
		// puzzle solved? stun robot for 10 seconds
        if (puzzleSolved){
			print(this.name +" puzzle solved");
			this.disableTime = 7.5f;
			disable = true;
			FindObjectOfType<AudioManager>().PlayDelayed("RobotDisabled", 2f);
			disableRobot();

            Transform visionPivotTransform = this.gameObject.transform.Find("VisionPivot");

            if (visionPivotTransform != null)
            {
                visionPivotTransform.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("VisionPivot not found!");
            }

			puzzlePrompted = false;
			puzzleSolved = false;
			puzzleTimeRanOut = false;
        }
		// time ran out? DO X HERE
		else if (puzzleTimeRanOut)
		{
            Transform visionPivotTransform = this.gameObject.transform.Find("VisionPivot");

            if (visionPivotTransform != null)
            {
                visionPivotTransform.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("VisionPivot not found!");
            }

            puzzlePrompted = false;
            puzzleSolved = false;
            puzzleTimeRanOut = false;
        }
    }
}
