﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using NFX.OS;
using System.Runtime.InteropServices;

namespace NFX.PAL.NetFramework
{
  internal class PALMachineInfo : IPALMachineInfo
  {
    public PALMachineInfo()
    {
      m_CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
      m_RAMAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", true);
      m_IsMono = Type.GetType("Mono.Runtime") != null;
    }

    private bool m_IsMono;
    private PerformanceCounter m_CPUCounter;
    private PerformanceCounter m_RAMAvailableCounter;


    public int CurrentProcessorUsagePct { get => (int)m_CPUCounter.NextValue(); }
    public int CurrentAvailableMemoryMb { get => (int)m_RAMAvailableCounter.NextValue(); }
    public bool IsMono { get => m_IsMono; }


    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
      public uint dwLength;
      public uint dwMemoryLoad;
      public ulong ullTotalPhys;
      public ulong ullAvailPhys;
      public ulong ullTotalPageFile;
      public ulong ullAvailPageFile;
      public ulong ullTotalVirtual;
      public ulong ullAvailVirtual;
      public ulong ullAvailExtendedVirtual;
      public MEMORYSTATUSEX()
      {
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
      }
    }


    public MemoryStatus GetMemoryStatus()
    {
      if (IsMono) return new MemoryStatus();
      var stat = new MEMORYSTATUSEX();

      if (NFX.OS.Computer.OSFamily == OS.OSFamily.Windows)
        GlobalMemoryStatusEx(stat);

      return new MemoryStatus()
      {
        LoadPct = stat.dwMemoryLoad,

        TotalPhysicalBytes = stat.ullTotalPhys,
        AvailablePhysicalBytes = stat.ullAvailPhys,

        TotalPageFileBytes = stat.ullTotalPageFile,
        AvailablePageFileBytes = stat.ullAvailPageFile,

        TotalVirtBytes = stat.ullTotalVirtual,
        AvailableVirtBytes = stat.ullAvailVirtual
      };
    }
  }
}
