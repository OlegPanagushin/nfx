﻿using NFX.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using NFX.Environment;

namespace NFX.Scripting
{
  /// <summary>
  /// Hosts unit test runner in a console application. This host is NOT thread-safe
  /// </summary>
  public sealed class TestRunnerConsoleHost : DisposableObject, IRunnerHost
  {
    private Stopwatch  m_Stopwatch;
    private int m_TotalRunnables;
    private int m_TotalMethods;
    private int m_TotalOKs;
    private int m_TotalErrors;

    private FileConfiguration m_Out;

    [Config("$out|$file|$out-file")]
    public string OutFileName { get; set;}

    public int TotalRunnables => m_TotalRunnables;
    public int TotalMethods   => m_TotalMethods;
    public int TotalOKs        => m_TotalOKs;
    public int TotalErrors     => m_TotalErrors;


    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);

    public TextWriter ConsoleOut => Console.Out;
    public TextWriter ConsoleError => Console.Error;


    private string m_RunnableHeader;
    private bool m_HadRunnableMethods;
    private string m_PriorMethodName;
    private int m_PriorMethodCount;


    private ConfigSectionNode m_RunnableNode;

    public void BeginRunnable(FID id, object runnable)
    {
      m_TotalRunnables++;
      var t = runnable.GetType();
      m_RunnableHeader = "Starting {0}::{1}.{2} ...".Args(t.Assembly.GetName().Name, t.Namespace, t.DisplayNameWithExpandedGenericArgs());
      m_HadRunnableMethods = false;
      m_PriorMethodName = null;
      m_PriorMethodCount = 0;

      var o = m_Out?.Root;
      if (o!=null)
      {
        m_RunnableNode = o.AddChildNode("runnable", runnable.GetType().Name);
        m_RunnableNode.AddAttributeNode("id", id);
        m_RunnableNode.AddAttributeNode("type", runnable.GetType().AssemblyQualifiedName);
        m_RunnableNode.AddAttributeNode("now-loc", App.LocalizedTime);
        m_RunnableNode.AddAttributeNode("now-utc", App.TimeSource.UTCNow);
      }
    }

    public void EndRunnable(FID id, object runnable, Exception error)
    {
      if (m_RunnableNode!=null)
      {
         outError(m_RunnableNode, error);
      }

      if (error!=null)
      {
        m_TotalErrors++;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("EndRunnable caught: ");
        writeError(error);
        Console.ForegroundColor = ConsoleColor.Gray;
      }
      if (!m_HadRunnableMethods) return;
      Console.WriteLine("... done {0}".Args(runnable.GetType().DisplayNameWithExpandedGenericArgs()));
      Console.WriteLine();
      Console.WriteLine();
    }


