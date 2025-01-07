using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RobotVision : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool CanSeePlayer;
    public float viewRadius;
    public float viewAngle;

    void Start(){
        CanSeePlayer = false;
    }
    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag("Player")){
            // Debug.Log("i see player");

            // SAYEM AUDIO CODE
            FindObjectOfType<AudioManager>().Play("RobotSeePlayer");

            CanSeePlayer = true;
            FearArea.instance?.InRobotVision(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision){
        if (collision.CompareTag("Player")){
            // Debug.Log("cant see player");

            // SAYEM AUDIO CODE
            FindObjectOfType<AudioManager>().Play("RobotUnseePlayer");
            CanSeePlayer = false;
            FearArea.instance?.InRobotVision(false);
        }

    }
}
