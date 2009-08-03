﻿using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Csla.Properties;
using System.Reflection;
using Csla.Reflection;
using System.Collections;
using System.Collections.Generic;
using Csla.Core;
using System.Windows;

namespace Csla.Silverlight
{
  /// <summary>
  /// Creates, retrieves and manages business objects
  /// from XAML in a form.
  /// </summary>
  public class CslaDataProvider : INotifyPropertyChanged
  {
    #region Events

    /// <summary>
    /// Event raised when the data object has
    /// changed to another data object, or when
    /// an exception has occurred during processing.
    /// </summary>
    public event EventHandler DataChanged;

    /// <summary>
    /// Raises the DataChanged event.
    /// </summary>
    protected void OnDataChanged()
    {
      RefreshCanOperationsValues();
      RefreshCanOperationsOnObjectLevel();
      if (DataChanged != null)
        DataChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event raised when a Save operation
    /// is complete.
    /// </summary>
    public event EventHandler<Csla.Core.SavedEventArgs> Saved;

    /// <summary>
    /// Raises the Saved event.
    /// </summary>
    /// <param name="newObject">Reference to new
    /// object resulting from the save.</param>
    /// <param name="error">Reference to any exception
    /// object that may have resulted from the operation.</param>
    /// <param name="userState">Reference to any user state
    /// object provided by the caller.</param>
    protected void OnSaved(object newObject, Exception error, object userState)
    {
      if (Saved != null)
        Saved(this, new Csla.Core.SavedEventArgs(newObject, error, userState));
    }

    private void dataObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      RefreshCanOperationsValues();
    }

    private void dataObject_ChildChanged(object sender, ChildChangedEventArgs e)
    {
      RefreshCanOperationsValues();
    }

    #endregion

    #region Properties

    private bool _isBusy = false;

    /// <summary>
    /// Gets a value indicating whether this object is
    /// executing an asynchronous process.
    /// </summary>
    public bool IsBusy
    {
      get { return _isBusy; }
      protected set
      {
        _isBusy = value;
        OnPropertyChanged(new PropertyChangedEventArgs("IsBusy"));
        OnPropertyChanged(new PropertyChangedEventArgs("IsNotBusy"));
        RefreshCanOperationsValues();
      }
    }

    /// <summary>
    /// Gets a value indicating whether this object is
    /// not executing an asynchronous process.
    /// </summary>
    public bool IsNotBusy
    {
      get
      { return !IsBusy; }
    }

    void CslaDataProvider_BusyChanged(object sender, BusyChangedEventArgs e)
    {
      // only set busy state for entire object.  Ignore busy state based
      // on asynch rules being active
      if (e.PropertyName == string.Empty)
        IsBusy = e.Busy;
      else
        RefreshCanOperationsValues();
    }


    private object _dataObject;

    /// <summary>
    /// Gets or sets a reference to the data
    /// object.
    /// </summary>
    public static readonly DependencyProperty ObjectInstanceProperty =
     DependencyProperty.Register("ObjectInstance", typeof(object),
     typeof(CslaDataProvider), new PropertyMetadata(null));


    /// <summary>
    /// Gets or sets a reference to the data
    /// object.
    /// </summary>
    public object ObjectInstance
    {
      get
      {
        return _dataObject;
      }
      set
      {
        SetError(null);
        SetObjectInstance(value);
        try
        {
          OnDataChanged();
        }
        catch (Exception ex)
        {
          // Silverlight seems to throw a meaningless null ref exception
          // and during page load there are possible timing issues
          // where these events may cause non-useful exceptions
          // and this is a workaround to ignore the issues
          var o = ex;
        }
      }
    }

