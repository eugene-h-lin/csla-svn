﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Csla.Test.ValidationRules
{
  public partial class HasRulesManager
  {
    private HasRulesManager() : base()
    {
      //prevent direct creation
      AddBusinessRules();
    }

    private void DataPortal_Create(object criteria)
    {
      Criteria crit = (Criteria)(criteria);
      _name = crit._name;
      //Name = crit._name;
      Csla.ApplicationContext.GlobalContext.Add("HasRulesManager", "Created");
      this.ValidationRules.CheckRules();
    }

    protected override void DataPortal_Fetch(object criteria)
    {
      Criteria crit = (Criteria)(criteria);
      _name = crit._name;
      MarkOld();
      Csla.ApplicationContext.GlobalContext.Add("HasRulesManager", "Fetched");
    }

    protected override void DataPortal_Update()
    {
      if (IsDeleted)
      {
        //we would delete here
        Csla.ApplicationContext.GlobalContext.Add("HasRulesManager", "Deleted");
        MarkNew();
      }
      else
      {
        if (this.IsNew)
        {
          //we would insert here
          Csla.ApplicationContext.GlobalContext.Add("HasRulesManager", "Inserted");
        }
        else
        {
          //we would update here
          Csla.ApplicationContext.GlobalContext.Add("HasRulesManager", "Updated");
        }
        MarkOld();
      }
    }
  }
}
