﻿using System;
using System.Collections.Generic;
using System.Text;

using NFX;
using NFX.ApplicationModel.Pile;
using System.Threading.Tasks;

namespace BusinessLogic.Toy
{
  internal static class PileSpeed
  {
    public static void ProfileByteArray(int threads, int count, int byteSize)
    {
      var payload = new byte[byteSize];
      body(count, threads, payload);
    }

    public static void ProfileString(int threads, int count, int charSize)
    {
      var payload = new string('a', charSize);
      body(count, threads, payload);
    }

    public static void ProfileSimpleObject(int threads, int count)
    {
      var payload = new SimpleObject{ Name = "Victor Zoi", Flag =true, ID = Guid.NewGuid(), Int = 123, DOB = new DateTime(1962, 6, 21)};
      body(count, threads, payload);
    }

    public static void ProfilePersonObject(int threads, int count)
    {
      var payload = new Person{ Name = "Victor Zoi", Age =28, Salary=234234234, Notes ="blah,blah", Satisfied =true, K=0.0002f, Distance=10000, DOB = new DateTime(1962, 6, 21), L1=1, L2=34234};
      body(count, threads, payload);
    }


    private static void body(int cnt, int threads, object payload)
    {
      GC.Collect();
      using(var pile = new DefaultPile())
      {
        pile.Start();
        bodyCore(pile, cnt, threads, payload);
      }
    }

    private static void bodyCore(IPile pile, int cnt, int threads, object payload)
    {
      Console.WriteLine("Payload of type `{0}` tested on {1} thread/s".Args(payload.GetType().DisplayNameWithExpandedGenericArgs(), threads));
      Console.WriteLine("-------------------------------------------");


      var firstPointer = pile.Put(payload); //warmup

      var sw = System.Diagnostics.Stopwatch.StartNew();

      Parallel.For(0, cnt, new ParallelOptions{ MaxDegreeOfParallelism = threads}, (i) =>
      {
        var pp = pile.Put(payload);
      });
      Console.WriteLine("Put {0:n0} at {1:n0} ops/sec".Args(cnt, cnt / (sw.ElapsedMilliseconds / 1000d)));


      sw = System.Diagnostics.Stopwatch.StartNew();
      Parallel.For(0, cnt, new ParallelOptions{ MaxDegreeOfParallelism = threads }, (i) =>
      {
        var got = pile.Get(firstPointer);
      });
      Console.WriteLine("Got {0:n0} at {1:n0} ops/sec".Args(cnt, cnt / (sw.ElapsedMilliseconds / 1000d)));

      Console.WriteLine();
      Console.WriteLine();
    }

  }
}