    public void BeforeMethodRun(FID id, MethodInfo method, RunAttribute attr)
    {
      if (m_RunnableHeader!=null)
      {
        Console.WriteLine(m_RunnableHeader);
        m_RunnableHeader = null;
      }
      m_HadRunnableMethods =true;
      m_TotalMethods++;
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.Write("  - {0} ".Args(method.Name));
      if (attr.Name.IsNotNullOrWhiteSpace())
      {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("::");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("'{0}'".Args(attr.Name));
      }


      if (attr.ConfigContent.IsNotNullOrWhiteSpace())
      {

        try
        {
          Console.ForegroundColor = ConsoleColor.DarkCyan;
          Console.Write(" {0} ".Args( attr.Config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)
                                               .Remove(0, 1)
                                               .TakeFirstChars(128, "...")));
        }
        catch
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("<bad config>");
        }
      }

      if (method.Name == m_PriorMethodName)
      {
        m_PriorMethodCount++;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("[{0}] ".Args(m_PriorMethodCount));
      }
      else
       m_PriorMethodCount = 0;

      m_PriorMethodName = method.Name;

      Console.ForegroundColor = ConsoleColor.DarkGray;
    }

    public void AfterMethodRun(FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      Console.ForegroundColor = ConsoleColor.Gray;

      //check for Aver.Throws()
      try
      {
        var aversThrows = Aver.ThrowsAttribute.CheckMethodError(method, error);
        if (aversThrows) error =null;
      }
      catch(Exception err)
      {
        error = err;
      }

      var o = m_RunnableNode;
      if (o != null)
      {
        var nrun = o.AddChildNode("run", method.Name);
        nrun.AddAttributeNode("id", id);
        nrun.AddAttributeNode("now-loc", App.LocalizedTime);
        nrun.AddAttributeNode("now-utc", App.TimeSource.UTCNow);
        nrun.AddAttributeNode("OK", error==null);
        nrun.AddAttributeNode("run-name", attr.Name);
        nrun.AddAttributeNode("run-explicit", attr.ExplicitName);
        nrun.AddAttributeNode("run-config", attr.ConfigContent);

        outError(nrun, error);
      }


      var wasF = Console.ForegroundColor;
      if (error==null)
      {
        m_TotalOKs++;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[OK]");
      }
      else
      {
        m_TotalErrors++;
        writeError(error);
      }

      Console.ForegroundColor = wasF;
      Console.WriteLine();
    }



    public void Start()
    {
      m_Stopwatch = Stopwatch.StartNew();
      m_TotalRunnables = 0;
      m_TotalMethods = 0;
      m_TotalOKs =0;
      m_TotalErrors = 0;

      if (OutFileName.IsNotNullOrWhiteSpace())
      {
        m_Out = Configuration.MakeProviderForFile(OutFileName);
        m_Out.Create(this.GetType().FullName);
        m_Out.Root.AddAttributeNode("runtime", NFX.PAL.PlatformAbstractionLayer.PlatformName);
        m_Out.Root.AddAttributeNode("timestamp-local", App.LocalizedTime);
        m_Out.Root.AddAttributeNode("timestamp-utc", App.TimeSource.UTCNow);
        m_Out.Root.AddAttributeNode("user", System.Environment.UserName);
        m_Out.Root.AddAttributeNode("machine", System.Environment.MachineName);
        m_Out.Root.AddAttributeNode("os", OS.Computer.OSFamily);
        m_Out.Root.AddAttributeNode("cmd", System.Environment.CommandLine);
        m_Out.Root.AddAttributeNode("app-name", App.Name);
        m_Out.Root.AddAttributeNode("app-instance", App.InstanceID);
        m_Out.SaveAs(OutFileName);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Out file format: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("{0}".Args(m_Out.GetType()));

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Out file name: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("'{0}'".Args(OutFileName));
      }


      Console.ForegroundColor = ConsoleColor.White;
      Console.Write("Started ");
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine("{0}".Args(App.TimeSource.Now));
    }

    public void Summarize()
    {
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine("---------------------------------------------------------------------------");
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("Platform runtime: ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(NFX.PAL.PlatformAbstractionLayer.PlatformName);

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("Total runnables: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(m_TotalRunnables));

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("Total methods: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(m_TotalMethods));

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("Finished: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(App.TimeSource.Now));

      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("Runtime: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}".Args(m_Stopwatch.Elapsed));

      Console.WriteLine();

      Console.ForegroundColor = m_TotalOKs >0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
      Console.Write("   OK: {0}   ".Args(m_TotalOKs));
      Console.ForegroundColor = m_TotalErrors>0? ConsoleColor.Red : ConsoleColor.DarkGray;
      Console.WriteLine("ERROR: {0} ".Args(m_TotalErrors));

      if (OutFileName.IsNotNullOrWhiteSpace())
      {
        m_Out.SaveAs(OutFileName);
        Console.WriteLine();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Saved file: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("'{0}'".Args(OutFileName));
      }

      Console.ForegroundColor = ConsoleColor.Gray;
    }


    private void writeError(Exception error)
    {
      var nesting = 0;
      while (error!=null)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        if (nesting==0)
          Console.Write("[Error]");
        else
          Console.Write("[Error[{0}]]".Args(nesting));
        Console.ForegroundColor = error is ScriptingException ? ConsoleColor.Cyan : ConsoleColor.Magenta;
        Console.WriteLine(" "+error.ToMessageWithType());
        Console.WriteLine(error.StackTrace); //todo stack trace conditionaly
        Console.WriteLine();

        error = error.InnerException;
        nesting++;
      }
    }

    private void outError(ConfigSectionNode node, Exception error)
    {
      var nesting = 0;
      while (error!=null)
      {
        node = node.AddChildNode("error", error.GetType().Name);
        node.AddAttributeNode("type", error.GetType().AssemblyQualifiedName);
        node.AddAttributeNode("nesting", nesting);
        node.AddAttributeNode("msg", error.Message);
        node.AddAttributeNode("stack", error.StackTrace);

        error = error.InnerException;
        nesting++;
      }
    }


  }
}
