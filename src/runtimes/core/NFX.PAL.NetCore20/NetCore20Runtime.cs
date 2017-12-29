﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.PAL;

namespace NFX.PAL.NetCore20
{

  //https://stackoverflow.com/questions/3769405/determining-cpu-utilization

  /// <summary>
  /// Represents NET Core 2 Runtime
  /// </summary>
  public sealed class NetCore20Runtime : PALImplementation
  {
    public NetCore20Runtime() : this(false)
    {
      NFX.PAL.PlatformAbstractionLayer.____SetImplementation(this);
    }

    internal NetCore20Runtime(bool testMode) : base()
    {
      m_Machine = null;
      m_FS = null;
      m_Graphics = null;
    }

    private IPALMachineInfo m_Machine;
    private IPALFileSystem m_FS;
    private Graphics.IPALGraphics m_Graphics;

    public override string Name => nameof(NetCore20Runtime);
    public override bool IsNetCore => true;
    public override IPALFileSystem FileSystem => m_FS;
    public override IPALMachineInfo MachineInfo => m_Machine;
    public override Graphics.IPALGraphics Graphics => m_Graphics;
  }
}
