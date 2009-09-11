﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Csla.Core
{
  /// <summary>
  /// Defines the interface for a factory object
  /// that creates IPropertyInfo objects.
  /// </summary>
  public interface IPropertyInfoFactory
  {
    /// <summary>
    /// Creates a new instance of PropertyInfo.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    Csla.PropertyInfo<T> Create<T>(string name);
    /// <summary>
    /// Creates a new instance of PropertyInfo.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="friendlyName">
    /// Friendly display name for the property.
    /// </param>
    Csla.PropertyInfo<T> Create<T>(string name, string friendlyName);
    /// <summary>
    /// Creates a new instance of PropertyInfo.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="friendlyName">
    /// Friendly display name for the property.
    /// </param>
    /// <param name="relationship">Relationship with
    /// referenced object.</param>
    Csla.PropertyInfo<T> Create<T>(string name, string friendlyName, RelationshipTypes relationship);
    /// <summary>
    /// Creates a new instance of PropertyInfo.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="friendlyName">
    /// Friendly display name for the property.
    /// </param>
    /// <param name="defaultValue">
    /// Default value for the property.
    /// </param>
    Csla.PropertyInfo<T> Create<T>(string name, string friendlyName, T defaultValue);
    /// <summary>
    /// Creates a new instance of PropertyInfo.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="friendlyName">
    /// Friendly display name for the property.
    /// </param>
    /// <param name="defaultValue">
    /// Default value for the property.
    /// </param>
    /// <param name="relationship">Relationship with
    /// referenced object.</param>
    Csla.PropertyInfo<T> Create<T>(string name, string friendlyName, T defaultValue, RelationshipTypes relationship);
  }
}
