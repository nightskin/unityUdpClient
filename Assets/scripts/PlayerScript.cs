using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    
    public string id;
    public float r;
    public float g;
    public float b;
    public NetworkMan client;
    GameObject clientObj;
    void Start()
    {
        clientObj = GameObject.Find("NetworkMan");
        client = clientObj.GetComponent<NetworkMan>();
        if (client.latestGameState.players.Count >= 1)
        {
            id = client.latestGameState.players[client.latestGameState.players.Count -1].id;
            r = client.latestGameState.players[client.latestGameState.players.Count - 1].color.R;
            g = client.latestGameState.players[client.latestGameState.players.Count - 1].color.G;
            b = client.latestGameState.players[client.latestGameState.players.Count - 1].color.B;
        }
    }


   
}
