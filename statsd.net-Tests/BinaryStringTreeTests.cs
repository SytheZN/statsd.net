using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using statsd.net.shared;

namespace statsd.net_Tests
{
  [TestClass]
  public class BinaryStringTreeTests
  {
    [TestMethod]
    public void FirstInsertShouldSetRootNode()
    {
      var testValue = "test";

      var bst = new BinaryStringTree();
      bst.Insert(testValue);

      Assert.AreSame(bst.RootNode.Value, testValue);
    }

    [TestMethod]
    public void Comparison_A_ShouldBe_LeftOf_B()
    {
      var a = "a";
      var b = "b";

      var bst = new BinaryStringTree();
      bst.Insert(b);
      bst.Insert(a);

      Assert.AreSame(bst.RootNode.Left.Value, a);
    }

    [TestMethod]
    public void Comparison_B_ShouldBe_RightOf_A()
    {
      var a = "a";
      var b = "b";

      var bst = new BinaryStringTree();
      bst.Insert(a);
      bst.Insert(b);

      Assert.AreSame(bst.RootNode.Right.Value, b);
    }

    [TestMethod]
    public void Rotation_ABC_ShouldBe_RootB_LeftA_RightC()
    {
      var a = "a";
      var b = "b";
      var c = "c";

      var bst = new BinaryStringTree();
      bst.Insert(a);
      bst.Insert(b);
      bst.Insert(c);

      Assert.AreSame(bst.RootNode.Value,b);
      Assert.AreSame(bst.RootNode.Left.Value, a);
      Assert.AreSame(bst.RootNode.Right.Value, c);
    }

    [TestMethod]
    public void Rotation_CBA_ShouldBe_RootB_LeftA_RightC()
    {
      var a = "a";
      var b = "b";
      var c = "c";

      var bst = new BinaryStringTree();
      bst.Insert(c);
      bst.Insert(b);
      bst.Insert(a);

      Assert.AreSame(bst.RootNode.Value, b);
      Assert.AreSame(bst.RootNode.Left.Value, a);
      Assert.AreSame(bst.RootNode.Right.Value, c);
    }

    [TestMethod]
    public void Rotation_ABCDEF_ShouldBe_D_BF_ACEG()
    {
      var a = "a";
      var b = "b";
      var c = "c";
      var d = "d";
      var e = "e";
      var f = "f";
      var g = "g";

      var bst = new BinaryStringTree();
      bst.Insert(a);
      bst.Insert(b);
      bst.Insert(c);
      bst.Insert(d);
      bst.Insert(e);
      bst.Insert(f);
      bst.Insert(g);

      // given input a b c d e f g
      // output should be:
      //                           
      //        d                   
      //      b   f                  
      //     a c e g                   

      Assert.AreSame(bst.RootNode.Value, d);

      Assert.AreSame(bst.RootNode.Left.Value, b);
      Assert.AreSame(bst.RootNode.Right.Value, f);

      Assert.AreSame(bst.RootNode.Left.Left.Value, a);
      Assert.AreSame(bst.RootNode.Left.Right.Value, c);

      Assert.AreSame(bst.RootNode.Right.Left.Value, e);
      Assert.AreSame(bst.RootNode.Right.Right.Value, g);
    }

    [TestMethod]
    public void Balance_Given1023Nodes_ShouldBe_Depth10()
    {
      var bst = new BinaryStringTree();

      for (int i = 1; i < 1024; i++)
      {
        bst.Insert(i.ToString());
      }
    }

  }
}
