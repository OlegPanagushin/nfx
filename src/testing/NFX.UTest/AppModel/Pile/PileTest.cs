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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using NFX;
using NFX.ApplicationModel;
using NFX.ApplicationModel.Pile;
using NFX.DataAccess.Distributed;
using NFX.Scripting;

namespace NFX.UTest.AppModel.Pile
{
  [Runnable(TRUN.BASE, 7)]
  public class PileTest : IRunHook
  {
      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        GC.Collect();
        return false;
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        GC.Collect();
        return false;
      }


      [Run(TRUN.BASE, null, 8)]
      public void Initial()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(0, ipile.UtilizedBytes);
          Aver.AreEqual(0, ipile.OverheadBytes);
          Aver.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void PutWOStart()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;

          var row = ChargeRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Aver.IsFalse(pp.Valid);

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(0, ipile.UtilizedBytes);
          Aver.AreEqual(0, ipile.OverheadBytes);
          Aver.AreEqual(0, ipile.SegmentCount);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void GetWOStart()
      {
        using (var pile = new DefaultPile())
        {
          var ipile = pile as IPile;
          var obj = ipile.Get(PilePointer.Invalid);
          Aver.IsNull(obj);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void PutOne()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var row = CheckoutRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(row);

          Aver.AreEqual(1, ipile.ObjectCount);
          Aver.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(1, ipile.SegmentCount);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void PutGetOne()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = CheckoutRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          var rowOut = ipile.Get(pp) as CheckoutRow;

          Aver.AreObjectsEqual(rowIn, rowOut);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void PutGetTwo()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();

          var ipile = pile as IPile;

          var rowIn1 = CheckoutRow.MakeFake(new GDID(0, 1));
          var rowIn2 = CheckoutRow.MakeFake(new GDID(0, 2));

          var pp1 = ipile.Put(rowIn1);
          var pp2 = ipile.Put(rowIn2);

          Aver.AreEqual(2, ipile.ObjectCount);
          Aver.AreEqual(DefaultPile.SEG_SIZE_DFLT, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(1, ipile.SegmentCount);

          var rowOut1 = pile.Get(pp1) as CheckoutRow;
          var rowOut2 = pile.Get(pp2) as CheckoutRow;

          Aver.AreObjectsEqual(rowIn1, rowOut1);
          Aver.AreObjectsEqual(rowIn2, rowOut2);
        }
      }




      [Run(TRUN.BASE, null, 8)]
      public void PutGetRawObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var buf = new byte[]{1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

          var pp = ipile.Put(buf);

          byte svr;
          var buf2 = ipile.GetRawBuffer(pp, out svr); //main point: we dont get any exceptions

          Aver.IsTrue(buf2.Length >= buf.Length);
        }
      }



