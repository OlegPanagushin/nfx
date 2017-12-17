﻿/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/


using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using NFX.Scripting;

using NFX.Environment;

using NFX.Log;
using LSVC = NFX.Log.LogService;
using TSLS = NFX.UTest.TestSyncLog;
using NFX.Log.Destinations;
using NFX.ApplicationModel;
using System.Threading;
using System.Reflection;

namespace NFX.UTest.Logging
{
    [Runnable]
    public class BasicsvcTests
    {
      public const string TEST_DIR = @"c:\NFX"; // TODO: Don't hard-code - get from environment

      [Run]
      public void CSVFileDestinationStartByCode()
      {
        var TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = TNAME + ".csv.log";

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        using (var svc = new LSVC())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.RegisterDestination(
            new CSVFileDestination(TNAME) { Path = TEST_DIR, FileName = FNAME });

          svc.Start();

          svc.Write(new Message{Text = "1 message"});
          svc.Write(new Message{Text = "2 message"});
          svc.Write(new Message{Text = "3 message"});
          svc.Write(new Message{Text = "4 message"});

          svc.WaitForCompleteStop();

          Aver.IsTrue(File.Exists(fname));
          Aver.AreEqual(4, File.ReadAllLines(fname).Length);
        }
      }

      [Run]
      public void CSVFileDestinationStartByConfig1()
      {
        var TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = TNAME + ".csv.log";
        const string DATE = "20131012";

        var xml= @"<log>
                      <destination type='NFX.Log.Destinations.CSVFileDestination, NFX'
                                   name='{0}'
                                   path='$(@~path)'
                                   filename='$(::now fmt=yyyyMMdd value={1})-$($name).csv.log'/>
                   </log>".Args(TNAME, DATE);

        var fname = Path.Combine(TEST_DIR, DATE + "-" + FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        using (var svc = new LSVC())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          var cfg = XMLConfiguration.CreateFromXML(xml);
          cfg.EnvironmentVarResolver = new Vars ( new VarsDictionary{ { "path", TEST_DIR } } );
          svc.Configure(cfg.Root);

          svc.Start();

          svc.Write(new Message{Text = "1 message"});
          svc.Write(new Message{Text = "2 message"});
          svc.Write(new Message{Text = "3 message"});

          svc.WaitForCompleteStop();

          Aver.IsTrue(File.Exists(fname));
          Aver.AreEqual(3, File.ReadAllLines(fname).Length);
        }
      }

      [Run]
      public void CSVFileDestinationStartByConfig2()
      {
        var TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = TNAME + ".csv.log";
        var xml= @"<log>
                      <destination  type='NFX.Log.Destinations.CSVFileDestination, NFX'
                                    name='{0}' path='{1}' file-name='$($name).csv.log'
                      />
                   </log>".Args(TNAME, TEST_DIR);

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        using (var svc = new LSVC())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);

          svc.Start();

          svc.Write(new Message{Text = "1 message"});
          svc.Write(new Message{Text = "2 message"});
          svc.Write(new Message{Text = "3 message"});

          svc.WaitForCompleteStop();

