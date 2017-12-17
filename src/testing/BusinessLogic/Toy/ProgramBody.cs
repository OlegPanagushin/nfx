﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using NFX;
using NFX.IO;
using NFX.ApplicationModel;
using NFX.Wave;

namespace BusinessLogic.Toy
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        var w = Stopwatch.StartNew();
        run(args);
        w.Stop();
        ConsoleUtils.Info("Runtime: "+w.Elapsed);
        Environment.ExitCode = 0;
      }
      catch(Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        Console.WriteLine("------------------------------------------");
        ConsoleUtils.Error(error.StackTrace);
        Environment.ExitCode = -1;
      }
    }

    public static void run(string[] args)
    {
      ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Welcome.txt") );
      ConsoleUtils.Info("Running on a `{0}` platform".Args(NFX.PAL.PlatformAbstractionLayer.PlatformName));

      Console.WriteLine("Init app container...");

      //////System.Threading.ThreadPool.SetMaxThreads(1000, 1000);


      using(var app = new ServiceBaseApplication(args, null))
      {
        Console.WriteLine("...app container is up");

        var cmd = app.CommandArgs.AttrByIndex(0).Value;

        if (app.CommandArgs["?"].Exists ||
            app.CommandArgs["h"].Exists ||
            app.CommandArgs["help"].Exists)
        {
            ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Help.txt") );
            return;
        }

        ConsoleUtils.Info("Glue servers:");
        var any = false;
        foreach(var s in App.Glue.Servers)
        {
          Console.WriteLine("    " + s);
          any = true;
        }
        if (!any) Console.WriteLine("  - No servers");
        Console.WriteLine();

        if (cmd.EqualsOrdIgnoreCase("machine"))
        {
          MachineSpeed.CRC32();
        }
        else if (cmd.EqualsOrdIgnoreCase("slim"))
        {
          SlimSpeed.SerDeserSimpleObject();
          SlimSpeed.SerDeserPerson();
          SlimSpeed.SerDeserPersonArray();
        }
        else if (cmd.EqualsOrdIgnoreCase("echo"))
        {
          var node = app.CommandArgs.AttrByIndex(1).ValueAsString() ?? "sync://127.0.0.1:8000";
          var count = app.CommandArgs["c"].AttrByIndex(0).ValueAsInt(1000);//count of requests
          var par = app.CommandArgs["p"].AttrByIndex(0).ValueAsInt(1);//max degree of parallelism
         // GlueSpeed.Echo(node, count, par);
          GlueSpeed.EchoThreaded(node, count, par);
        }
        else if (cmd.EqualsOrdIgnoreCase("pile"))
        {
          var threads = app.CommandArgs["t"].AttrByIndex(0).ValueAsInt(4);
          var count = app.CommandArgs["c"].AttrByIndex(0).ValueAsInt(20_000_000);
          var size = app.CommandArgs["sz"].AttrByIndex(0).ValueAsInt(32);
          PileSpeed.ProfileByteArray(threads, count, size);
          PileSpeed.ProfileString(threads, count, size);
          PileSpeed.ProfileSimpleObject(threads, count);
          PileSpeed.ProfilePersonObject(threads, count);
        }
        else if (cmd.EqualsOrdIgnoreCase("wave"))
        {
          using(var ws = new WaveServer())
          {
            ws.Configure(null);

            ws.IgnoreClientWriteErrors = false;

            ws.Start();
            Console.WriteLine("Web server started");
            Console.WriteLine("Strike <ENTER> to terminate web server ");
            Console.ReadLine();
          }
        }


        Console.WriteLine("Strike <ENTER> to exit app");
        Console.ReadLine();
        Console.WriteLine("Shutting app container...");
      }
      Console.WriteLine("...app container is disposed");
    }
  }
}
