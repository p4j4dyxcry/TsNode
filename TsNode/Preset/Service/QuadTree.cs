using System;
using System.Collections.Generic;
using System.Windows;

namespace TsNode.Preset.Service
{
    /// <summary>
    /// 4分木空間のあたり判定用
    /// </summary>
    public class QuadTree<T>
    {
        Rect _bounds;
        Quadrant _root;
        IDictionary<T, Quadrant> _table;
 
        public Rect Bounds
        {
            get => this._bounds;
            set { this._bounds = value; ReIndex(); }
        }
 
        public void Insert(T node, Rect bounds)
        {
            if (this._bounds.Width is 0 || this._bounds.Height is 0)
            {
                throw new ArgumentException(nameof(bounds));
            }
            if (bounds.Width is 0 || bounds.Height is 0)
            {
                throw new ArgumentException(nameof(bounds));
            }
            if (this._root is null)
            {
                this._root = new Quadrant(null, this._bounds);
            }
 
            Quadrant parent = this._root.Insert(node, bounds);
 
            if (this._table == null)
            {
                this._table = new Dictionary<T, Quadrant>();
            }
            this._table[node] = parent;
        }
 
        public IEnumerable<T> GetNodesInside(Rect bounds)
        {
            foreach (QuadNode n in GetNodes(bounds))
            {
                yield return n.Node;
            }
        }
        public bool HasNodesInside(Rect bounds)
        {
            if (this._root == null)
            {
                return false;                
            }
            return this._root.HasIntersectingNodes(bounds);
        }

        IEnumerable<QuadNode> GetNodes(Rect bounds)
        {
            List<QuadNode> result = new List<QuadNode>();
            _root?.GetIntersectingNodes(result, bounds);
            return result;
        }
 
        public bool Remove(T node)
        {
            if (this._table is null) 
                return false;
            
            if (this._table.TryGetValue(node, out var parent))
            {
                parent.RemoveNode(node);
                this._table.Remove(node);
                return true;
            }
            return false;
        }
 
        void ReIndex()
        {
            this._root = null;
            foreach (QuadNode n in GetNodes(this._bounds))
            {
                Insert(n.Node, n.Bounds);
            }
        }

        internal class QuadNode
        {
            public T Node { get; set; }

            public Rect Bounds { get; }

            public QuadNode Next { get; set; }
            public QuadNode(T node, Rect bounds)
            {
                this.Node = node;
                this.Bounds = bounds;
            }
        }

        internal class Quadrant
        {
            private Quadrant Parent { get; }
            private Rect Bounds { get; }
 
            private QuadNode Nodes { get; set; }

            private Quadrant TopLeft { get; set; }
            private Quadrant TopRight { get; set; }
            private Quadrant BottomLeft { get; set; }
            private Quadrant BottomRight { get; set; }
 
            public Quadrant(Quadrant parent, Rect bounds)
            {
                this.Parent = parent;
                this.Bounds = bounds;
            }
 
            internal Quadrant Insert(T node, Rect bounds)
            {
                Quadrant toInsert = this;
                while (true)
                {
                    double w = toInsert.Bounds.Width / 2;
                    if (w < 1)
                    {
                        w = 1;
                    }
                    double h = toInsert.Bounds.Height / 2;
                    if (h < 1)
                    {
                        h = 1;
                    }

                    Rect topLeft = new Rect(toInsert.Bounds.Left, toInsert.Bounds.Top, w, h);
                    Rect topRight = new Rect(toInsert.Bounds.Left + w, toInsert.Bounds.Top, w, h);
                    Rect bottomLeft = new Rect(toInsert.Bounds.Left, toInsert.Bounds.Top + h, w, h);
                    Rect bottomRight = new Rect(toInsert.Bounds.Left + w, toInsert.Bounds.Top + h, w, h);
 
                    Quadrant child = null;
 
                    if (topLeft.Contains(bounds))
                    {
                        if (toInsert.TopLeft == null)
                        {
                            toInsert.TopLeft = new Quadrant(toInsert, topLeft);
                        }
                        child = toInsert.TopLeft;
                    }
                    else if (topRight.Contains(bounds))
                    {
                        if (toInsert.TopRight == null)
                        {
                            toInsert.TopRight = new Quadrant(toInsert, topRight);
                        }
                        child = toInsert.TopRight;
                    }
                    else if (bottomLeft.Contains(bounds))
                    {
                        if (toInsert.BottomLeft == null)
                        {
                            toInsert.BottomLeft = new Quadrant(toInsert, bottomLeft);
                        }
                        child = toInsert.BottomLeft;
                    }
                    else if (bottomRight.Contains(bounds))
                    {
                        if (toInsert.BottomRight == null)
                        {
                            toInsert.BottomRight = new Quadrant(toInsert, bottomRight);
                        }
                        child = toInsert.BottomRight;
                    }
 
                    if (child != null)
                    {
                        toInsert = child;
                    }
                    else
                    {
                        QuadNode n = new QuadNode(node, bounds);
                        if (toInsert.Nodes == null)
                        {
                            n.Next = n;
                        }
                        else
                        {
                            QuadNode x = toInsert.Nodes;
                            n.Next = x.Next;
                            x.Next = n;
                        }
                        toInsert.Nodes = n;
                        return toInsert;
                    }
                }
            }
 
