using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public OctreeNode rootNode = null;
    int maxDepth = 8;
    float minimumNodeSize = 1f;

    public Stack<INode> nodePool = new Stack<INode>();
    public Stack<INode> leafPool = new Stack<INode>();
    public Stack<OctreePoint> pointPool = new Stack<OctreePoint>();

    /// <summary>
    /// Create an octree.
    /// </summary>
    /// <param name="origin">Origin of the octree.</param>
    /// <param name="halfSize">Half of the extents of the octree.</param>
    /// <param name="maxDepth">Max depth the octree is allowed to reach.</param>
    /// <param name="minimumNodeSize">Minimum node half extent the tree is allowed to reach.</param>
    public Octree(Vector3 origin, float halfSize, int maxDepth, float minimumNodeSize)
    {
        this.rootNode = new OctreeNode(origin, halfSize);
        //Prevents initalizing the tree with size 0
        if (halfSize <= 2)
            this.rootNode.HalfSize = 2;

        this.maxDepth = maxDepth;
        this.minimumNodeSize = minimumNodeSize;
    }


    public void Insert(OctreePoint point)
    {
        if (IsPositionOutOfBounds(point.Position))
        {
            Grow(point.Position);
            Insert(point);
            return;
        }

        RecursiveInsert(point, rootNode);
    }

    /// <summary>
    /// Insert a point in the tree.
    /// </summary>
    /// <param name="position">Position of the point.</param>
    /// <param name="id">ID of the point.</param>
    public void Insert(Vector3 position, int id)
    {
        OctreePoint point;
        if (pointPool.Count > 0)
        {
            point = pointPool.Pop();
            point.Position = position;
            point.Id = id;
        }
        else
        {
            point = new OctreePoint(position, id);
        }

        RecursiveInsert(point, rootNode);
    }

    /// <summary>
    /// Insert a point in the tree. If the point is out of bounds, the tree grows in the points direction.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="id"></param>
    public void SafeInsert(Vector3 position, int id)
    {
        OctreePoint point;
        if (pointPool.Count > 0)
        {
            point = pointPool.Pop();
            point.Position = position;
            point.Id = id;
        }
        else
        {
            point = new OctreePoint(position, id);
        }

        if (IsPositionOutOfBounds(position))
        {
            Grow(position);
            Insert(point);
            return;
        }

        RecursiveInsert(point, rootNode);
    }

    /// <summary>
    /// Remove point from the tree.
    /// </summary>
    /// <param name="point">Point to be removed.</param>
    public void Remove(OctreePoint point)
    {
        RecursiveRemovePoint(point, rootNode);
    }

    public void Remove(int id, Vector3 pos)
    {
        RecursiveRemovePoint(id, pos, rootNode);
    }

    /// <summary>
    /// Remove point from the tree. Collapses the tree if root has only one child node.
    /// </summary>
    /// <param name="point">Point to be removed.</param>
    public void SafeRemove(OctreePoint point)
    {
        RecursiveRemovePoint(point, rootNode);

        Shrink();
    }

    public void SafeRemove(int id, Vector3 pos)
    {
        RecursiveRemovePoint(id, pos, rootNode);

        Shrink();
    }

    /// <summary>
    /// Adjusts root origin of the tree, only when root node has no chidlren
    /// </summary>
    /// <param name="position"></param>
    public void AdjustOrigin(Vector3 position)
    {
        //This should be done after initialization, after elements have been removed
        if (!rootNode.HasChildren())
        {
            rootNode.Origin = position;
        }
    }

    /// <summary>
    /// Traverses the tree and returns a list of points in a given radius.
    /// </summary>
    /// <param name="center">Center of the search radius.</param>
    /// <param name="radius">Search radius.</param>
    /// <returns></returns>
    public List<OctreePoint> GetPointsInRadius(Vector3 center, float radius)
    {
        List<OctreePoint> points = new List<OctreePoint>();

        Vector3 bmax = center + new Vector3(radius, radius, radius);
        Vector3 bmin = center - new Vector3(radius, radius, radius);

        RecursiveGetPointsInRadius(bmin, bmax, center, radius, ref points, rootNode);

        return points;
    }

    /// <summary>
    /// Grows the tree in the direction of the point.
    /// </summary>
    /// <param name="point">Point which was out of bounds.</param>
    void Grow(Vector3 point)
    {
        //if any point is out of scope of the tree, it has to grow
        Vector3 newOrigin = new Vector3(
            rootNode.Origin.x + (point.x > rootNode.Origin.x ? rootNode.HalfSize : -rootNode.HalfSize),
            rootNode.Origin.y + (point.y > rootNode.Origin.y ? rootNode.HalfSize : -rootNode.HalfSize),
            rootNode.Origin.z + (point.z > rootNode.Origin.z ? rootNode.HalfSize : -rootNode.HalfSize));

        OctreeNode newRoot = new OctreeNode(newOrigin, rootNode.HalfSize * 2);

        int index = newRoot.GetNodeIndexContainingPoint(rootNode.Origin);
        newRoot.children[index] = rootNode;
        newRoot.point = rootNode.point;
        rootNode.Parent = newRoot;
        rootNode.point = null;
        rootNode = newRoot;
        maxDepth++;
    }

    /// <summary>
    /// Shrinks the tree if a root node has only one child node.
    /// </summary>
    void Shrink()
    {
        if (rootNode.point != null)
            return;
        
        int count = 0;
        int octant = 0;

        for (int i = 0; i < 8; ++i)
        {
            if(rootNode.children[i] != null)
            {
                count++;
                octant = i;
            }
        }

        if (count != 1)
            return;

        if (rootNode.children[octant].IsLeafNode())
                return;
                
        rootNode = rootNode.children[octant] as OctreeNode;
        rootNode.Parent = null;
        maxDepth--;

        //Recursive will stop when:
        // - there are no children
        // - there is more than one child
        // - there is a point in this node
        Shrink();
    }

    /// <summary>
    /// Checs if this position out of bounds of the current node.
    /// </summary>
    /// <returns></returns>
    bool IsPositionOutOfBounds(Vector3 position)
	{
        Vector3 cmax = rootNode.Origin.AddFloat(rootNode.HalfSize);
        Vector3 cmin = rootNode.Origin.SubtractFloat(rootNode.HalfSize);

		if (position.x < cmax.x &&
			position.y < cmax.y &&
			position.z < cmax.z &&
			position.x > cmin.x &&
			position.y > cmin.y &&
			position.z > cmin.z)
		{
			return false;
		}
		return true;
	}

    /// <summary>
    /// Recursively traverses the tree to insert a point when the conditions are met.
    /// </summary>
    /// <param name="p">Point to be inserted.</param>
    /// <param name="currentNode">Current node being accessed.</param>
    /// <param name="currentDepth">Current traversal depth.</param>
    void RecursiveInsert(OctreePoint p, OctreeNode currentNode, int currentDepth = 0)
    {
        if (currentDepth >= maxDepth || (currentNode.HalfSize) <= minimumNodeSize)
        {
            int index = currentNode.GetNodeIndexContainingPoint(p.Position);

            if (currentNode.children[index] == null)
            {
                currentNode.Split(index, true, ref leafPool);
            }

            currentNode.children[index].InsertPoint(p);
            return;
        }

        if(!currentNode.HasChildren())
        {
            if (currentNode.point == null)
            {
                //currentNode.point = p;
                currentNode.InsertPoint(p);
                return;
            }
            else
            {
                OctreePoint oldPoint = currentNode.point;
                currentNode.point = null;

                int index = currentNode.GetNodeIndexContainingPoint(oldPoint.Position);

                if (currentNode.children[index] == null)
                {
                    currentNode.Split(index, false, ref nodePool);
                }

                int otherIndex = currentNode.GetNodeIndexContainingPoint(p.Position);

                if (currentNode.children[otherIndex] == null)
                {
                    currentNode.Split(otherIndex, false, ref nodePool);
                }

                this.RecursiveInsert(oldPoint, currentNode.children[index] as OctreeNode, currentDepth + 1);
                this.RecursiveInsert(p, currentNode.children[otherIndex] as OctreeNode, currentDepth + 1);
                return;
            }
        }
        else
        {
            int index = currentNode.GetNodeIndexContainingPoint(p.Position);

            if (currentNode.children[index] == null)
            {
                currentNode.Split(index, false, ref nodePool);
            }

            this.RecursiveInsert(p, currentNode.children[index] as OctreeNode, currentDepth + 1);
            return;
        }
    }

    /// <summary>
    /// Traverses the tree for the search of points in a given radius.
    /// It's done by first checking if the node intersects with a box surrounding the search radius,
    /// then another check for all points in intersecting nodes against this radius.
    /// </summary>
    /// <param name="bmin">Minimum extent of the search box.</param>
    /// <param name="bmax">Maximum extent of the search box.</param>
    /// <param name="center">Center of the search box.</param>
    /// <param name="radius">Radius for the radius search</param>
    /// <param name="points">Reference to the list of found points to be returned.</param>
    /// <param name="currentNode">Current node being accessed.</param>
    /// <param name="currentDepth">Current traversal depth.</param>
    void RecursiveGetPointsInRadius(Vector3 bmin, Vector3 bmax, Vector3 center, float radius, ref List<OctreePoint> points, OctreeNode currentNode, int currentDepth = 0)
    {
        if (currentDepth >= maxDepth || currentNode.HalfSize <= minimumNodeSize)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (currentNode.children[i] != null)
                {
                    Vector3 cmax = currentNode.children[i].Origin.AddFloat(currentNode.children[i].HalfSize);
                    Vector3 cmin = currentNode.children[i].Origin.SubtractFloat(currentNode.children[i].HalfSize);
        
                    if (cmax.x < bmin.x || cmax.y < bmin.y || cmax.z < bmin.z) continue;
                    if (cmin.x > bmax.x || cmin.y > bmax.y || cmin.z > bmax.z) continue;

     
                    currentNode.children[i].GetPointsInRadius(center, radius, ref points);
                }
            }
            return;
        }

        if (!currentNode.HasChildren())
        {
            if (currentNode.point != null)
            {
                currentNode.GetPointsInRadius(center, radius, ref points);
                return;
            }
        }
        else
        {
            for (int i = 0; i < 8; ++i)
            {
                if (currentNode.children[i] != null)
                {
                    Vector3 cmax = currentNode.children[i].Origin.AddFloat(currentNode.children[i].HalfSize);
                    Vector3 cmin = currentNode.children[i].Origin.SubtractFloat(currentNode.children[i].HalfSize);

                    if (cmax.x < bmin.x || cmax.y < bmin.y || cmax.z < bmin.z) continue;
                    if (cmin.x > bmax.x || cmin.y > bmax.y || cmin.z > bmax.z) continue;

                    this.RecursiveGetPointsInRadius(bmin, bmax, center, radius, ref points, currentNode.children[i] as OctreeNode, currentDepth + 1);
                }
            }
        }
    }

    /// <summary>
    /// Finds the node to be removed by recursively traversing the tree.
    /// </summary>
    /// <param name="p">Point to be found</param>
    /// <param name="returnNode">When the node is found it will use this reference to return it.</param>
    /// <param name="currentNode">Current node being accessed.</param>
    /// <param name="currentDepth">Current traversal depth.</param>
    void RecursiveGetNode(OctreePoint p, ref INode returnNode, INode currentNode = null, int currentDepth = 0)
    {
        //if leaf node return
        if (currentNode.IsLeafNode())
        {
            returnNode = currentNode;
            return;
        }

        //we know it's definitely not leaf so treat as OctreeNode
        if ((currentNode as OctreeNode).point != null)
        {
            if ((currentNode as OctreeNode).point == p)
            {
                returnNode = currentNode;
                return;
            }
        }

        int index = (currentNode as OctreeNode).GetNodeIndexContainingPoint(p.Position);
        
        this.RecursiveGetNode(p, ref returnNode, (currentNode as OctreeNode).children[index], currentDepth + 1);
    }

    void RecursiveRemovePoint(OctreePoint p, INode currentNode = null, int currentDepth = 0)
    {
        //if leaf node return
        if (currentNode.IsLeafNode())
        {
            currentNode.RemovePoint(p, ref nodePool, ref leafPool, ref pointPool);
            return;
        }

        //we know it's definitely not leaf so treat as OctreeNode
        if ((currentNode as OctreeNode).point != null)
        {
            if ((currentNode as OctreeNode).point == p)
            {
                currentNode.RemovePoint(p, ref nodePool, ref leafPool, ref pointPool);
                return;
            }
        }

        int index = (currentNode as OctreeNode).GetNodeIndexContainingPoint(p.Position);

        this.RecursiveRemovePoint(p, (currentNode as OctreeNode).children[index], currentDepth + 1);
    }

    void RecursiveRemovePoint(int pid, Vector3 pos, INode currentNode = null, int currentDepth = 0)
    {
        //if leaf node return
        if (currentNode.IsLeafNode())
        {
            currentNode.RemovePoint(pid, ref nodePool, ref leafPool, ref pointPool);
            return;
        }

        //we know it's definitely not leaf so treat as OctreeNode
        if ((currentNode as OctreeNode).point != null)
        {
            if ((currentNode as OctreeNode).point.Id == pid)
            {
                currentNode.RemovePoint(pid, ref nodePool, ref leafPool, ref pointPool);
                return;
            }
        }

        int index = (currentNode as OctreeNode).GetNodeIndexContainingPoint(pos);

        this.RecursiveRemovePoint(pid, pos, (currentNode as OctreeNode).children[index], currentDepth + 1);
    }
}
