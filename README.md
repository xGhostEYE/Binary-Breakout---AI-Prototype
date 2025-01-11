
# BinaryBreakout-AI-Prototype

Binary Breakout is a Game about a student who gets locked in the University library and must escape. Unbenounced to them, the helpful robots that used to aid students in tasks are now out to kill them!

This game was made with a team of students for my cmpt 406 Game Design class. I was in charge of implementing and building the AI for the Sentry and Hunter robots. The code within this repo are just the main parts of what makes the AI tick. If you would like to play our game (15min) then follow the link here to play it in the browser.

https://xghosteye.itch.io/binary-breakout



## How the AI works (The Robots)

### Sentinel AI 

This AI does not harm the player but instead it patrols a set path and has a vision FOV that when the player is spotted (overlaps with the FOV) the robot will chase the player. If the player is caught by the robot, it will prompt them to answer a puzzle correctly of which they will have 3 tries to get it right. Each failed puzzle increases the player’s fear level. Depending on the outcome of the attempts by the player to solve the puzzles, 1 of 3 outcomes can occur:
1. Player solves all puzzles and robot is deactivated
2. Player fails all attempts and the Hunter teleports to them and kills them.
3. Player fails only a couple puzzles and the fear meter increases to a high enough level that the Hunter is alerted to their position and hunts them. Player must get far enough away from all robots to decrease the fear meter and end the hunt.

  ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/sentinel%20robot.PNG?raw=true)
  
### Hunter AI

This AI is only triggered when the player's fear meter passes a certain threshold. It will then rush very quickly to the player’s current location until they kill the player. However if the player’s fear meter decreases enough that the threshhold is no longer exceeded, then it returns to its starting location.
  
  ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/hunter%20robot.PNG?raw=true)
  
### The Player

The player has two circles surrounding them, each serving a different purpose:
- Inner circle (RED): This is used for the Fear counter. The longer an AI is within this circle, the higher the fear counter increases. Once it gets too high, the Hunter robot and the nearest Sentinel are alerted to the player’s position and will head there.
- Outer circle (BLUE): This is used for AI optimization. Only AI within the circle will be active, otherwise they are deactivated and save CPU resources. This occurs off screen to not break immersion (the rectangle around the player is their view). 

  ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/Player%20Rings.jpg?raw=true)

## Path Finding (The Grid)

All AI use A* pathfinding logic with a dynamically generated grid. This grid merely needs to be attached to any object in the scene, and whatever 2D sprites are placed on the scene will have a grid block spawned at the same location. Sprites that are not meant to be walked on just need to toggle a variable, and the grid automatically sets the block to be unwalkable.


|![alt text](https://github.com/xGhostEYE/BinaryBreakout-AI-Prototype/blob/main/tile%20visible.PNG?raw=true)<br>Grid tiles visible with red tiles being unwalkable|![alt text](https://github.com/xGhostEYE/BinaryBreakout-AI-Prototype/blob/main/tile%20invisible.PNG?raw=true)<br>Tiles not visible|
|:-:|:-:|



## AI States

### Sentinel States
- **Patrol:** Walk from one node to the next and repeat this process. Each patrol waypoint can be placed freely in a walkable location on the grid. These waypoints are then referenced by drag and dropping them into a robot's instance waypoint array.
  
    ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/AI%20Pathing.jpg?raw=true)

- **Alerted:** Triggered when the Player's fear has exceeded the threshold or the player has failed a puzzle. Then the nearest Sentinel robot goes to their location.
  
- **Chasing:** Robot sees the player and chases after them. The Hunter robot will be activated here as well, as long as the Sentinel has the player within its vision FOV.

    ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/spotted.png?raw=true)

- **Deactivated:** The robot has been deactivated either due to story reasons, or the player passed all puzzles given to them by the robot.

### Hunter States:
- **Hunting:** The Hunter robot has been given the player location due to the player's fear being too high, or a Sentinel robot has entered the chasing state. The Hunter will then rush to the player's location in this state.
    
    ![alt text](https://github.com/xGhostEYE/Binary-Breakout---AI-Prototype/blob/main/chasing.jpg?raw=true)
  
- **Standby:** Hunter robot is stationary at its charge point and does nothing. It will also enter this state if it has lost the player's location, and return to its starting position.