            internal void GetIntersectingNodes(List<QuadNode> nodes, Rect bounds)
            {
                if (bounds.IsEmpty) 
                    return;

                double w = Bounds.Width / 2;
                double h = Bounds.Height / 2;

                Rect topLeft = new Rect(this.Bounds.Left, this.Bounds.Top, w, h);
                Rect topRight = new Rect(this.Bounds.Left + w, this.Bounds.Top, w, h);
                Rect bottomLeft = new Rect(this.Bounds.Left, this.Bounds.Top + h, w, h);
                Rect bottomRight = new Rect(this.Bounds.Left + w, this.Bounds.Top + h, w, h);
 
                if (topLeft.IntersectsWith(bounds))
                {
                    TopLeft?.GetIntersectingNodes(nodes, bounds);
                }
 
                if (topRight.IntersectsWith(bounds))
                {
                    TopRight?.GetIntersectingNodes(nodes, bounds);
                }
 
                if (bottomLeft.IntersectsWith(bounds))
                {
                    BottomLeft?.GetIntersectingNodes(nodes, bounds);
                }
 
                if (bottomRight.IntersectsWith(bounds))
                {
                    BottomRight?.GetIntersectingNodes(nodes, bounds);
                }
 
                GetIntersectingNodes(this.Nodes, nodes, bounds);
            }
            static void GetIntersectingNodes(QuadNode last, List<QuadNode> nodes, Rect bounds)
            {
                if (last != null)
                {
                    QuadNode n = last;
                    do
                    {
                        n = n.Next; // first node.
                        if (n.Bounds.IntersectsWith(bounds))
                        {
                            nodes.Add(n);
                        }
                    } while (n != last);
                }
            }
 
            internal bool HasIntersectingNodes(Rect bounds)
            {
                if (bounds.IsEmpty) return false;
                double w = this.Bounds.Width / 2;
                double h = this.Bounds.Height / 2;
 
                Rect topLeft = new Rect(this.Bounds.Left, this.Bounds.Top, w, h);
                Rect topRight = new Rect(this.Bounds.Left + w, this.Bounds.Top, w, h);
                Rect bottomLeft = new Rect(this.Bounds.Left, this.Bounds.Top + h, w, h);
                Rect bottomRight = new Rect(this.Bounds.Left + w, this.Bounds.Top + h, w, h);
 
                bool found = false;
 
                if (topLeft.IntersectsWith(bounds) && this.TopLeft != null)
                {
                    found = this.TopLeft.HasIntersectingNodes(bounds);
                }
 
                if (!found && topRight.IntersectsWith(bounds) && this.TopRight != null)
                {
                    found = this.TopRight.HasIntersectingNodes(bounds);
                }
 
                if (!found && bottomLeft.IntersectsWith(bounds) && this.BottomLeft != null)
                {
                    found = this.BottomLeft.HasIntersectingNodes(bounds);
                }
 
                if (!found && bottomRight.IntersectsWith(bounds) && this.BottomRight != null)
                {
                    found = this.BottomRight.HasIntersectingNodes(bounds);
                }
                if (!found)
                {
                    found = HasIntersectingNodes(this.Nodes, bounds);
                }
                return found;
            }
 
            static bool HasIntersectingNodes(QuadNode last, Rect bounds)
            {
                if (last != null)
                {
                    var n = last;
                    do
                    {
                        n = n.Next; // first node.
                        if (n.Bounds.IntersectsWith(bounds))
                        {
                            return true;
                        }
                    } while (n != last);
                }
                return false;
            }
 
            internal bool RemoveNode(T node)
            {
                bool rc = false;
                if (this.Nodes != null)
                {
                    QuadNode p = this.Nodes;
                    while (!Equals(p.Next.Node, node) && p.Next != this.Nodes)
                    {
                        p = p.Next;
                    }
                    if (Equals(p.Next.Node, node))
                    {
                        rc = true;
                        QuadNode n = p.Next;
                        if (p == n)
                        {
                            // list goes to empty
                            this.Nodes = null;
                        }
                        else
                        {
                            if (this.Nodes == n) this.Nodes = p;
                            p.Next = n.Next;
                        }
                    }
                }
                return rc;
            } 
        }
    }
 
}