using System;
using System.Diagnostics;
using System.Web.UI;

namespace statsd.net.shared
{
  public class BinaryStringTree
  {
    private Node _rootNode;

    public Node RootNode => _rootNode;
    public BinaryStringTree()
    {
      
    }

    public void Insert(string value)
    {
      var currentNode = _rootNode;

      while (currentNode != null)
      {
        var compareResult = String.Compare(value, currentNode.Value, StringComparison.InvariantCultureIgnoreCase);

        if (compareResult < 0)
        {
          if (currentNode.Left == null)
          {
            currentNode.Left = new Node {Parent = currentNode, Value = value};
            Balance(currentNode, 1);
            return;
          }
          currentNode = currentNode.Left;
        }
        else if (compareResult > 0)
        {
          if (currentNode.Right == null)
          {
            currentNode.Right = new Node() {Parent = currentNode, Value = value};
            Balance(currentNode, -1);
            return;
          }
          currentNode = currentNode.Right;
        }
        else
        {
          // duplicate value, nothing to do
          return;
        }
      }

      _rootNode = new Node()
      {
        Value = value
      };
    }

    private void Balance(Node node, sbyte balanceOffset)
    {
      while (node != null)
      {
        // update current node balance
        node.Balance += balanceOffset;

        if (node.Balance == 0)
          return;

        //Right Unbalance
        if (node.Balance == -2)
        {
          // Left Right Rotate
          // A      A
          //  C ->   B
          // B        C
          if (node.Right.Balance == 1)
            RotateLeft(node.Right);

          // Left Rotate
          // A       B
          //  B  -> A C
          //   C
          RotateLeft(node);
          return;
        }

        // Left Unbalance
        if (node.Balance == 2)
        {
          // Right Left Rotate
          //  C      C
          // A  ->  B
          //  B    A
          if (node.Left.Balance == -1)
            RotateRight(node.Left);

          // Right Rotate
          //   C      B
          //  B   -> A C
          // A
          RotateRight(node);
          return;
        }

        if (node.Parent != null)
        {
          if (node == node.Parent.Left)
            balanceOffset = 1;
          else
            balanceOffset = -1;
        }

        node = node.Parent;
      }
    }

    private void RotateLeft(Node node)
    {
      // Left Rotate
      // A       B
      //  B  -> A C
      //   C
      var parent = node.Parent;
      var a = node;
      var b = a.Right;

      if (parent == null)
        _rootNode = b;
      else
        parent.Right = b;
      b.Parent = parent;

      a.Right = b.Left;
      if (a.Right != null)
        a.Right.Parent = a;

      b.Left = a;
      b.Left.Parent = b;

      b.Balance++;
      a.Balance = -b.Balance;
    }

    private void RotateRight(Node node)
    {
      // Right Rotate
      //   C      B
      //  B   -> A C
      // A
      var parent = node.Parent;
      var c = node;
      var b = c.Left;

      if (parent == null)
        _rootNode = b;
      else
        parent.Left = b;
      b.Parent = parent;

      c.Left = b.Right;
      if (c.Left != null)
        c.Left.Parent = c;

      b.Right = c;
      b.Right.Parent = b;

      b.Balance--;
      c.Balance = -b.Balance;
    }

    public void Delete(string value)
    {
      throw new NotImplementedException();
    }

    public bool Match(string value)
    {
      throw new NotImplementedException();
    }

    public string MatchNearest(string value)
    {
      throw new NotImplementedException();
    }

    [DebuggerDisplay("{Value} {Balance}")]
    public class Node
    {
      public string Value;
      public int Balance;
      public Node Parent;
      public Node Left;
      public Node Right;
    }
  }
}