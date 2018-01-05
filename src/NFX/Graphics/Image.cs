﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using NFX.PAL;
using NFX.PAL.Graphics;

namespace NFX.Graphics
{
  /// <summary>
  /// Represents a 2d graphical in-memory Image.
  /// The purpose of this object is to provide basic image processing capabilities cross-platform.
  /// Graphics objects are NOT thread-safe
  /// </summary>
  public sealed class Image : DisposableObject
  {

    public const int DEFAULT_RESOLUTION_PPI = 72;

    /// <summary>Creates a new image instance from a named image file</summary>
    public static Image FromFile(string fileName) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(fileName));

    /// <summary>Creates a new image instance from image content contained in a byte[]</summary>
    public static Image FromBytes(byte[] data)    => new Image(PlatformAbstractionLayer.Graphics.CreateImage(data));

    /// <summary>Creates a new image instance from image content contained in a stream</summary>
    public static Image FromStream(Stream stream) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(stream));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(int width, int height) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(
                                                                    new Size(width, height),
                                                                    new Size(DEFAULT_RESOLUTION_PPI, DEFAULT_RESOLUTION_PPI),
                                                                    ImagePixelFormat.Default));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(int width, int height, int xDPI, int yDPI) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(
                                                                    new Size(width, height),
                                                                    new Size(xDPI, yDPI),
                                                                    ImagePixelFormat.Default));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(int width, int height, ImagePixelFormat pixFormat) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(
                                                                    new Size(width, height),
                                                                    new Size(DEFAULT_RESOLUTION_PPI, DEFAULT_RESOLUTION_PPI),
                                                                    pixFormat));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(int width, int height, int xDPI, int yDPI, ImagePixelFormat pixFormat) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(
                                                                    new Size(width, height),
                                                                    new Size(xDPI, yDPI),
                                                                    pixFormat));


    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(Size size) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(
                                                                    size,
                                                                    new Size(DEFAULT_RESOLUTION_PPI, DEFAULT_RESOLUTION_PPI),
                                                                    ImagePixelFormat.Default));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(Size size, ImagePixelFormat pixFormat) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(
                                                                    size,
                                                                    new Size(DEFAULT_RESOLUTION_PPI, DEFAULT_RESOLUTION_PPI),
                                                                    pixFormat));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(Size size, Size resolution) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(size, resolution, ImagePixelFormat.Default));

    /// <summary>Creates a new image instance of the specified properties</summary>
    public static Image Of(Size size, Size resolution, ImagePixelFormat format) => new Image(PlatformAbstractionLayer.Graphics.CreateImage(size, resolution, format));



    private Image(IPALImage handle) { m_Handle = handle; }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Handle);
    }

    private IPALImage m_Handle;

    internal IPALImage Handle
    {
      get
      {
        EnsureObjectNotDisposed();
        return m_Handle;
      }
    }

    public ImagePixelFormat PixelFormat { get {  EnsureObjectNotDisposed(); return m_Handle.PixelFormat; } }

    public Size Size { get{  EnsureObjectNotDisposed(); return m_Handle.GetSize(); } }
    public int Width  => Size.Width;
    public int Height => Size.Height;

    public Rectangle Dimensions => new Rectangle(Point.Empty, Size);

    public Size Resolution { get{  EnsureObjectNotDisposed(); return m_Handle.GetResolution(); } }
    public int XResolution => Resolution.Width;
    public int YResolution => Resolution.Height;


    public void SetResolution(int xDPI, int yDPI) => SetResolution(new Size(xDPI, yDPI));

    public void SetResolution(Size resolution)
    {
      EnsureObjectNotDisposed();
      m_Handle.SetResolution(resolution);
    }

    public void MakeTransparent(Color? dflt = null)
    {
      EnsureObjectNotDisposed();
      m_Handle.MakeTransparent(dflt);
    }

    //todo: Lock bits

    public Color GetPixel(int x, int y) => GetPixel(new Point(x, y));
    public Color GetPixel(Point p)
    {
      EnsureObjectNotDisposed();
      return m_Handle.GetPixel(p);
    }

    public Color GetPixel(float x, float y) => GetPixel(new PointF(x, y));
    public Color GetPixel(PointF p)
    {
      EnsureObjectNotDisposed();
      return m_Handle.GetPixel(p);
    }

    public void SetPixel(int x, int y, Color color) => SetPixel(new Point(x, y), color);
    public void SetPixel(Point p, Color color)
    {
      EnsureObjectNotDisposed();
      m_Handle.SetPixel(p, color);
    }

    public void SetPixel(float x, float y, Color color) => SetPixel(new PointF(x, y), color);
    public void SetPixel(PointF p, Color color)
    {
      EnsureObjectNotDisposed();
      m_Handle.SetPixel(p, color);
    }

    public Image ResizeTo(int newWidth, int newHeight, InterpolationMode interpolation = InterpolationMode.Default) => ResizeTo(new Size(newWidth, newHeight), interpolation);
    public Image ResizeTo(Size newSize, InterpolationMode interpolation = InterpolationMode.Default)
    {
      if (newSize.Width==0 || newSize.Height==0)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + "Resize(size is empty)");

      var result = Image.Of(newSize, Resolution, PixelFormat);

      using (var canvas = result.CreateCanvas())
      {
        canvas.Interpolation = interpolation;
        canvas.DrawImage(this, result.Dimensions);
      }

      return result;
    }


    /// <summary>
    /// Saves image to the named file
    /// </summary>
    public void Save(string fileName, ImageFormat format)
    {
      EnsureObjectNotDisposed();
      m_Handle.Save(fileName, format);
    }

    /// <summary>
    /// Saves image to stream
    /// </summary>
    public void Save(Stream stream, ImageFormat format)
    {
      EnsureObjectNotDisposed();
      m_Handle.Save(stream, format);
    }

    /// <summary>
    /// Saves image to byte[]
    /// </summary>
    public byte[] Save(ImageFormat format)
    {
      EnsureObjectNotDisposed();
      return m_Handle.Save(format);
    }

    /// <summary>
    /// Create a canvas object for this image. Canvases are used to draw on images.
    /// Canvas must be disposed after drawing is finished.
    /// Check Canvas.OwnsAssets to determine cacing for graphics primitives (suchs as pens, brushes etc.) beyond the
    /// lifecycle of Canvas
    /// </summary>
    public Canvas CreateCanvas()
    {
      EnsureObjectNotDisposed();
      var hcanvas = m_Handle.CreateCanvas();
      return new Canvas(hcanvas);
    }

  }
}