      [Run(TRUN.BASE, null, 8)]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void GetNoObject()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Get(PilePointer.Invalid);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void DeleteInvalid()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;
          ipile.Delete(PilePointer.Invalid);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void DeleteExisting()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = ChargeRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Delete(pp);

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.AllocatedMemoryBytes);
          Aver.AreEqual(0, ipile.UtilizedBytes);
          Aver.AreEqual(0, ipile.OverheadBytes);
          Aver.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);
        }
      }

      [Run(TRUN.BASE, null, 8)]
      [Aver.Throws(typeof(PileAccessViolationException))]
      public void Purge()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          var rowIn = ChargeRow.MakeFake(new GDID(0, 1));

          var pp = ipile.Put(rowIn);

          ipile.Purge();

          Aver.AreEqual(0, ipile.ObjectCount);
          Aver.AreEqual(0, ipile.SegmentCount);

          var rowOut = ipile.Get(pp);


        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void PutCheckerboardPattern2()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 100;

          var ppp = new Tuple<PilePointer, ChargeRow>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var ch = ChargeRow.MakeFake(new GDID(0, i));
            ppp[i] = new Tuple<PilePointer,ChargeRow>( ipile.Put(ch), ch);
          }

          Aver.AreEqual(CNT, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            var ch = ipile.Get(ppp[i].Item1);
            Aver.AreObjectsEqual(ch, ppp[i].Item2);
          }

          for(ulong i = 0; i < CNT; i+=2)
            ipile.Delete(ppp[i].Item1);

          Aver.AreEqual(CNT/2, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 2 == 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Aver.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Aver.AreObjectsEqual(ch, ppp[i].Item2);
            }
          }
        }
      }

      [Run(TRUN.BASE, null, 8)]
      public void PutCheckerboardPattern3()
      {
        using (var pile = new DefaultPile())
        {
          pile.Start();
          var ipile = pile as IPile;

          const ulong CNT = 123;

          var ppp = new Tuple<PilePointer, string>[CNT];

          for (ulong i = 0; i < CNT; i++)
          {
            var str = NFX.Parsing.NaturalTextGenerator.Generate(179);
            ppp[i] = new Tuple<PilePointer,string>( ipile.Put(str), str);
          }

          Aver.AreEqual(CNT, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
              ipile.Delete(ppp[i].Item1);
          }

          Aver.AreEqual(CNT/3, (ulong)ipile.ObjectCount);

          for(ulong i = 0; i < CNT; i++)
          {
            if (i % 3 != 0)
            {
              try
              {
                ipile.Get(ppp[i].Item1);
                Aver.Fail("Object is deleted but its pointer doesn't throw exception!");
              }
              catch (PileAccessViolationException) {}
            }
            else
            {
              var ch = ipile.Get(ppp[i].Item1);
              Aver.AreObjectsEqual(ppp[i].Item2, ch);
            }
          }

          ////Console.WriteLine("ObjectCount: {0}", ipile.ObjectCount);
          ////Console.WriteLine("AllocatedMemoryBytes: {0}", ipile.AllocatedMemoryBytes);
          ////Console.WriteLine("UtilizedBytes: {0}", ipile.UtilizedBytes);
          ////Console.WriteLine("OverheadBytes: {0}", ipile.OverheadBytes);
          ////Console.WriteLine("SegmentCount: {0}", ipile.SegmentCount);
        }
      }

      [Run(TRUN.BASE, null, 8, "isParallel=false  cnt=100000  minSz=0      maxSz=40      speed=true")]
      [Run("isParallel=false  cnt=10000   minSz=0      maxSz=50000   speed=true")]
      [Run("isParallel=false  cnt=1000    minSz=70000  maxSz=150000  speed=true")]
      [Run("isParallel=false  cnt=5000    minSz=0      maxSz=150000  speed=true")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=40      speed=true")]
      [Run("isParallel=true  cnt=10000   minSz=0      maxSz=50000   speed=true")]
      [Run("isParallel=true  cnt=1000    minSz=70000  maxSz=150000  speed=true")]
      [Run("isParallel=true  cnt=5000    minSz=0      maxSz=150000  speed=true")]

      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=40      speed=false")]
      [Run("isParallel=false  cnt=10000   minSz=0      maxSz=50000   speed=false")]
      [Run("isParallel=false  cnt=1000    minSz=70000  maxSz=150000  speed=false")]
      [Run("isParallel=false  cnt=5000    minSz=0      maxSz=150000  speed=false")]

      [Run(TRUN.BASE, null, 8, "isParallel=true  cnt=100000  minSz=0      maxSz=40      speed=false")]
      [Run("isParallel=true  cnt=10000   minSz=0      maxSz=50000   speed=false")]
      [Run("isParallel=true  cnt=1000    minSz=70000  maxSz=150000  speed=false")]
      [Run("isParallel=true  cnt=5000    minSz=0      maxSz=150000  speed=false")]
      public void VarSizes_Checkboard(bool isParallel, int cnt, int minSz, int maxSz, bool speed)
      {
        PileCacheTestCore.VarSizes_Checkboard(isParallel, cnt, minSz, maxSz, speed);
      }

      [Run(TRUN.BASE, null, 8,"isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=true")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=true")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=true")]
      [Run("isParallel=false  cnt=2100    minSz=65000  maxSz=129000  speed=false  rnd=true")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=true")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=true")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=true")]
      [Run("isParallel=true  cnt=2100    minSz=65000  maxSz=129000  speed=false  rnd=true")]


      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=true")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=true")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=true")]
      [Run("isParallel=false  cnt=2100    minSz=65000  maxSz=129000  speed=true  rnd=true")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=true")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=true")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=true")]
      [Run("isParallel=true  cnt=2100    minSz=65000  maxSz=129000  speed=true  rnd=true")]


      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=false")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=false")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=false")]
      [Run("isParallel=false  cnt=1200    minSz=65000  maxSz=129000  speed=false  rnd=false")]

      [Run("isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=false  rnd=false")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=false  rnd=false")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=false  rnd=false")]
      [Run("isParallel=true  cnt=1200    minSz=65000  maxSz=129000  speed=false  rnd=false")]


      [Run("isParallel=false  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=false")]
      [Run("isParallel=false  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=false")]
      [Run("isParallel=false  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=false")]
      [Run("isParallel=false  cnt=1200    minSz=65000  maxSz=129000  speed=true  rnd=false")]

      [Run(TRUN.BASE, null, 8,"isParallel=true  cnt=100000  minSz=0      maxSz=256     speed=true  rnd=false")]
      [Run("isParallel=true  cnt=25000   minSz=0      maxSz=8000    speed=true  rnd=false")]
      [Run("isParallel=true  cnt=15000   minSz=0      maxSz=24000   speed=true  rnd=false")]
      [Run("isParallel=true  cnt=1200    minSz=65000  maxSz=129000  speed=true  rnd=false")]
      public void VarSizes_Increasing_Random(bool isParallel, int cnt, int minSz, int maxSz, bool speed, bool rnd)
      {
        PileCacheTestCore.VarSizes_Increasing_Random(isParallel, cnt, minSz, maxSz, speed, rnd);
      }

      [Run(TRUN.BASE, null, 8)]
      public void Configuration()
      {
        var conf = @"
 app
 {
   memory-management
   {
     pile
     {
       alloc-mode=favorspeed
       free-list-size=100000
       max-segment-limit=79
       segment-size=395313143 //will be rounded to 16 byte boundary: 395,313,152
       max-memory-limit=123666333000

       free-chunk-sizes='128, 256, 512, 1024, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 16000, 32000, 64000,  256000'
     }

     pile
     {
       name='specialNamed'
       free-list-size=99000
       max-segment-limit=73
       segment-size=395313147 //will be rounded to 16 byte boundary: 395,313,152
       max-memory-limit=127666333000

       free-chunk-sizes='77, 124, 180, 190, 200, 210, 220, 230, 1000, 2000, 3000, 4000, 5000, 32000, 64000,  257000'
     }
   }
 }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

        using(var app = new ServiceBaseApplication(null, conf))
        {
          using (var pile = new DefaultPile())
          {
            pile.Configure(null);

            Aver.IsTrue(AllocationMode.FavorSpeed == pile.AllocMode);
            Aver.AreEqual(100000, pile.FreeListSize);
            Aver.AreEqual(79, pile.MaxSegmentLimit);
            Aver.AreEqual(395313152, pile.SegmentSize);
            Aver.AreEqual(123666333000, pile.MaxMemoryLimit);

            Aver.AreEqual(128, pile.FreeChunkSizes[00]);
            Aver.AreEqual(256, pile.FreeChunkSizes[01]);
            Aver.AreEqual(512, pile.FreeChunkSizes[02]);
            Aver.AreEqual(1024, pile.FreeChunkSizes[03]);
            Aver.AreEqual(2000, pile.FreeChunkSizes[04]);
            Aver.AreEqual(3000, pile.FreeChunkSizes[05]);
            Aver.AreEqual(4000, pile.FreeChunkSizes[06]);
            Aver.AreEqual(5000, pile.FreeChunkSizes[07]);
            Aver.AreEqual(6000, pile.FreeChunkSizes[08]);
            Aver.AreEqual(7000, pile.FreeChunkSizes[09]);
            Aver.AreEqual(8000, pile.FreeChunkSizes[10]);
            Aver.AreEqual(9000, pile.FreeChunkSizes[11]);
            Aver.AreEqual(16000, pile.FreeChunkSizes[12]);
            Aver.AreEqual(32000, pile.FreeChunkSizes[13]);
            Aver.AreEqual(64000, pile.FreeChunkSizes[14]);
            Aver.AreEqual(256000, pile.FreeChunkSizes[15]);

            pile.Start();//just to test that it starts ok
          }

          using (var pile = new DefaultPile("specialNamed"))
          {
            pile.Configure(null);

            Aver.IsTrue(AllocationMode.ReuseSpace == pile.AllocMode);
            Aver.AreEqual(99000, pile.FreeListSize);
            Aver.AreEqual(73, pile.MaxSegmentLimit);
            Aver.AreEqual(395313152, pile.SegmentSize);
            Aver.AreEqual(127666333000, pile.MaxMemoryLimit);

            Aver.AreEqual(77, pile.FreeChunkSizes[00]);
            Aver.AreEqual(124, pile.FreeChunkSizes[01]);
            Aver.AreEqual(180, pile.FreeChunkSizes[02]);
            Aver.AreEqual(190, pile.FreeChunkSizes[03]);
            Aver.AreEqual(200, pile.FreeChunkSizes[04]);
            Aver.AreEqual(210, pile.FreeChunkSizes[05]);
            Aver.AreEqual(220, pile.FreeChunkSizes[06]);
            Aver.AreEqual(230, pile.FreeChunkSizes[07]);
            Aver.AreEqual(1000, pile.FreeChunkSizes[08]);
            Aver.AreEqual(2000, pile.FreeChunkSizes[09]);
            Aver.AreEqual(3000, pile.FreeChunkSizes[10]);
            Aver.AreEqual(4000, pile.FreeChunkSizes[11]);
            Aver.AreEqual(5000, pile.FreeChunkSizes[12]);
            Aver.AreEqual(32000, pile.FreeChunkSizes[13]);
            Aver.AreEqual(64000, pile.FreeChunkSizes[14]);
            Aver.AreEqual(257000, pile.FreeChunkSizes[15]);

            pile.Start();//just to test that it starts ok


          }

        }//using app
      }
  }
}
