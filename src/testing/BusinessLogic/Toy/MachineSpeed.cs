﻿using System;
using System.Collections.Generic;
using System.Text;

using NFX;

namespace BusinessLogic.Toy
{

    internal static class MachineSpeed
    {
      public static void CRC32()
      {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var buf = new byte[128*1024*1024];

        for(var i=0; i<buf.Length; i++)
        {
          var rnd = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 0xff);
          buf[i] = (byte)rnd;
        }

        var csum = NFX.IO.ErrorHandling.CRC32.ForBytes(buf);

        Console.WriteLine("Checksum {0} in {1:n0} msec".Args(csum, sw.ElapsedMilliseconds));

        Console.WriteLine();
        Console.WriteLine();
     }
    }
}
