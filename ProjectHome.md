## <font color='red'>ATTENTION!</font> ##

<font size='3'>BackLINQ sources have been rolled and merged into the <a href='http://code.google.com/p/linqbridge/'>LINQBridge</a> project and maintained under its identity. As a result, this project site has been retired and left here for archival purposes. Please follow LINQBridge for further <a href='http://code.google.com/p/linqbridge/updates/list'>updates, contributions</a> and <a href='http://code.google.com/p/linqbridge/issues/list'>reporting of issues</a>.</font>

Oct 5, 2009


---


BackLINQ allows applications written in C# 3.0, but which need to target .NET Framework 2.0 and/or 3.x, to reap the benefits and expressivity of [LINQ to Objects](http://msdn.microsoft.com/en-us/library/bb397919.aspx) and lambda expressions.

## The Short of BackLINQ ##

Once you've gotten used to expressing queries on objects using LINQ, it's hard to go back and write them in an imperative and verbose style. Some folks may still need to target .NET Framework 2.0 and with the [multi-targeting capability of Visual Studio 2008](http://msdn.microsoft.com/en-us/library/bb398197.aspx), it is possible to continue to use C# 3.0 without requiring users to install .NET Framework 3.5 (which is required to use LINQ to Objects). This is where BackLINQ comes in.

### What Do I Get? ###

BackLINQ gives you:

  * A set of C# files that you can add to your project as part of your binaries; no need to add a reference to yet another library.
  * An [implementation](http://backlinq.googlecode.com/svn/trunk/src/Enumerable.cs) of [System.Linq.Enumerable](http://msdn.microsoft.com/en-us/library/system.linq.enumerable.aspx) and a few other supporting types that you can take with you.
  * An open source project where you can control your fate.