    private void SetObjectInstance(object value)
    {
      //hook up event handlers for notificaiton propagation
      if (_dataObject != null && _dataObject is INotifyPropertyChanged)
        ((INotifyPropertyChanged)_dataObject).PropertyChanged -= new PropertyChangedEventHandler(dataObject_PropertyChanged);
      if (_dataObject != null && _dataObject is INotifyChildChanged)
        ((INotifyChildChanged)_dataObject).ChildChanged -= new EventHandler<ChildChangedEventArgs>(dataObject_ChildChanged);
      if (_dataObject != null && _dataObject is INotifyBusy)
        ((INotifyBusy)_dataObject).BusyChanged -= new BusyChangedEventHandler(CslaDataProvider_BusyChanged);

      _dataObject = value;

      if (_manageObjectLifetime)
      {
        var undoable = _dataObject as Csla.Core.ISupportUndo;
        if (undoable != null)
          undoable.BeginEdit();
      }

      if (_dataObject != null && _dataObject is INotifyPropertyChanged)
        ((INotifyPropertyChanged)_dataObject).PropertyChanged += new PropertyChangedEventHandler(dataObject_PropertyChanged);
      if (_dataObject != null && _dataObject is INotifyChildChanged)
        ((INotifyChildChanged)_dataObject).ChildChanged += new EventHandler<ChildChangedEventArgs>(dataObject_ChildChanged);

      if (_dataObject != null && _dataObject is INotifyBusy)
        ((INotifyBusy)_dataObject).BusyChanged += new BusyChangedEventHandler(CslaDataProvider_BusyChanged);

      try
      {
        OnPropertyChanged(new PropertyChangedEventArgs("ObjectInstance"));
      }
      catch (Exception ex)
      {
        // Silverlight seems to throw a meaningless null ref exception
        // and during page load there are possible timing issues
        // where these events may cause non-useful exceptions
        // and this is a workaround to ignore the issues
        var o = ex;
      }
      try
      {
        OnPropertyChanged(new PropertyChangedEventArgs("Data"));
      }
      catch (Exception ex)
      {
        // Silverlight seems to throw a meaningless null ref exception
        // and during page load there are possible timing issues
        // where these events may cause non-useful exceptions
        // and this is a workaround to ignore the issues
        var o = ex;
      }
    }

    /// <summary>
    /// Gets a reference to the data object.
    /// </summary>
    public object Data
    {
      get
      {
        if (_isInitialLoadEnabled && !_isInitialLoadCompleted)
        {
          _isInitialLoadCompleted = true;
          Refresh();
        }
        return _dataObject;
      }
    }

    private bool _manageObjectLifetime = true;

    /// <summary>
    /// Gets or sets a value indicating whether
    /// the business object's lifetime should
    /// be managed automatically.
    /// </summary>
    public bool ManageObjectLifetime
    {
      get { return _manageObjectLifetime; }
      set
      {
        if (_dataObject != null)
          throw new NotSupportedException(Resources.ObjectNotNull);
        _manageObjectLifetime = value;
      }
    }

    private bool _isInitialLoadEnabled = false;
    private bool _isInitialLoadCompleted = false;

    /// <summary>
    /// Gets or sets a value indicating whether the
    /// data provider should load data as the page
    /// is loaded.
    /// </summary>
    public bool IsInitialLoadEnabled
    {
      get { return _isInitialLoadEnabled; }
      set
      {
        if (_dataObject != null)
          throw new NotSupportedException(Resources.ObjectNotNull);
        _isInitialLoadEnabled = value;
      }
    }

    private string _objectType;

    /// <summary>
    /// Gets or sets the assembly qualified type
    /// name for the business object to be
    /// created or retrieved.
    /// </summary>
    public string ObjectType
    {
      get
      {
        return _objectType;
      }
      set
      {
        _objectType = value;
        OnPropertyChanged(new PropertyChangedEventArgs("ObjectType"));
      }
    }

    private string _factoryMethod;

    /// <summary>
    /// Gets or sets the name of the static factory
    /// method to be invoked to create or retrieve
    /// the business object.
    /// </summary>
    public string FactoryMethod
    {
      get
      {
        return _factoryMethod;
      }
      set
      {
        _factoryMethod = value;
        OnPropertyChanged(new PropertyChangedEventArgs("FactoryMethod"));
      }
    }

    private ObservableCollection<object> _factoryParameters;

