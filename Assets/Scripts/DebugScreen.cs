using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
    }

    void Update()
    {
        string debugText = "Everfree";
        debugText += "\n";
        debugText += "FPS: " + frameRate;
        debugText += "\n\n";
        debugText += "Position: X: " + world.player.transform.position.x + ", Y: " + world.player.transform.position.y + ", Z: " + world.player.transform.position.z;
        debugText += "\n";
        debugText += "Chunk Pos: X: " + world.playerChunkCoord.x + ", Z: " + world.playerChunkCoord.z;

        text.text = debugText;

        if(timer > 1f)
        {
            frameRate = (int) (1f / Time.unscaledDeltaTime);
            timer = 0f;
        }
        else
            timer += Time.deltaTime;
    }
}
