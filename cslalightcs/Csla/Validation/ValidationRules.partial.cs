﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Csla.Core;
using Csla.Serialization.Mobile;
using System.Collections.Generic;

namespace Csla.Validation
{
  public partial class ValidationRules : IUndoableObject
  {
    public ValidationRules() { }

    #region IUndoableObject Members

    private Stack<SerializationInfo> _stateStack = new Stack<SerializationInfo>();

    int IUndoableObject.EditLevel
    {
      get { return _stateStack.Count; }
    }

    void IUndoableObject.CopyState(int parentEditLevel, bool parentBindingEdit)
    {
      if (((IUndoableObject)this).EditLevel + 1 > parentEditLevel)
        throw new UndoException(string.Format(Properties.Resources.EditLevelMismatchException, "CopyState"));

      SerializationInfo state = new SerializationInfo(0);
      OnGetState(state, StateMode.Undo);

      if (_brokenRules != null && _brokenRules.Count > 0)
      {
        byte[] rules = MobileFormatter.Serialize(_brokenRules);
        state.AddValue("_rules", Convert.ToBase64String(rules));
      }

      _stateStack.Push(state);
    }

    void IUndoableObject.UndoChanges(int parentEditLevel, bool parentBindingEdit)
    {
      if (((IUndoableObject)this).EditLevel > 0)
      {
        if (((IUndoableObject)this).EditLevel - 1 < parentEditLevel)
          throw new UndoException(string.Format(Properties.Resources.EditLevelMismatchException, "UndoChanges"));

        SerializationInfo state = _stateStack.Pop();
        OnSetState(state, StateMode.Undo);
        
        lock(SyncRoot)
          _brokenRules = null;

        if (state.Values.ContainsKey("_rules"))
        {
          byte[] rules = Convert.FromBase64String(state.GetValue<string>("_rules"));

          lock(SyncRoot)
            _brokenRules = (BrokenRulesCollection)MobileFormatter.Deserialize(rules);
        }
      }
    }

    void IUndoableObject.AcceptChanges(int parentEditLevel, bool parentBindingEdit)
    {
      if (((IUndoableObject)this).EditLevel - 1 < parentEditLevel)
        throw new UndoException(string.Format(Properties.Resources.EditLevelMismatchException, "AcceptChanges"));

      if (((IUndoableObject)this).EditLevel > 0)
      {
        // discard latest recorded state
        _stateStack.Pop();
      }
    }

    #endregion
  }
}
