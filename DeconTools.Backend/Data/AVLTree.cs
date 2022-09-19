//====================================================
//| Downloaded From                                  |
//| Visual C# Kicks - http://www.vcskicks.com/       |
//| License - http://www.vcskicks.com/license.html   |
//====================================================

using System;
#pragma warning disable 618

namespace DeconTools.Backend.Data.Structures
{
    [Obsolete("Unused")]
    public class AVLTreeNode<T> : BinaryTreeNode<T>
      where T : IComparable
    {
        public AVLTreeNode(T value)
            : base(value)
        {
        }

        public new AVLTreeNode<T> LeftChild
        {
            get => (AVLTreeNode<T>)base.LeftChild;
            set => base.LeftChild = value;
        }

        public new AVLTreeNode<T> RightChild
        {
            get => (AVLTreeNode<T>)base.RightChild;
            set => base.RightChild = value;
        }

        public new AVLTreeNode<T> Parent
        {
            get => (AVLTreeNode<T>)base.Parent;
            set => base.Parent = value;
        }
    }

    /// <summary>
    /// AVL Tree data structure
    /// </summary>
    public class AVLTree<T> : BinaryTree<T>
        where T : IComparable
    {
        /// <summary>
        /// Returns the AVL Node of the tree
        /// </summary>
        public new AVLTreeNode<T> Root
        {
            get => (AVLTreeNode<T>)base.Root;
            set => base.Root = value;
        }

        /// <summary>
        /// Returns the AVL Node corresponding to the given value
        /// </summary>
        public new AVLTreeNode<T> Find(T value)
        {
            return (AVLTreeNode<T>)base.Find(value);
        }

        /// <summary>
        /// Insert a value in the tree and rebalance the tree if necessary.
        /// </summary>
        public override void Add(T value)
        {
            var node = new AVLTreeNode<T>(value);

            base.Add(node); //add normally

            //Balance every node going up, starting with the parent
            var parentNode = node.Parent;

            while (parentNode != null)
            {
                var balance = getBalance(parentNode);
                if (Math.Abs(balance) == 2) //-2 or 2 is unbalanced
                {
                    // Rebalance tree
                    balanceAt(parentNode, balance);
                }

                parentNode = parentNode.Parent; //keep going up
            }
        }

        /// <summary>
        /// Removes a given value from the tree and rebalances the tree if necessary.
        /// </summary>
        public override bool Remove(T value)
        {
            var valueNode = Find(value);
            return Remove(valueNode);
        }

        /// <summary>
        /// Wrapper method for removing a node within the tree
        /// </summary>
        protected new bool Remove(BinaryTreeNode<T> removeNode)
        {
            return Remove((AVLTreeNode<T>)removeNode);
        }

        /// <summary>
        /// Removes a given node from the tree and rebalances the tree if necessary.
        /// </summary>
        public bool Remove(AVLTreeNode<T> valueNode)
        {
            //Save reference to the parent node to be removed
            var parentNode = valueNode.Parent;

            //Remove the node as usual
            var removed = base.Remove(valueNode);

            if (!removed)
            {
                return false; //removing failed, no need to rebalance
            }

            //Balance going up the tree
            while (parentNode != null)
            {
                var balance = getBalance(parentNode);

                if (Math.Abs(balance) == 1) //1, -1
                {
                    break; //height hasn't changed, can stop
                }

                if (Math.Abs(balance) == 2) //2, -2
                {
                    //Rebalance tree
                    balanceAt(parentNode, balance);
                }

                parentNode = parentNode.Parent;
            }

            return true;
        }

        /// <summary>
        /// Balances an AVL Tree node
        /// </summary>
        protected virtual void balanceAt(AVLTreeNode<T> node, int balance)
        {
            if (balance == 2) //right outweighs
            {
                var rightBalance = getBalance(node.RightChild);

                if (rightBalance == 1 || rightBalance == 0)
                {
                    //Left rotation needed
                    rotateLeft(node);
                }
                else if (rightBalance == -1)
                {
                    //Right rotation needed
                    rotateRight(node.RightChild);

                    //Left rotation needed
                    rotateLeft(node);
                }
            }
            else if (balance == -2) //left outweighs
            {
                var leftBalance = getBalance(node.LeftChild);
                if (leftBalance == 1)
                {
                    //Left rotation needed
                    rotateLeft(node.LeftChild);

                    //Right rotation needed
                    rotateRight(node);
                }
                else if (leftBalance == -1 || leftBalance == 0)
                {
                    //Right rotation needed
                    rotateRight(node);
                }
            }
        }

        /// <summary>
        /// Determines the balance of a given node
        /// </summary>
        protected virtual int getBalance(AVLTreeNode<T> root)
        {
            //Balance = right child's height - left child's height
            return GetHeight(root.RightChild) - GetHeight(root.LeftChild);
        }

        /// <summary>
        /// Rotates a node to the left within an AVL Tree
        /// </summary>
        protected virtual void rotateLeft(AVLTreeNode<T> root)
        {
            var pivot = root?.RightChild;

            if (pivot == null)
            {
                return;
            }

            var rootParent = root.Parent; //original parent of root node
            var isLeftChild = (rootParent != null) && rootParent.LeftChild == root; //whether the root was the parent's left node
            var makeTreeRoot = root.Tree.Root == root; //whether the root was the root of the entire tree

            //Rotate
            root.RightChild = pivot.LeftChild;
            pivot.LeftChild = root;

            //Update parents
            root.Parent = pivot;
            pivot.Parent = rootParent;

            if (root.RightChild != null)
            {
                root.RightChild.Parent = root;
            }

            //Update the entire tree's Root if necessary
            if (makeTreeRoot)
            {
                pivot.Tree.Root = pivot;
            }

            //Update the original parent's child node
            if (isLeftChild)
            {
                rootParent.LeftChild = pivot;
            }
            else
            if (rootParent != null)
            {
                rootParent.RightChild = pivot;
            }
        }

        /// <summary>
        /// Rotates a node to the right within an AVL Tree
        /// </summary>
        protected virtual void rotateRight(AVLTreeNode<T> root)
        {
            var pivot = root?.LeftChild;

            if (pivot == null)
            {
                return;
            }

            var rootParent = root.Parent; //original parent of root node
            var isLeftChild = (rootParent != null) && rootParent.LeftChild == root; //whether the root was the parent's left node
            var makeTreeRoot = root.Tree.Root == root; //whether the root was the root of the entire tree

            //Rotate
            root.LeftChild = pivot.RightChild;
            pivot.RightChild = root;

            //Update parents
            root.Parent = pivot;
            pivot.Parent = rootParent;

            if (root.LeftChild != null)
            {
                root.LeftChild.Parent = root;
            }

            //Update the entire tree's Root if necessary
            if (makeTreeRoot)
            {
                pivot.Tree.Root = pivot;
            }

            //Update the original parent's child node
            if (isLeftChild)
            {
                rootParent.LeftChild = pivot;
            }
            else
            if (rootParent != null)
            {
                rootParent.RightChild = pivot;
            }
        }
    }
}