    /// <summary>
    /// Gets or sets a collection of parameter values
    /// that are passed to the factory method.
    /// </summary>
    public ObservableCollection<object> FactoryParameters
    {
      get
      {
        if (_factoryParameters == null)
          _factoryParameters = new ObservableCollection<object>();
        return _factoryParameters;
      }
      set
      {
        _factoryParameters =
          new ObservableCollection<object>();
        List<object> temp = new List<object>(value);
        foreach (object oneParameters in temp)
        {
          _factoryParameters.Add(oneParameters);
        }
        OnPropertyChanged(new PropertyChangedEventArgs("FactoryParameters"));
      }
    }

    private Exception _error;

    /// <summary>
    /// Gets a reference to the Exception object
    /// (if any) from the last data portal operation.
    /// </summary>
    public Exception Error
    {
      get { return _error; }
      private set
      {
        SetError(value);
        OnDataChanged();
      }
    }

    private void SetError(Exception value)
    {
      var changed = !ReferenceEquals(_error, value);
      if (changed)
        _error = value;
      IsBusy = false;
      if (changed)
        OnPropertyChanged(new PropertyChangedEventArgs("Error"));
    }

    private object _dataChangedHandler;

    /// <summary>
    /// Gets or sets a reference to an object that
    /// will handle the DataChanged event raised
    /// by this data provider.
    /// </summary>
    /// <remarks>
    /// This property is designed to 
    /// reference an ErrorDialog control.
    /// </remarks>
    public object DataChangedHandler
    {
      get
      {
        return _dataChangedHandler;
      }
      set
      {
        _dataChangedHandler = value;
        var dialog = value as ErrorDialog;
        if (dialog != null)
          dialog.Register(this);
        OnPropertyChanged(new PropertyChangedEventArgs("DataChangedHandler"));
      }
    }

    #endregion

    #region Operations

    /// <summary>
    /// Cancels any changes to the object.
    /// </summary>
    public void Cancel()
    {
      SetError(null);
      if (_manageObjectLifetime)
      {
        try
        {
          var undoable = _dataObject as Csla.Core.ISupportUndo;
          if (undoable != null)
          {
            IsBusy = true;
            ObjectInstance = null;
            undoable.CancelEdit();
            ObjectInstance = undoable;
            var trackable = _dataObject as ITrackStatus;
            if (trackable != null)
              IsBusy = trackable.IsBusy;
            else
              IsBusy = false;
          }
        }
        catch (Exception ex)
        {
          this.Error = ex;
        }
      }
    }

    /// <summary>
    /// Accepts any changes to the object and
    /// invokes the object's BeginSave() method.
    /// </summary>
    public void Save()
    {
      SetError(null);
      try
      {

        var obj = _dataObject as Csla.Core.ISavable;
        if (obj != null)
        {
          if (_manageObjectLifetime)
          {
            // clone the object if possible

            ICloneable clonable = _dataObject as ICloneable;
            if (clonable != null)
              obj = (Csla.Core.ISavable)clonable.Clone();

            var undoable = obj as Csla.Core.ISupportUndo;
            if (undoable != null)
              undoable.ApplyEdit();
          }


          obj.Saved -= new EventHandler<Csla.Core.SavedEventArgs>(obj_Saved);
          obj.Saved += new EventHandler<Csla.Core.SavedEventArgs>(obj_Saved);
          IsBusy = true;
          obj.BeginSave();
        }
      }
      catch (Exception ex)
      {
        IsBusy = false;
        this.Error = ex;
        OnSaved(_dataObject, ex, null);
      }
    }

    void obj_Saved(object sender, Csla.Core.SavedEventArgs e)
    {
      IsBusy = false;
      if (e.Error != null)
        Error = e.Error;
      else
        ObjectInstance = e.NewObject;

      OnSaved(e.NewObject, e.Error, e.UserState);
    }

    /// <summary>
    /// Marks an editable root object for deletion.
    /// </summary>
    public void Delete()
    {
      SetError(null);
      try
      {
        var obj = _dataObject as Csla.Core.BusinessBase;
        if (obj != null && !obj.IsChild)
        {
          IsBusy = true;
          obj.Delete();
          var trackable = _dataObject as ITrackStatus;
          if (trackable != null)
            IsBusy = trackable.IsBusy;
          else
            IsBusy = false;
        }
      }
      catch (Exception ex)
      {
        this.Error = ex;
      }
    }

