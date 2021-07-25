# Octree-Unity

I've been trying to find a method of creating a point cloud mesh for my [Geometry Grass Shader](https://github.com/TheRensei/GrandGeoGrass-Public), or rather a way to modify a large point mesh with a good performance. I played around with an [Octree in C++](https://github.com/TheRensei/Octree-cpp) before, so I decided to continue working on it in Unity for that point cloud mesh modification. I'm fairly happy with the Octree at the moment and can start working on a Grass Painter tool.

Current features:
- Growing/Shrinking - tree will grow of point out of scope is added and shrink when root node has only one child remaining.
- Insertion/Removal - done by reference or ID, Safe Insertion/Removal checks if tree should shrink or grow.
- When inserting a new node will be split until each point is in different node OR leaf node was created. The implementation is like a 'bucket' octree, where Inner nodes are used to transport points into Leaf nodes ('buckets').
- Tree origin is adjusted if root has no children and a new object has been added to the tree.
- InnerNode/LeadNode and Points object pools - stacks of objects to reuse. Instead of deleting objects and creating new ones they are now stored for the future use.
- Radius search - returns a list of points in a given radius. It starts by checking if a given child origin lies inside an area box, and then each point inside it is checked against the radius.
- The tree will split until it reaches max depth or minimal node size allowed.

References and useful resources can be found [here](https://github.com/TheRensei/Octree-cpp)
