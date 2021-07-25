using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    Vector3 Origin { get; set; }
    float HalfSize { get; set; }
    OctreeNode Parent { get; set; }
    void InsertPoint(OctreePoint p);
    void GetPointsInRadius(Vector3 center, float radius, ref List<OctreePoint> points);

    void RemovePoint(OctreePoint p, ref Stack<INode> nodePool, ref Stack<INode> leafPool, ref Stack<OctreePoint> pointPool);
    void RemovePoint(int pid, ref Stack<INode> nodePool, ref Stack<INode> leafPool, ref Stack<OctreePoint> pointPool);
    bool IsLeafNode();
}

public class LeafNode : INode
{
    public Vector3 Origin { get; set; }
    public float HalfSize { get; set; }
    public OctreeNode Parent { get; set; }

    public List<OctreePoint> points = new List<OctreePoint>();

    public LeafNode(Vector3 origin, float halfSize)
    {
        Origin = origin;
        HalfSize = halfSize;
        Parent = null;
    }

    /// <summary>
    /// Insert a new point in this node.
    /// </summary>
    /// <param name="p">Point to be inserted.</param>
    public void InsertPoint(OctreePoint p)
    {
        points.Add(p);
    }

    /// <summary>
    /// Find and return all points inside a given radius.
    /// </summary>
    /// <param name="center">Center of the search.</param>
    /// <param name="radius">Radius of the search.</param>
    /// <param name="p">Reference to the list which will be returned.</param>
    public void GetPointsInRadius(Vector3 center, float radius, ref List<OctreePoint> p)
    {
        foreach (OctreePoint point in points)
        {
            if ((point.Position - center).sqrMagnitude < (radius * radius))
            {
                p.Add(point);
            }
        }
    }

    /// <summary>
    /// Remove point from this node.
    /// </summary>
    /// <param name="p">Point to be removed.</param>
    public void RemovePoint(OctreePoint p, ref Stack<INode> nodePool, ref Stack<INode> leafPool, ref Stack<OctreePoint> pointPool)
    {
        for (int i = 0; i < points.Count; ++i)
        {
            if (points[i].Id == p.Id)
            {
                pointPool.Push(points[i]);
                points.Remove(points[i]);
            }
        }

        if(points.Count == 0)
        {
            leafPool.Push(this);
            //delete this node from parent
            Parent.RemoveChildNode(this, ref nodePool, ref leafPool);

        }
    }

    /// <summary>
    /// Remove point from this node.
    /// </summary>
    /// <param name="p">Point to be removed.</param>
    public void RemovePoint(int pid, ref Stack<INode> nodePool, ref Stack<INode> leafPool, ref Stack<OctreePoint> pointPool)
    {
        for (int i = 0; i < points.Count; ++i)
        {
            if (points[i].Id == pid)
            {
                pointPool.Push(points[i]);
                points.Remove(points[i]);
            }
        }

        if (points.Count == 0)
        {
            leafPool.Push(this);
            //delete this node from parent
            Parent.RemoveChildNode(this, ref nodePool, ref leafPool);

        }
    }

    public bool IsLeafNode()
    {
        return true;
    }
}

public class OctreeNode : INode
{
    public Vector3 Origin { get; set; }
    public float HalfSize { get; set; }
    public OctreeNode Parent { get; set; }

    public INode[] children = new INode[8];

    public OctreePoint point = null;

    public OctreeNode(Vector3 origin, float halfSize)
    {
        Origin = origin;
        HalfSize = halfSize;
        Parent = null;
    }

    public OctreeNode() {
    }

