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
        node.Balance += balanceOffset;

        if (node.Balance == 0)
          return;

        if (node.Balance == -2) //a
        {
          node.Right.Left = node;
          node.Right.Parent = node.Parent;
          node.Parent = node.Right;
          node.Right = null;

          if (node.Parent.Parent == null)
            _rootNode = node.Parent;

          node.Balance = 0;
          node.Parent.Balance = 0;
          return;
        }

        if (node.Balance == 2)
        {
          node.Left.Right = node;
          node.Left.Parent = node.Parent;
          node.Parent = node.Left;
          node.Left = null;

          if (node.Parent.Parent == null)
            _rootNode = node.Parent;

          node.Balance = 0;
          node.Parent.Balance = 0;
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

    [DebuggerDisplay("{Value}")]
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