    private void QueryCompleted(object sender, EventArgs e)
    {
      IDataPortalResult eventArgs = e as IDataPortalResult;
      SetError(eventArgs.Error);
      SetObjectInstance(eventArgs.Object);
      try
      {
        OnDataChanged();
      }
      catch (Exception ex)
      {
        // Silverlight seems to throw a meaningless null ref exception
        // and during page load there are possible timing issues
        // where these events may cause non-useful exceptions
        // and this is a workaround to ignore the issues
        var o = ex;
      }
      RefreshCanOperationsValues();
      this.IsBusy = false;
    }

    /// <summary>
    /// Begins an async add new operation to add a 
    /// new item to an editable list business object.
    /// </summary>
    public void AddNewItem()
    {
      SetError(null);
      try
      {
        var obj = _dataObject as Csla.Core.IBindingList;
        if (obj != null)
        {
          IsBusy = true;
          obj.AddNew();
          var trackable = _dataObject as ITrackStatus;
          if (trackable != null)
            IsBusy = trackable.IsBusy;
          else
            IsBusy = false;
        }
        RefreshCanOperationsValues();
      }
      catch (Exception ex)
      {
        this.Error = ex;
      }
    }

    /// <summary>
    /// Removes an item from an editable list
    /// business object.
    /// </summary>
    /// <param name="item">
    /// Reference to the child item to remove.
    /// </param>
    public void RemoveItem(object item)
    {
      try
      {
        SetError(null);
        var obj = _dataObject as System.Collections.IList;
        if (obj != null)
        {
          IsBusy = true;
          obj.Remove(item);
          var trackable = _dataObject as ITrackStatus;
          if (trackable != null)
            IsBusy = trackable.IsBusy;
          else
            IsBusy = false;
        }
        RefreshCanOperationsValues();
      }
      catch (Exception ex)
      {
        this.Error = ex;
      }
    }

    private Delegate CreateHandler(Type objectType)
    {
      var args = typeof(DataPortalResult<>).MakeGenericType(objectType);
      MethodInfo method = MethodCaller.GetNonPublicMethod(this.GetType(), "QueryCompleted");
      Delegate handler = Delegate.CreateDelegate(typeof(EventHandler<>).MakeGenericType(args), this, method);
      return handler;
    }

    /// <summary>
    /// Causes the data provider to execute the
    /// factory method, refreshing the business
    /// object by creating or retrieving a new
    /// instance.
    /// </summary>
    public void Refresh()
    {
      if (_objectType != null && _factoryMethod != null)
        try
        {
          SetError(null);
          this.IsBusy = true;
          List<object> parameters = new List<object>(FactoryParameters);
          Type objectType = Csla.Reflection.MethodCaller.GetType(_objectType);
          parameters.Add(CreateHandler(objectType));

          MethodCaller.CallFactoryMethod(objectType, _factoryMethod, parameters.ToArray());
        }
        catch (Exception ex)
        {
          this.Error = ex;
        }
    }

    /// <summary>
    /// Causes the data provider to trigger data binding
    /// to rebind to the current business object.
    /// </summary>
    public void Rebind()
    {
      object tmp = ObjectInstance;
      ObjectInstance = null;
      ObjectInstance = tmp;
    }

    #endregion

    #region  Can methods that account for user rights and object state

    private bool _canSave = false;

    /// <summary>
    /// Gets a value indicating whether the business object
    /// can currently be saved.
    /// </summary>
    public bool CanSave
    {
      get
      {
        return _canSave;
      }
      private set
      {
        if (_canSave != value)
        {
          _canSave = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanSave"));
        }
      }
    }

    private bool _canCancel = false;

    /// <summary>
    /// Gets a value indicating whether the business object
    /// can currently be canceled.
    /// </summary>
    public bool CanCancel
    {
      get
      {
        return _canCancel;
      }
      private set
      {
        if (_canCancel != value)
        {
          _canCancel = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanCancel"));
        }
      }
    }

    private bool _canCreate = false;

    /// <summary>
    /// Gets a value indicating whether an instance
    /// of the business object
    /// can currently be created.
    /// </summary>
    public bool CanCreate
    {
      get
      {
        return _canCreate;
      }
      private set
      {
        if (_canCreate != value)
        {
          _canCreate = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanCreate"));
        }
      }
    }