    /// <summary>
    /// Checks if this node has any children.
    /// </summary>
    /// <returns></returns>
    public bool HasChildren()
    {
        for (int i = 0; i < 8; ++i)
        {
            if (children[i] != null)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Splits the node by creating a new node in an octant specified.
    /// </summary>
    /// <param name="octant">Octant at which a new node is to be created.</param>
    /// <param name="makeLeaf">Should leaf node be created.</param>
    public void Split(int octant, bool makeLeaf, ref Stack<INode> nodePool)
    {
        Vector3 pos = new Vector3(
                Origin.x + HalfSize * ((octant & 4) != 0 ? 0.5f : -0.5f),
                Origin.y + HalfSize * ((octant & 2) != 0 ? 0.5f : -0.5f),
                Origin.z + HalfSize * ((octant & 1) != 0 ? 0.5f : -0.5f));

        if (makeLeaf)
        {
            if(nodePool.Count > 0)
            {
                children[octant] = nodePool.Pop();
                children[octant].Origin = pos;
                children[octant].HalfSize = HalfSize * 0.5f;
                children[octant].Parent = this;
                return;
            }

            children[octant] = new LeafNode(pos, HalfSize * 0.5f)
            {
                Parent = this
            };
        }
        else
        {
            if (nodePool.Count > 0)
            {
                children[octant] = nodePool.Pop();
                children[octant].Origin = pos;
                children[octant].HalfSize = HalfSize * 0.5f;
                children[octant].Parent = this;

                return;
            }

            children[octant] = new OctreeNode(pos, HalfSize * 0.5f)
            {
                Parent = this
            };
        }
    }

    /// <summary>
    /// Returns the octant containing given position.
    /// </summary>
    /// <returns></returns>
    public int GetNodeIndexContainingPoint(Vector3 position)
	{
        int oct = 0;   
		if (position.x >= Origin.x) oct |= 4;
		if (position.y >= Origin.y) oct |= 2;
		if (position.z >= Origin.z) oct |= 1;
		return oct;
	}

    /// <summary>
    /// Checks if point in this node is inside a given radius.
    /// </summary>
    /// <param name="center">Center of the search.</param>
    /// <param name="radius">Radius of the search.</param>
    /// <param name="points">Reference to the list of points to be returned.</param>
    public void GetPointsInRadius(Vector3 center, float radius, ref List<OctreePoint> points)
    {
        if ((point.Position - center).sqrMagnitude < (radius * radius))
        {
            points.Add(point);
        }
    }

    /// <summary>
    /// Inserts a point in this node.
    /// </summary>
    /// <param name="p">Node to be inserted.</param>
    public void InsertPoint(OctreePoint p)
    {
        point = p;
    }

    /// <summary>
    /// Removes given point from this node.
    /// </summary>
    /// <param name="p">Point to be removed.</param>
    public void RemovePoint(OctreePoint p, ref Stack<INode> nodePool, ref Stack<INode> leafPool, ref Stack<OctreePoint> pointPool)
    {
        pointPool.Push(point);
        point = null;

        nodePool.Push(this);
        this.Parent.RemoveChildNode(this, ref nodePool, ref leafPool);
    }

    public void RemovePoint(int pid, ref Stack<INode> nodePool, ref Stack<INode> leafPool, ref Stack<OctreePoint> pointPool)
    {
        pointPool.Push(point);
        point = null;

        nodePool.Push(this);
        this.Parent.RemoveChildNode(this, ref nodePool, ref leafPool);
    }

    public bool IsLeafNode()
    {
        return false;
    }

    /// <summary>
    /// Removes a specified child node from this node.
    /// Recursively goes up the tree removing nodes if they have no children.
    /// </summary>
    /// <param name="node">Node to be removed.</param>
    public void RemoveChildNode(INode node, ref Stack<INode> nodePool, ref Stack<INode> leafPool)
    {
        for (int i = 0; i < 8; ++i)
        {
            if (children[i] == node)
            {
                children[i] = null;
            }
        }

        if (!this.HasChildren())
        {
            if (point == null)
            {
                if (this.Parent != null)
                {
                    nodePool.Push(this);
                    this.Parent.RemoveChildNode(this, ref nodePool, ref leafPool);
                }
            }
        }
    }
}
