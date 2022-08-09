using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject player;
    public static Action<Vector2> EnterRoom;

    private void Update()
    {
        Vector3 setPos = player.transform.position;
        int roomSize = FindObjectOfType<DungeonGenerator>().roomSize;
        setPos.x = Mathf.Round(setPos.x / roomSize) * roomSize;
        setPos.y = Mathf.Round(setPos.y / roomSize) * roomSize;
        setPos.z = -10;
        if (transform.position != setPos)
        {
            transform.position = setPos;
            EnterRoom(setPos / roomSize);
        } 
    }
}