          Aver.IsTrue(File.Exists(fname));
          Aver.AreEqual(3, File.ReadAllLines(fname).Length);
        }
      }

      [Run]
      public void CSVFileDestinationStartByLaConfig()
      {
        var TNAME = "UnitTest-" + MethodInfo.GetCurrentMethod().Name;
        var FNAME = TNAME + ".csv.log";
        const string DATE = "20131012";

        var laStr = @"log
                      {{
                        destination
                        {{
                          type='NFX.Log.Destinations.CSVFileDestination, NFX'
                          name='{0}'
                          path='$(@~path)'
                          file-name='$(::now fmt=yyyyMMdd value={1})-$($name).csv.log'
                        }}
                      }}".Args(TNAME, DATE);

        var cnf = LaconicConfiguration.CreateFromString(laStr);
        cnf.EnvironmentVarResolver = new Vars( new VarsDictionary { { "path", TEST_DIR}} );

        var fname = Path.Combine(TEST_DIR, DATE + "-" + FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        using (var svc = new LSVC())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.Configure(cnf.Root);

          svc.Start();

          svc.Write(new Message() { Text = "Msg 1"});
          svc.Write(new Message() { Text = "Msg 2" });
          svc.Write(new Message() { Text = "Msg 3" });

          svc.WaitForCompleteStop();

          Aver.IsTrue(File.Exists(fname));
          Aver.AreEqual(3, File.ReadAllLines(fname).Length);
        }
      }

      [Run]
      public void FloodFilter()
      {
        var TNAME = "UnitTest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = TNAME + ".csv.log";

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        var svc = new LSVC();
        try
        {
          svc.RegisterDestination(
            new FloodFilter(new CSVFileDestination(TNAME) {Path = TEST_DIR, FileName = FNAME })
            {
              IntervalSec = 10,
              MaxCount = 1000,
              MaxTextLength = 1024
            }
          );

          svc.Start();

          for (var i=0; i < 100000; i++)
            svc.Write(new Message{Text = i.ToString() +" message"});

          svc.WaitForCompleteStop();

          Aver.IsTrue(File.Exists(fname));
          Aver.AreEqual(1, File.ReadAllLines(fname).Length);
          Aver.IsTrue(new FileInfo(fname).Length < 1500);
        }
        finally
        {
          File.Delete(fname);
        }
      }

      [Run]
      public void CSVDestinationFilenameDefaultTest()
      {
        var TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = TNAME + ".csv";

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.RegisterDestination(
            new CSVFileDestination(TNAME) { Path = TEST_DIR, FileName = FNAME });
          svc.Start();

          Aver.IsTrue(File.Exists(fname));

          svc.WaitForCompleteStop();
        }
      }

      [Run]
      public void CSVDestinationFilenameConfigTest()
      {
        DateTime now = App.LocalizedTime;

        var TNAME = "TestDest" + MethodBase.GetCurrentMethod().Name;
        var FNAME = "{0}-{1:yyyyMMdd}{2}".Args(TNAME, now, ".csv");

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        var xml= @"<log>
                        <destination type='NFX.Log.Destinations.CSVFileDestination, NFX'
                          name='{0}' path='{1}' file-name='{2}'/>
                    </log>".Args(TNAME, TEST_DIR, FNAME);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);
          svc.Start();

          Aver.IsTrue(File.Exists(fname));

          svc.WaitForCompleteStop();
        }
      }

      [Run]
      public void DebugDestinationFilenameDefaultTest()
      {
        var TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = Path.Combine(TEST_DIR, TNAME + ".log");

        IOMiscUtils.EnsureFileEventuallyDeleted(FNAME);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(FNAME)))
        {
          svc.RegisterDestination(new DebugDestination(TNAME) { FileName = FNAME });
          svc.Start();

          Aver.IsTrue(File.Exists(FNAME));

          svc.WaitForCompleteStop();
        }
      }

      [Run]
      public void FileDestinationFilenameConfigTest()
      {
        DateTime now = App.LocalizedTime;

        var TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = "{0:yyyyMMdd}-{1}.log".Args(now, TNAME);

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        var xml= @"<log>
                     <destination type='NFX.Log.Destinations.DebugDestination, NFX'
                                  name='{0}'
                                  path='{1}'
                                  filename='{2}'/>
                   </log>".Args(TNAME, TEST_DIR, FNAME);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);
          svc.Start();

          Aver.IsTrue(File.Exists(fname));

          svc.WaitForCompleteStop();
        }
      }

      [Run]
      public void CSVDestinationDefaultFilenameTest()
      {
        var FNAME = "{0:yyyyMMdd}.log.csv".Args(App.LocalizedTime);

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(fname);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.RegisterDestination(
            new CSVFileDestination() {Path = TEST_DIR});
          svc.Start();

          Aver.IsTrue(File.Exists(fname));

          svc.WaitForCompleteStop();
        }
      }

      [Run]
      public void DebugDestinationWriteTest()
      {
        var TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = TNAME + ".log";

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(FNAME);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.RegisterDestination(
            new DebugDestination(TNAME) { FileName = FNAME, Path = TEST_DIR });
          svc.Start();

          DateTime now = new DateTime(2013, 1, 2, 3, 4, 5);

          for (var i=0; i < 10; i++)
            svc.Write(new Message { Text = i.ToString(), TimeStamp = now });

          svc.WaitForCompleteStop();

          Aver.IsTrue(File.Exists(fname));
          string[] lines = File.ReadAllLines(fname);

          Aver.AreEqual(10, lines.Length);
          lines.Select((s, i) =>
          {
            Aver.AreEqual(
              "20130102-030405||Debug|{0}||0|".Args(i),
              s);
            return 0;
          }).ToArray();
        }
      }

      [Run]
      public void LogLevelsTest()
      {
        DateTime now  = App.LocalizedTime;
        DateTime time = new DateTime(now.Year, now.Month, now.Day, 3, 4, 5);

        var TNAME = "TestDest-" + MethodBase.GetCurrentMethod().Name;
        var FNAME = "{0:yyyyMMdd}-{1}.log".Args(now, TNAME);

        var fname = Path.Combine(TEST_DIR, FNAME);
        IOMiscUtils.EnsureFileEventuallyDeleted(FNAME);

        var xml= @"<log>
                        <destination type='NFX.Log.Destinations.DebugDestination, NFX'
                            name='{0}' path='{1}' file-name='{2}'
                            levels='DebugB-DebugC,InfoB-InfoD,Warning,Emergency'/>
                    </log>".Args(TNAME, TEST_DIR, FNAME);

        using (var svc = new TSLS())
        using (Scope.OnExit(() => File.Delete(fname)))
        {
          svc.Configure(XMLConfiguration.CreateFromXML(xml).Root);
          svc.Start();

          Aver.IsTrue(File.Exists(fname));

          Array mts = Enum.GetValues(typeof(MessageType));

          foreach (var mt in mts)
            svc.Write(new Message
                      {
                        Type = (MessageType)mt,
                        Text = ((int)mt).ToString(),
                        TimeStamp = time
                      });

          svc.WaitForCompleteStop();

          string[] lines = File.ReadAllLines(fname);

          Aver.AreEqual(7, lines.Length);

          lines.Select((s,i) =>
          {
            var sa = s.Split('|');
            MessageType mt;

            Aver.IsTrue(Enum.TryParse(sa[3], out mt));
            Aver.AreEqual(
              "{0:yyyyMMdd}-030405||{1}|{2}||0|".Args(now, mt.ToString(), (int)mt),
              s);
            return 0;
          }).ToArray();
        }

        Aver.IsTrue(
          (new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.DebugA, MessageType.DebugZ) }).SequenceEqual(
            Destination.ParseLevels("DebugA-DebugZ")));

        Aver.IsTrue(
          (new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Debug, MessageType.Info) }).SequenceEqual(
            Destination.ParseLevels("-Info")));

        Aver.IsTrue(
          (new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.CatastrophicError) }).SequenceEqual(
            Destination.ParseLevels("Info-")));

        Aver.IsTrue(
          (new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Trace, MessageType.TraceZ),
                                        new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.CatastrophicError) }).SequenceEqual(
            Destination.ParseLevels("Trace - TraceZ, Info-")));

        Aver.IsTrue(
          (new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Trace, MessageType.Trace),
                                        new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.Info),
                                        new Tuple<MessageType, MessageType>(MessageType.Warning, MessageType.Warning) }).SequenceEqual(
            Destination.ParseLevels("Trace | Info | Warning")));

        Aver.IsTrue(
          (new Destination.LevelsList { new Tuple<MessageType, MessageType>(MessageType.Trace, MessageType.Trace),
                                         new Tuple<MessageType, MessageType>(MessageType.Info, MessageType.Info) }).SequenceEqual(
            Destination.ParseLevels("Trace; Info")));
      }
    }
}
