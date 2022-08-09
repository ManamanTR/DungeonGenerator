using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonWallTemplate[] walls;
    public List<DungeonGeneratedWall> dgws;
    public ItemsToPlace itemsToPlace;
    public int roomsToCreate, roomSize;
    public Vector2 roomArea;
    public GameObject player;
    Vector2 inRoom;

    private void Start()
    {
        //Check if the available space is smaller than the rooms to create
        if (roomArea.x * roomArea.y < roomsToCreate)
            return;
        //Clear the DGWs
        dgws.Clear();
        //Set the starting room
        DungeonGeneratedWall startDGW = new DungeonGeneratedWall(Mathf.CeilToInt(roomArea.x / 2), Mathf.CeilToInt(roomArea.y / 2), Get(true, true, true, true));
        //Set the player's starting position
        player.transform.position = new Vector3(Mathf.CeilToInt(roomArea.x / 2) * roomSize, Mathf.CeilToInt(roomArea.y / 2) * roomSize, 0);
        //Add the starting room to DGWs list
        AddDGW(startDGW);
        //Sets the items in the starting room to empty
        startDGW.itemsIn = new GameObject[0];
        for (int i = 0; i < roomsToCreate - 1; i++)
        {
            AddNext();
        }
        foreach (DungeonGeneratedWall dgw in dgws)
            SetTemplate(dgw);
        //This is an Action from my camera move function and occurs everytime when the player changes room
        CameraMove.EnterRoom += LoadRoom;
    }

    //Loades items in a room
    void LoadRoom(Vector2 pos)
    {
        //Returns if its the same room player's already in
        if (pos == inRoom)
            return;
        //Returns if the items had already been loaded
        if (Get(pos).itemsIn != null)
            return;
        //Sets the room player's in
        inRoom = pos;
        //Adds places items going to be placed randomly
        List<Vector2> places = new List<Vector2>();
        foreach (Vector2 p in itemsToPlace.placePositions)
            if (RandomBoolean())
                places.Add(p);
        int i = 0;
        List<GameObject> placed = new List<GameObject>();
        //Sets the room as empty if there isn't any places
        if (places.Count == 0)
        {
            Get(pos).itemsIn = new GameObject[0];
            return;
        }
        //Adds a random item to each of the place
        foreach (Vector2 p in places)
        {
            GameObject place = Instantiate(itemsToPlace.placeables[Random.Range(0, itemsToPlace.placeables.Length)], Get(pos).gm.transform);
            place.transform.position = GetPosInRoom(Get(pos), p);
            place.name = "Item" + i;
            placed.Add(place);
            i++;
        }
        //Adds items in the room to DGW
        Get(pos).itemsIn = placed.ToArray();
    }
    
    Vector2 GetPosInRoom(DungeonGeneratedWall dgw, Vector2 pos)
    {
        //Multiplies the position of the room and its size, and adds the given value
        return dgw.pos * roomSize + pos;
    }

    void SetTemplate(DungeonGeneratedWall dgw)
    {
        //Checks if there is rooms in every direction and adds doors and returns true or false
        dgw.template = Get(IsOverlapping(dgw.pos + Vector2.up), IsOverlapping(dgw.pos + Vector2.right), IsOverlapping(dgw.pos + Vector2.down), IsOverlapping(dgw.pos + Vector2.left));
        SetDGW(dgw);
    }

    void AddNext()
    {
        //Gets a random room
        Vector2 pos = dgws[Random.Range(0, dgws.Count)].pos;
        //Runs 2 random booleans and adds or subtracts from x or y
        if (RandomBoolean())
            pos.x += RandomBoolean() ? 1 : -1;
        else
            pos.y += RandomBoolean() ? 1 : -1;
        //Creates a new DGW
        DungeonGeneratedWall dgw = new DungeonGeneratedWall((int)pos.x, (int)pos.y, Get(true, true, true, true));
        //Checks if its in the area or overlapping and adds to the list. If its overlapping or outbounds recalls the function
        if (!IsOverlapping(dgw.pos) && IsInbound(dgw))
            AddDGW(dgw);
        else
            AddNext();
    }
    bool IsInbound(DungeonGeneratedWall dgw)
    {
        //Checks if its in the given area
        if (dgw.pos.x > roomArea.x || dgw.pos.x < 1 || dgw.pos.y > roomArea.y || dgw.pos.y < 1)
            return false;
        return true;
    }
    bool IsOverlapping(Vector2 pos)
    {
        //Checks if it is overlapping with any of the DGWs created before
        foreach (DungeonGeneratedWall d in dgws)
            if (d.pos == pos)
                return true;
        return false;
    }
    void AddDGW(DungeonGeneratedWall dgw)
    {
        //Adds the given DGW to the DGWs list
        dgws.Add(dgw);
    }
    void SetDGW(DungeonGeneratedWall dgw)
    {
        //Gets the template gameobject from itself and creates a gameobject with the parent object Dungeon
        dgw.gm = Instantiate(dgw.template.gameObject, GameObject.Find("Dungeon").transform);
        //Sets its position
        dgw.gm.transform.position = dgw.pos * roomSize;
        //Gives a name to it
        dgw.gm.name = "DungeonTile(" + dgw.pos.x + "," + dgw.pos.y + ")";
    }

    DungeonWallTemplate Get(bool t, bool r, bool b, bool l)
    {
        //Checks if the template has the same doors and returns the template
        foreach (DungeonWallTemplate w in walls)
            if (t == w.top && r == w.right && b == w.bottom && l == w.left)
                return w;
        return null;
    }

    DungeonGeneratedWall Get(Vector2 pos)
    {
        //Gets the room from its position
        foreach (DungeonGeneratedWall d in dgws)
            if (pos == d.pos)
                return d;
        return null;
    }

    bool RandomBoolean()
    {
        //Just a random boolean function I found on the internet
        if (Random.value >= 0.5)
        {
            return true;
        }
        return false;
    }

}

[System.Serializable]
public class DungeonGeneratedWall
{
    //Positions of the room (Not the one in transform)
    public Vector2 pos;
    public DungeonWallTemplate template;
    //Room's Gameobject
    public GameObject gm;
    //Items in the room
    public GameObject[] itemsIn;

    public DungeonGeneratedWall(int x, int y, DungeonWallTemplate template)
    {
        pos.x = x;
        pos.y = y;
        this.template = template;
        itemsIn = null;
    }
}

[System.Serializable]
public class ItemsToPlace
{
    //Placeable positions in a room
    public Vector2[] placePositions;

    //Placeable Objects
    public GameObject[] placeables;
}

[System.Serializable]
public class DungeonWallTemplate
{
    //Door position
    public bool top, right, bottom, left;

    //Gameobject to use
    public GameObject gameObject;
}