    private bool _canDelete = false;

    /// <summary>
    /// Gets a value indicating whether the business object
    /// can currently be deleted.
    /// </summary>
    public bool CanDelete
    {
      get
      {
        return _canDelete;
      }
      private set
      {
        if (_canDelete != value)
        {
          _canDelete = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanDelete"));
        }
      }
    }

    private bool _canFetch = false;

    /// <summary>
    /// Gets a value indicating whether an instance
    /// of the business object
    /// can currently be retrieved.
    /// </summary>
    public bool CanFetch
    {
      get
      {
        return _canFetch;
      }
      private set
      {
        if (_canFetch != value)
        {
          _canFetch = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanFetch"));
        }
      }
    }

    private bool _canRemoveItem = false;

    /// <summary>
    /// Gets a value indicating whether the business object
    /// can currently be removed.
    /// </summary>
    public bool CanRemoveItem
    {
      get
      {
        return _canRemoveItem;
      }
      private set
      {
        if (_canRemoveItem != value)
        {
          _canRemoveItem = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanRemoveItem"));
        }
      }
    }

    private bool _canAddNewItem = false;

    /// <summary>
    /// Gets a value indicating whether the business object
    /// can currently be added.
    /// </summary>
    public bool CanAddNewItem
    {
      get
      {
        return _canAddNewItem;
      }
      private set
      {
        if (_canAddNewItem != value)
        {
          _canAddNewItem = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanAddNewItem"));
        }
      }
    }

    private void RefreshCanOperationsValues()
    {
      ITrackStatus targetObject = this.Data as ITrackStatus;
      ICollection list = this.Data as ICollection;
      INotifyBusy busyObject = this.Data as INotifyBusy;
      bool isObjectBusy = false;
      if (busyObject != null && busyObject.IsBusy)
        isObjectBusy = true;
      if (this.Data != null && targetObject != null)
      {

        if (Csla.Security.AuthorizationRules.CanEditObject(this.Data.GetType()) && targetObject.IsSavable)
          this.CanSave = true;
        else
          this.CanSave = false;

        if (Csla.Security.AuthorizationRules.CanEditObject(this.Data.GetType()) && targetObject.IsDirty && !isObjectBusy)
          this.CanCancel = true;
        else
          this.CanCancel = false;

        if (Csla.Security.AuthorizationRules.CanCreateObject(this.Data.GetType()) && !targetObject.IsDirty && !isObjectBusy)
          this.CanCreate = true;
        else
          this.CanCreate = false;

        if (Csla.Security.AuthorizationRules.CanDeleteObject(this.Data.GetType()) && !isObjectBusy)
          this.CanDelete = true;
        else
          this.CanDelete = false;

        if (Csla.Security.AuthorizationRules.CanGetObject(this.Data.GetType()) && !targetObject.IsDirty && !isObjectBusy)
          this.CanFetch = true;
        else
          this.CanFetch = false;

        if (list != null)
        {
          Type itemType = Csla.Utilities.GetChildItemType(this.Data.GetType());
          if (itemType != null)
          {

            if (Csla.Security.AuthorizationRules.CanDeleteObject(itemType) && ((ICollection)this.Data).Count > 0 && !isObjectBusy)
              this.CanRemoveItem = true;
            else
              this.CanRemoveItem = false;

            if (Csla.Security.AuthorizationRules.CanCreateObject(itemType) && !isObjectBusy)
              this.CanAddNewItem = true;
            else
              this.CanAddNewItem = false;
          }
          else
          {
            this.CanAddNewItem = false;
            this.CanRemoveItem = false;
          }
        }
        else
        {
          this.CanRemoveItem = false;
          this.CanAddNewItem = false;
        }
      }
      else if (list != null)
      {
        Type itemType = Csla.Utilities.GetChildItemType(this.Data.GetType());
        if (itemType != null)
        {

          if (Csla.Security.AuthorizationRules.CanDeleteObject(itemType) && ((ICollection)this.Data).Count > 0 && !isObjectBusy)
            this.CanRemoveItem = true;
          else
            this.CanRemoveItem = false;

          if (Csla.Security.AuthorizationRules.CanCreateObject(itemType) && !isObjectBusy)
            this.CanAddNewItem = true;
          else
            this.CanAddNewItem = false;
        }
        else
        {
          this.CanAddNewItem = false;
          this.CanRemoveItem = false;
        }
      }
      else
      {
        this.CanCancel = false;
        this.CanCreate = false;
        this.CanDelete = false;
        this.CanFetch = !this.IsBusy;
        this.CanSave = false;
        this.CanRemoveItem = false;
        this.CanAddNewItem = false;
      }
    }

