﻿using System;
using System.Collections.Generic;
using System.Collections;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Data.Structures
{
    //====================================================
    //| Downloaded From                                  |
    //| Visual C# Kicks - http://www.vcskicks.com/       |
    //| License - http://www.vcskicks.com/license.html   |
    //====================================================
    /// <summary>
    /// A Binary Tree node that holds an element and references to other tree nodes
    /// </summary>
    public class BinaryTreeNode<T>
        where T : IComparable
    {
        public override string ToString()
        {
            return Value.ToString();
        }

        private T value;

        /// <summary>
        /// The value stored at the node
        /// </summary>
        public virtual T Value
        {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// Gets or sets the left child node
        /// </summary>
        public virtual BinaryTreeNode<T> LeftChild { get; set; }

        /// <summary>
        /// Gets or sets the right child node
        /// </summary>
        public virtual BinaryTreeNode<T> RightChild { get; set; }

        /// <summary>
        /// Gets or sets the parent node
        /// </summary>
        public virtual BinaryTreeNode<T> Parent { get; set; }

        /// <summary>
        /// Gets or sets the Binary Tree the node belongs to
        /// </summary>
        public virtual BinaryTree<T> Tree { get; set; }

        /// <summary>
        /// Gets whether the node is a leaf (has no children)
        /// </summary>
        public virtual bool IsLeaf => ChildCount == 0;

        /// <summary>
        /// Gets whether the node is internal (has children nodes)
        /// </summary>
        public virtual bool IsInternal => ChildCount > 0;

        /// <summary>
        /// Gets whether the node is the left child of its parent
        /// </summary>
        public virtual bool IsLeftChild => Parent != null && Parent.LeftChild == this;

        /// <summary>
        /// Gets whether the node is the right child of its parent
        /// </summary>
        public virtual bool IsRightChild => Parent != null && Parent.RightChild == this;

        /// <summary>
        /// Gets the number of children this node has
        /// </summary>
        public virtual int ChildCount
        {
            get
            {
                var count = 0;

                if (LeftChild != null)
                {
                    count++;
                }

                if (RightChild != null)
                {
                    count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets whether the node has a left child node
        /// </summary>
        public virtual bool HasLeftChild => (LeftChild != null);

        /// <summary>
        /// Gets whether the node has a right child node
        /// </summary>
        public virtual bool HasRightChild => (RightChild != null);

        /// <summary>
        /// Create a new instance of a Binary Tree node
        /// </summary>
        public BinaryTreeNode(T value)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Binary Tree data structure
    /// </summary>
    public class BinaryTree<T> : ICollection<T>
        where T : IComparable
    {
        /// <summary>
        /// Specifies the mode of scanning through the tree
        /// </summary>
        public enum TraversalMode
        {
            InOrder = 0,
            PostOrder,
            PreOrder
        }

        private BinaryTreeNode<T> head;
        private readonly Comparison<IComparable> comparer = CompareElements;
        private int size;

        /// <summary>
        /// Gets or sets the root of the tree (the top-most node)
        /// </summary>
        public virtual BinaryTreeNode<T> Root
        {
            get => head;
            set => head = value;
        }

        /// <summary>
        /// Gets whether the tree is read-only
        /// </summary>
        public virtual bool IsReadOnly => false;

        public int ToleranceInPPM { get; set; }

        /// <summary>
        /// Gets the number of elements stored in the tree
        /// </summary>
        public virtual int Count => size;

        /// <summary>
        /// Gets or sets the traversal mode of the tree
        /// </summary>
        public virtual TraversalMode TraversalOrder { get; set; } = TraversalMode.InOrder;

        /// <summary>
        /// Creates a new instance of a Binary Tree
        /// </summary>
        public BinaryTree()
        {
            head = null;
            size = 0;
        }

        /// <summary>
        /// Adds a new element to the tree
        /// </summary>
        public virtual void Add(T value)
        {
            var node = new BinaryTreeNode<T>(value);
            Add(node);
        }

        /// <summary>
        /// Adds a node to the tree
        /// </summary>
        public virtual void Add(BinaryTreeNode<T> node)
        {
            if (head == null) //first element being added
            {
                head = node; //set node as root of the tree
                node.Tree = this;
                size++;
            }
            else
            {
                if (node.Parent == null)
                {
                    node.Parent = head; //start at head
                }

                //Node is inserted on the left side if it is smaller or equal to the parent
                var matchValue = comparer(node.Value, node.Parent.Value);
                if (matchValue == 0)
                {
                    //this means that the mass was within the tolerance, we should have to update the frame and scans map for this peak

                    //at this node we have a MSResultPeakWithLocation, .
                    var peakWithLoc = node.Value as MSResultPeakWithLocation;
                }

                var insertLeftSide = matchValue < 0;

                if (insertLeftSide) //insert on the left
                {
                    if (node.Parent.LeftChild == null)
                    {
                        node.Parent.LeftChild = node; //insert in left
                        size++;
                        node.Tree = this; //assign node to this binary tree
                    }
                    else
                    {
                        node.Parent = node.Parent.LeftChild; //scan down to left child
                        Add(node); //recursive call
                    }
                }
                else //insert on the right
                {
                    if (node.Parent.RightChild == null)
                    {
                        node.Parent.RightChild = node; //insert in right
                        size++;
                        node.Tree = this; //assign node to this binary tree
                    }
                    else
                    {
                        node.Parent = node.Parent.RightChild;
                        Add(node);
                    }
                }
            }
        }

        public BinaryTreeNode<T> FindFeatureWithGivenMass(double massValue, int toleranceInPPM)
        {
            var node = head;

            while (node != null)
            {
                if (!(node.Value is Peak thisFeature))
                {
                    return null;
                }

                var ppmDifference = 1000000 * (massValue - thisFeature.XValue) / massValue;
                var absolutePPMDifference = Math.Abs(ppmDifference);

                //if (node.Value.Equals(value)) //parameter value found
                if (absolutePPMDifference <= toleranceInPPM)
                {
                    return node;
                }

                if (ppmDifference < 0)
                {
                    //search left
                    node = node.LeftChild;
                }
                else
                {
                    node = node.RightChild;
                }
            }

            return null;
        }

        public bool FindPeakWithinFeatures(Peak value, int frameNum, int scanNum, int toleranceInPPM, int netTolRange, int driftTolRange)
        {
            var node = head;
            while (node != null)
            {
                try
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (!(node.Value is MSResultPeakWithLocation thisFeature))
                    {
                        Console.WriteLine("BinarySearchTree: The node is not of type MSResultPeakWithLocation");
                        continue;
                    }

                    //now check if the peak value is within the mass tolerance of this UMC
                    //int number = thisFeature.ContainsPeak(value, (ushort)frameNum, (ushort)scanNum, (ushort)toleranceInPPM, (ushort)netTolRange, (ushort)driftTolRange);
                    var hasMass = thisFeature.ContainsMass(value.XValue, toleranceInPPM);

                    if (hasMass)
                    {
                        var number = thisFeature.ContainsPeak(value, (ushort)frameNum, (ushort)scanNum, (ushort)toleranceInPPM, (ushort)netTolRange, (ushort)driftTolRange);

                        if (number == 0)
                        {
                            //we've found a node that contains that feature value
                            return true;
                        }
                    }

                    if (value.XValue < thisFeature.XValue)
                    {
                        node = node.LeftChild;
                    }
                    else
                    {
                        node = node.RightChild;
                    }
                }
                catch (InvalidCastException invalidCast)
                {
                    Console.WriteLine("BinarySearchTree: The node is not of type MSResultPeakWithLocation");
                    Console.WriteLine(invalidCast.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the first node in the tree with the parameter value.
        /// </summary>
        public virtual BinaryTreeNode<T> Find(T value)
        {
            var node = head; //start at head
            while (node != null)
            {
                try
                {
                    if (!(node.Value is Peak msPeak1) || !(value is Peak msPeak2))
                    {
                        continue;
                    }

                    var ppmDifference = 1000000 * (msPeak1.XValue - msPeak2.XValue) / msPeak1.XValue;
                    var absolutePPMDifference = Math.Abs(ppmDifference);

                    //if (node.Value.Equals(value)) //parameter value found
                    if (absolutePPMDifference <= ToleranceInPPM)
                    {
                        return node;
                    }

                    //Search left if the value is smaller than the current node

                    if (ppmDifference < 0)
                    {
                        node = node.LeftChild; //search left
                    }
                    else
                    {
                        node = node.RightChild; //search right
                    }
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }
            return null; //not found
        }

        /// <summary>
        /// Returns whether a value is stored in the tree
        /// </summary>
        public virtual bool Contains(T value)
        {
            return (Find(value) != null);
        }

        /// <summary>
        /// Removes a value from the tree and returns whether the removal was successful.
        /// </summary>
        public virtual bool Remove(T value)
        {
            var removeNode = Find(value);

            return Remove(removeNode);
        }

        /// <summary>
        /// Removes a node from the tree and returns whether the removal was successful.
        /// </summary>>
        public virtual bool Remove(BinaryTreeNode<T> removeNode)
        {
            if (removeNode == null || removeNode.Tree != this)
            {
                return false; //value doesn't exist or not of this tree
            }

            //Note whether the node to be removed is the root of the tree
            var wasHead = (removeNode == head);

            if (Count == 1)
            {
                head = null; //only element was the root
                removeNode.Tree = null;

                size--; //decrease total element count
            }
            else if (removeNode.IsLeaf) //Case 1: No Children
            {
                //Remove node from its parent
                if (removeNode.IsLeftChild)
                {
                    removeNode.Parent.LeftChild = null;
                }
                else
                {
                    removeNode.Parent.RightChild = null;
                }

                removeNode.Tree = null;
                removeNode.Parent = null;

                size--; //decrease total element count
            }
            else if (removeNode.ChildCount == 1) //Case 2: One Child
            {
                if (removeNode.HasLeftChild)
                {
                    //Put left child node in place of the node to be removed
                    removeNode.LeftChild.Parent = removeNode.Parent; //update parent

                    if (wasHead)
                    {
                        Root = removeNode.LeftChild; //update root reference if needed
                    }

                    if (removeNode.IsLeftChild) //update the parent's child reference
                    {
                        removeNode.Parent.LeftChild = removeNode.LeftChild;
                    }
                    else
                    {
                        removeNode.Parent.RightChild = removeNode.LeftChild;
                    }
                }
                else //Has right child
                {
                    //Put left node in place of the node to be removed
                    removeNode.RightChild.Parent = removeNode.Parent; //update parent

                    if (wasHead)
                    {
                        Root = removeNode.RightChild; //update root reference if needed
                    }

                    if (removeNode.IsLeftChild) //update the parent's child reference
                    {
                        removeNode.Parent.LeftChild = removeNode.RightChild;
                    }
                    else
                    {
                        removeNode.Parent.RightChild = removeNode.RightChild;
                    }
                }

                removeNode.Tree = null;
                removeNode.Parent = null;
                removeNode.LeftChild = null;
                removeNode.RightChild = null;

                size--; //decrease total element count
            }
            else //Case 3: Two Children
            {
                //Find in order predecessor (right-most node in left subtree)
                var successorNode = removeNode.LeftChild;
                while (successorNode.RightChild != null)
                {
                    successorNode = successorNode.RightChild;
                }

                removeNode.Value = successorNode.Value; //replace value

                Remove(successorNode); //recursively remove the in order predecessor
            }

            return true;
        }

        /// <summary>
        /// Removes all the elements stored in the tree
        /// </summary>
        public virtual void Clear()
        {
            //Remove children first, then parent
            //(Post-order traversal)
            var enumerator = GetPostOrderEnumerator();
            while (enumerator.MoveNext())
            {
                Remove(enumerator.Current);
            }
            enumerator.Dispose();
        }

        /// <summary>
        /// Returns the height of the entire tree
        /// </summary>
        public virtual int GetHeight()
        {
            return GetHeight(Root);
        }

        /// <summary>
        /// Returns the height of the subtree rooted at the parameter value
        /// </summary>
        public virtual int GetHeight(T value)
        {
            //Find the value's node in tree
            var valueNode = Find(value);
            if (value != null)
            {
                return GetHeight(valueNode);
            }

            return 0;
        }

        /// <summary>
        /// Returns the height of the subtree rooted at the parameter node
        /// </summary>
        public virtual int GetHeight(BinaryTreeNode<T> startNode)
        {
            if (startNode == null)
            {
                return 0;
            }

            return 1 + Math.Max(GetHeight(startNode.LeftChild), GetHeight(startNode.RightChild));
        }

        /// <summary>
        /// Returns the depth of a subtree rooted at the parameter value
        /// </summary>
        public virtual int GetDepth(T value)
        {
            var node = Find(value);
            return GetDepth(node);
        }

        /// <summary>
        /// Returns the depth of a subtree rooted at the parameter node
        /// </summary>
        public virtual int GetDepth(BinaryTreeNode<T> startNode)
        {
            var depth = 0;

            if (startNode == null)
            {
                return depth;
            }

            var parentNode = startNode.Parent; //start a node above
            while (parentNode != null)
            {
                depth++;
                parentNode = parentNode.Parent; //scan up towards the root
            }

            return depth;
        }

        /// <summary>
        /// Returns an enumerator to scan through the elements stored in tree.
        /// The enumerator uses the traversal set in the TraversalMode property.
        /// </summary>
        public virtual IEnumerator<T> GetEnumerator()
        {
            switch (TraversalOrder)
            {
                case TraversalMode.InOrder:
                    return GetInOrderEnumerator();
                case TraversalMode.PostOrder:
                    return GetPostOrderEnumerator();
                case TraversalMode.PreOrder:
                    return GetPreOrderEnumerator();
                default:
                    return GetInOrderEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator to scan through the elements stored in tree.
        /// The enumerator uses the traversal set in the TraversalMode property.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that visits node in the order: left child, parent, right child
        /// </summary>
        public virtual IEnumerator<T> GetInOrderEnumerator()
        {
            return new BinaryTreeInOrderEnumerator(this);
        }

        /// <summary>
        /// Returns an enumerator that visits node in the order: left child, right child, parent
        /// </summary>
        public virtual IEnumerator<T> GetPostOrderEnumerator()
        {
            return new BinaryTreePostOrderEnumerator(this);
        }

        /// <summary>
        /// Returns an enumerator that visits node in the order: parent, left child, right child
        /// </summary>
        public virtual IEnumerator<T> GetPreOrderEnumerator()
        {
            return new BinaryTreePreOrderEnumerator(this);
        }

        /// <summary>
        /// Copies the elements in the tree to an array using the traversal mode specified.
        /// </summary>
        public virtual void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the elements in the tree to an array using the traversal mode specified.
        /// </summary>
        public virtual void CopyTo(T[] array, int startIndex)
        {
            var enumerator = GetEnumerator();

            for (var i = startIndex; i < array.Length; i++)
            {
                if (enumerator.MoveNext())
                {
                    array[i] = enumerator.Current;
                }
                else
                {
                    break;
                }
            }

            enumerator.Dispose();
        }

        /// <summary>
        /// Compares two elements to determine their positions within the tree.
        /// </summary>
        public static int CompareElements(IComparable x, IComparable y)
        {
            return x.CompareTo(y);
        }

        /// <summary>
        /// Returns an in order-traversal enumerator for the tree values
        /// </summary>
        internal class BinaryTreeInOrderEnumerator : IEnumerator<T>
        {
            private BinaryTreeNode<T> current;
            private BinaryTree<T> tree;
            internal Queue<BinaryTreeNode<T>> traverseQueue;

            public BinaryTreeInOrderEnumerator(BinaryTree<T> tree)
            {
                this.tree = tree;

                //Build queue
                traverseQueue = new Queue<BinaryTreeNode<T>>();
                visitNode(this.tree.Root);
            }

            private void visitNode(BinaryTreeNode<T> node)
            {
                if (node == null)
                {
                    return;
                }

                visitNode(node.LeftChild);
                traverseQueue.Enqueue(node);
                visitNode(node.RightChild);
            }

            public T Current => current.Value;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                current = null;
                tree = null;
            }

            public void Reset()
            {
                current = null;
            }

            public bool MoveNext()
            {
                if (traverseQueue.Count > 0)
                {
                    current = traverseQueue.Dequeue();
                }
                else
                {
                    current = null;
                }

                return (current != null);
            }
        }

        /// <summary>
        /// Returns a post order-traversal enumerator for the tree values
        /// </summary>
        internal class BinaryTreePostOrderEnumerator : IEnumerator<T>
        {
            private BinaryTreeNode<T> current;
            private BinaryTree<T> tree;
            internal Queue<BinaryTreeNode<T>> traverseQueue;

            public BinaryTreePostOrderEnumerator(BinaryTree<T> tree)
            {
                this.tree = tree;

                //Build queue
                traverseQueue = new Queue<BinaryTreeNode<T>>();
                visitNode(this.tree.Root);
            }

            private void visitNode(BinaryTreeNode<T> node)
            {
                if (node == null)
                {
                    return;
                }

                visitNode(node.LeftChild);
                visitNode(node.RightChild);
                traverseQueue.Enqueue(node);
            }

            public T Current => current.Value;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                current = null;
                tree = null;
            }

            public void Reset()
            {
                current = null;
            }

            public bool MoveNext()
            {
                if (traverseQueue.Count > 0)
                {
                    current = traverseQueue.Dequeue();
                }
                else
                {
                    current = null;
                }

                return (current != null);
            }
        }

        /// <summary>
        /// Returns an pre order-traversal enumerator for the tree values
        /// </summary>
        internal class BinaryTreePreOrderEnumerator : IEnumerator<T>
        {
            private BinaryTreeNode<T> current;
            private BinaryTree<T> tree;
            internal Queue<BinaryTreeNode<T>> traverseQueue;

            public BinaryTreePreOrderEnumerator(BinaryTree<T> tree)
            {
                this.tree = tree;

                //Build queue
                traverseQueue = new Queue<BinaryTreeNode<T>>();
                visitNode(this.tree.Root);
            }

            private void visitNode(BinaryTreeNode<T> node)
            {
                if (node == null)
                {
                    return;
                }

                traverseQueue.Enqueue(node);
                visitNode(node.LeftChild);
                visitNode(node.RightChild);
            }

            public T Current => current.Value;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                current = null;
                tree = null;
            }

            public void Reset()
            {
                current = null;
            }

            public bool MoveNext()
            {
                if (traverseQueue.Count > 0)
                {
                    current = traverseQueue.Dequeue();
                }
                else
                {
                    current = null;
                }

                return (current != null);
            }
        }
    }
}
