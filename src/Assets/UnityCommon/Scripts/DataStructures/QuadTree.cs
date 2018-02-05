using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;

namespace AltSrc.UnityCommon.DataStructures
{
    public interface IBounds
    {
        Rect Bounds { get; }
    }

    /// <summary>
    ///   A quadtree is a tree data structure in which each internal node has exactly four children.
    /// </summary>
    public class QuadTree<T> where T : IBounds
    {
        public int Depth { get; private set; }
        public Rect Bounds { get; private set; }
        public int MaxObjectsPerNode { get; private set; }
        public int MaxDepth { get; private set; }
        public List<T> Objects;
        public QuadTree<T>[] Nodes;

        public QuadTree(int depth, Rect bounds, int maxObjectsPerNode = 8, int maxDepth = 8)
        {
            this.Depth = depth;
            this.Bounds = bounds;
            this.MaxObjectsPerNode = maxObjectsPerNode;
            this.MaxDepth = maxDepth;

            this.Objects = new List<T>();
            this.Nodes = new QuadTree<T>[4];
        }

        /// <summary>
        ///   Insert an object into the quad tree. If this node exceeds its maximum it will split
        ///   into subnodes and insert the objects that fit into those subnodes.
        /// </summary>
        public void Insert(T o)
        {
            if (IsSplit())
            {
                int index = GetIndex(o.Bounds);

                if (index != -1)
                {
                    this.Nodes[index].Insert(o);
                    return;
                }
            }

            this.Objects.Add(o);

            if (this.Objects.Count > this.MaxObjectsPerNode
                && this.Depth < this.MaxDepth)
            {
                if (!IsSplit())
                {
                    Split();
                }

                int i = 0;

                while (i < this.Objects.Count)
                {
                    T obj = this.Objects[i];
                    int index = GetIndex(obj.Bounds);

                    if (index != -1)
                    {
                        this.Nodes[index].Insert(obj);
                        this.Objects.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        ///   Retrieve a list of objects in the same node/subnodes as bounds b.
        /// </summary>
        public List<T> Retrieve(Rect bounds)
        {
            List<T> retrieved = new List<T>();

            if (IsSplit())
            {
                int index = GetIndex(bounds);

                if (index != -1)
                {
                    retrieved.AddRange(this.Nodes[index].Retrieve(bounds));
                }
            }

            retrieved.AddRange(this.Objects);
            return retrieved;
        }

        /// <summary>
        ///   Clear the quadtree of all objects and subnodes.
        /// </summary>
        public void Clear()
        {
            this.Objects.Clear();

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i] != null)
                {
                    this.Nodes[i].Clear();
                    this.Nodes[i] = null;
                }
            }
        }

        /// <summary>
        ///   Retrieve all nodes and subnodes of this quad tree.
        /// </summary>
        public List<QuadTree<T>> GetAllNodes()
        {
            List<QuadTree<T>> nodes = new List<QuadTree<T>>();

            if (IsSplit())
            {
                for (int i = 0; i < this.Nodes.Length; i++)
                {
                    nodes.AddRange(this.Nodes[i].GetAllNodes());
                }
            }

            nodes.Add(this);

            return nodes;
        }

        /// <summary>
        ///   Returns the leaf node for specified bounds.
        /// </summary>
        public QuadTree<T> GetLeafNodeAt(Rect bounds)
        {
            if (IsSplit())
            {
                int index = GetIndex(bounds);

                if (index != -1)
                {
                    return this.Nodes[index].GetLeafNodeAt(bounds);
                }
            }

            return this;
        }

        /// <summary>
        ///   Split the quadtree into 4 subnodes.
        /// </summary>
        protected void Split()
        {
            float splitWidth = this.Bounds.width / 2;
            float splitHeight = this.Bounds.height / 2;
            float x = this.Bounds.x;
            float y = this.Bounds.y;

            Nodes[0] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x + splitWidth, y, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            Nodes[1] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x, y, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            Nodes[2] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x, y + splitHeight, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
            Nodes[3] = new QuadTree<T>(
                this.Depth + 1,
                new Rect(x + splitWidth, y + splitHeight, splitWidth, splitHeight),
                this.MaxObjectsPerNode,
                this.MaxDepth);
        }

        /// <summary>
        ///   Has this node been split into subnodes? Returns true if a subnode is assigned at index 0.
        /// </summary>
        protected bool IsSplit()
        {
            return this.Nodes[0] != null;
        }

        /// <summary>
        ///   Determine which node the object belongs in. A return value of -1 means the object
        ///   can't fit within a subnode and must belong to the parent node.
        /// </summary>
        protected int GetIndex(Rect bounds)
        {
            float xMiddle = this.Bounds.x + (this.Bounds.width / 2);
            float yMiddle = this.Bounds.y + (this.Bounds.height / 2);

            // Object can completely fit within the top quadrants.
            bool topQuadrants =
                bounds.y >= this.Bounds.y
                && bounds.y + bounds.height < yMiddle;

            // Object can completely fit within the bottom quadrants.
            bool bottomQuadrants =
                bounds.y > yMiddle
                && bounds.y + bounds.height <= this.Bounds.y + this.Bounds.height;

            // Object can completely fit within the left quadrants.
            if (bounds.x >= this.Bounds.x
                && bounds.x + bounds.width < xMiddle)
            {
                if (topQuadrants)
                {
                    return 1;
                }
                else if (bottomQuadrants)
                {
                    return 2;
                }
            }
            // Object can completely fit within the right quadrants.
            else if (bounds.x > xMiddle
                && bounds.x + bounds.width <= this.Bounds.x + this.Bounds.width)
            {
                if (topQuadrants)
                {
                    return 0;
                }
                else if (bottomQuadrants)
                {
                    return 3;
                }
            }

            return -1;

            // NOTE: If this function doesn't work as expected, try the following code. -Casper 2017-08-16
            /*
            // Object can completely fit within the top quadrants
            bool topQuadrants = (bounds.y < yMiddle && bounds.y + bounds.height < yMiddle);
            // Object can completely fit within the bottom quadrants
            bool bottomQuadrants = (bounds.y > yMiddle);

            // Object can completely fit within the left quadrants
            if (bounds.x < xMiddle && bounds.x + bounds.width < xMiddle)
            {
                // ... same here
            }
            // Object can completely fit within the right quadrants
            else if (bounds.x > xMiddle)
            {
                // ... same here
            }
            */
        }
    }
}
