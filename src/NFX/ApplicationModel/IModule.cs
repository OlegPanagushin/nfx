﻿using System;
using System.Collections.Generic;
using System.Text;

using NFX.Environment;
using NFX.Instrumentation;

namespace NFX.ApplicationModel
{
  /// <summary>
  /// Describes application modules - entities that contain business domain logic of the application or
  /// general system logic (e.g. financial logic, complex image rendering service, social network mix-in etc.)
  /// </summary>
  public interface IModule : IApplicationComponent, INamed, IOrdered
  {
    /// <summary>
    /// References a parent logic module, or null if this is a root module injected in the application container
    /// </summary>
    IModule ParentModule { get; }

    /// <summary>
    /// Returns true when the module is injected in the parent context by the code, not configuration script
    /// </summary>
    bool IsHardcodedModule { get; }

    /// <summary>
    /// Enumerates an ordered collection of child modules and provides access by name
    /// </summary>
    IOrderedRegistry<IModule> ChildModules { get; }


    /// <summary>
    /// Gets a child module of the specified TModule type optionally applying a filter.
    /// If module is not found then exception is thrown. Contrast with TryGet()
    /// </summary>
    TModule Get<TModule>(Func<TModule, bool> filter = null) where TModule : class, IModule;

    /// <summary>
    /// Tries to get a child module of the specified TModule type optionally applying a filter.
    /// If module is not found then returns null. Contrast with Get()
    /// </summary>
    TModule TryGet<TModule>(Func<TModule, bool> filter = null) where TModule : class, IModule;

    /// <summary>
    /// Gets a child module of the specified TModule type with the specified name.
    /// If module is not found then exception is thrown. Contrast with TryGet()
    /// </summary>
    TModule Get<TModule>(string name) where TModule : class, IModule;

    /// <summary>
    /// Tries to get a child module of the specified TModule type with the specified name.
    /// If module is not found then returns null. Contrast with Get()
    /// </summary>
    TModule TryGet<TModule>(string name) where TModule : class, IModule;

    /// <summary>
    /// Determines the effective log level for this module, taking it from parent if it is not defined on this level
    /// </summary>
    NFX.Log.MessageType ModuleEffectiveLogLevel { get; }
  }

  /// <summary>
  /// Describes module implementation
  /// </summary>
  public interface IModuleImplementation : IModule, IDisposable, IConfigurable, IInstrumentable
  {
    /// <summary>
    /// Called by the application container after all services have initialized.
    /// An implementation is expected to notify all subordinate (child) modules.
    /// The call is used to perform initialization tasks such as inter-service dependency fixups,
    /// initial data loads (e.g. initial cache fetch etc..) after everything has loaded in the application container.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    void ApplicationAfterInit(IApplication application);

    /// <summary>
    /// Called by the application container before services shutdown.
    /// An implementation is expected to notify all subordinate (child) modules.
    /// The call is used to perform finalization tasks such as inter-service dependency cleanups,
    /// buffer flushes etc. before the application container starts to shutdown.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    void ApplicationBeforeCleanup(IApplication application);


    /// <summary>
    /// Defines log level for this module, if not defined then the component logger uses the parent log level
    /// via the ModuleEffectiveLogLevel property
    /// </summary>
    NFX.Log.MessageType? ModuleLogLevel { get; set; }

    /// <summary>
    /// Writes a log message through logic module; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged
    /// </summary>
    Guid ModuleLog(NFX.Log.MessageType type, string from, string text, Exception error = null, Guid? related = null, string pars = null);
  }
}