    #endregion

    #region Can methods that only account for user rights

    private bool _canCreateObject;

    /// <summary>
    /// Gets a value indicating whether the current
    /// user is authorized to create an object.
    /// </summary>
    public bool CanCreateObject
    {
      get { return _canCreateObject; }
      protected set
      {
        _canCreateObject = value;
        OnPropertyChanged(new PropertyChangedEventArgs("CanCreateObject"));
      }
    }

    private bool _canGetObject;

    /// <summary>
    /// Gets a value indicating whether the current
    /// user is authorized to retrieve an object.
    /// </summary>
    public bool CanGetObject
    {
      get { return _canGetObject; }
      protected set
      {
        _canGetObject = value;
        OnPropertyChanged(new PropertyChangedEventArgs("CanGetObject"));
      }
    }

    private bool _canEditObject;

    /// <summary>
    /// Gets a value indicating whether the current
    /// user is authorized to save (insert or update
    /// an object.
    /// </summary>
    public bool CanEditObject
    {
      get { return _canEditObject; }
      protected set
      {
        _canEditObject = value;
        OnPropertyChanged(new PropertyChangedEventArgs("CanEditObject"));
      }
    }

    private bool _canDeleteObject;

    /// <summary>
    /// Gets a value indicating whether the current
    /// user is authorized to delete
    /// an object.
    /// </summary>
    public bool CanDeleteObject
    {
      get { return _canDeleteObject; }
      protected set
      {
        _canDeleteObject = value;
        OnPropertyChanged(new PropertyChangedEventArgs("CanDeleteObject"));
      }
    }

    private void RefreshCanOperationsOnObjectLevel()
    {
      if (Data != null)
      {
        Type sourceType = Data.GetType();
        if (CanCreateObject != Csla.Security.AuthorizationRules.CanCreateObject(sourceType))
          CanCreateObject = Csla.Security.AuthorizationRules.CanCreateObject(sourceType);

        if (CanGetObject != Csla.Security.AuthorizationRules.CanGetObject(sourceType))
          CanGetObject = Csla.Security.AuthorizationRules.CanGetObject(sourceType);

        if (CanEditObject != Csla.Security.AuthorizationRules.CanEditObject(sourceType))
          CanEditObject = Csla.Security.AuthorizationRules.CanEditObject(sourceType);

        if (CanDeleteObject != Csla.Security.AuthorizationRules.CanDeleteObject(sourceType))
          CanDeleteObject = Csla.Security.AuthorizationRules.CanDeleteObject(sourceType);

      }
      else
      {
        this.CanCreateObject = false;
        this.CanDeleteObject = false;
        this.CanGetObject = false;
        this.CanEditObject = false;
      }

    }

    #endregion

    #region INotifyPropertyChanged Members

    /// <summary>
    /// Event raised when a property of the
    /// object has changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="e">
    /// Arguments for event.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (PropertyChanged != null)
      {
        Delegate[] targets = PropertyChanged.GetInvocationList();
        foreach (var oneTarget in targets)
        {
          try
          {
            oneTarget.DynamicInvoke(this, e);
          }
          catch (TargetInvocationException ex)
          {
            if (ex.InnerException != null && ex.InnerException is NullReferenceException)
            {
              //TODO: should revisit after RTM - should uncomment code below
              // can be thrown due to bug in SL
            }
            else
              throw;
          }
        }
      }
      //if (PropertyChanged != null)
      //  PropertyChanged(this, e);
    }

    #endregion
  }
}
