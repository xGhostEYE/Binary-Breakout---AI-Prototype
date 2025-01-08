
# BinaryBreakout-AI-Prototype

Binary Breakout is a Game about a student who gets locked in the University library and must escape. Unbenounced to them, the helpful robots that used to aid students in tasks are now out to kill them!

This game was made with a team of students for my cmpt 406 Game Design class. I was in charge of implementing and building the AI for the Sentry and Hunter robots. The code within this repo are just the main parts of what makes the AI tick. If you would like to play our game (15min) then follow the link here to play it in the browser.
- bit.ly/binarybreakout



## How the AI works (The Robots)

- Sentinel AI: This AI does not harm the player, it patrols a set path and has a vision FOV that when the player is spotted (overlaps with the FOV) the robot will chase the player. If the player is caught by the robot, it will prompt them to answer a puzzle correctly (they have 3 tries to get it right). Each failed puzzle increases the player’s fear level. This will alert the Hunter to the player’s location after a threshold is passed. If all attempts are answered incorrectly, the player will be killed automatically by the hunter robot. If the player passes the puzzle, then the Sentinel robot will be deactivated temporarily then return to its patrol.

  ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/sentinel%20robot.PNG?raw=true)
  
- Hunter AI: This AI is only triggered when the player fear level passes a certain threshold, then it will rush very quickly to the player’s current location until they kill the player, or the player’s fear level decreases enough, then it simply returns to its charging location.
  
  ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/hunter%20robot.PNG?raw=true)
  
- Player (not an AI but affects them): The player has two circles surrounding them. The inner circle (RED) is used for the Fear counter. The longer an AI is within this circle, the higher the fear counter increases. Once it gets too high, the Hunter robot and nearest Sentinel are alerted to the player’s position and head there. The outer circle (BLUE) is used for AI optimization, meaning that only AI that are within this circle will be active, otherwise they are deactivated and save CPU resources. This occurs off screen to not break imersion (the rectangle around the player is their view). 

  ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/Player%20Rings.jpg?raw=true)

## Path Finding (The Grid)

- All AI use A* pathfinding logic with a dynamically generated grid. This grid merely needs to be attached to any object in the scene, and whatever 2D sprites are placed on the scene will have a grid block spawned at the same location. Sprites that are not meant to be walked on just need to toggle a variable, and the grid automatically sets the block to be un walkable. If it’s not walkable, then the AI will not be able to go on it and adjust its path calculations accordingly.

- AI tools built into unity were not used because I wanted to learn from scratch how to build a game AI, as I have always wanted to make one. The grid was used to help assist in teaching me how to make and use A* path finding, instead of using default X,Y coordinates in unity.

## AI States
- Sentinel States:
    - Patrol: Walk from one node to the next and repeat this process
    ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/AI%20Pathing.jpg?raw=true)
    - Alerted: Player fear has increased and the robot goes to the player location. Or the player has failed a puzzle, and the nearest robot goes to them.
    - Chasing: Robot sees the player and is chasing after them. Hunter robot will be activated here as well, as long as the Sentinel has the player within its vision FOV.
    
    ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/spotted.png?raw=true)

    - Deactivated: The robot has been deactivated either due to story reasons, or the player passed all puzzles given to them by the robot.

- Hunter States:
    - Hunting: Sentinel has been given the player location either due to the player fear being too high, or a Sentinel sees them. The Hunter will rush to the player in this state.
    
    ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/chasing.jpg?raw=true)
  
    - Standby: Hunter is stationary at its charge point and does nothing. Or if it lost the player location, will return to its charging station.
