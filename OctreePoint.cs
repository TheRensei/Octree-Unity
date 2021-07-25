using UnityEngine;

public class OctreePoint
{
    public OctreePoint(Vector3 position, int id)
    {
        Position = position;
        this.Id = id;
    }

    public Vector3 Position { get; set; }
    public int Id { get; set; }
}
