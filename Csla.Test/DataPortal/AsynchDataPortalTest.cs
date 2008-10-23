﻿#if SILVERLIGHT
using Csla.DataPortalClient;
#else
using Csla.Test.Basic;
#endif
using System;
#if NUNIT
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif


using Csla;
using Csla.Core;
using cslalighttest.CslaDataProvider;
using UnitDriven;

using Single=Csla.Test.DataPortalTest.Single;

namespace Csla.Test.DataPortal
{
  [TestClass]
  public class AsynchDataPortalTest : TestBase
  {
#if SILVERLIGHT
    [TestInitialize]
    public void Setup()
    {
      Csla.DataPortal.ProxyTypeName = typeof(SynchronizedWcfProxy<>).AssemblyQualifiedName;
      Csla.DataPortalClient.WcfProxy.DefaultUrl = cslalighttest.Properties.Resources.RemotePortalUrl;
    }
#endif

    #region Create

    [TestMethod]
    public void BeginCreate_overload_without_parameters_Results_in_UserState_defaulted_to_Null_and_Id_to_0()
    {
      var context = GetContext();

      Csla.DataPortal.BeginCreate<Single>((o, e) =>
      {
        var created = e.Object; 
        context.Assert.IsNotNull(created);
        context.Assert.AreEqual(created.Id, 0);//DP_Create without criteria called
        context.Assert.IsNull(e.Error);
        context.Assert.IsNull(e.UserState);
        context.Assert.AreEqual("Created", created.MethodCalled);
        context.Assert.Success();
      });
      context.Complete();
    }

    [TestMethod]
    public void BeginCreate_overload_with_UserState_passed_Results_in_UserState_set_and_Id_defaulted_to_0()
    {
      var context = GetContext();
      object userState = "state";
      Csla.DataPortal.BeginCreate<Single>(
        (o, e) =>
        {
          var created = e.Object;
          context.Assert.IsNotNull(created);
          context.Assert.AreEqual(created.Id, 0);//DP_Create without criteria called
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual(userState, e.UserState);
          context.Assert.AreEqual("Created", created.MethodCalled);
          context.Assert.Success();
        }, userState);
      context.Complete();
    }

    [TestMethod]
    public void BeginCreate_overload_with_UserState_and_Criteria_passed_Results_in_UserState_and_Id_set()
    {
      var context = GetContext();
      object userState = "state";
      Csla.DataPortal.BeginCreate<Single>(
        new Single.Criteria(100),
        (o, e) =>
        {
          var created = e.Object;
          context.Assert.IsNotNull(created);
          context.Assert.AreEqual(created.Id, 100);//DP_Create with criteria called
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual(userState, e.UserState);
          context.Assert.AreEqual("Created", created.MethodCalled);
          context.Assert.Success();
        }, userState);
      context.Complete();
    }

    #endregion

    #region Fetch
    [TestMethod]
    public void BeginFetch_overload_without_Parameters_Results_in_UserState_defaulted_to_Null_and_Id_to_0()
    {
      var context = GetContext();
      Csla.DataPortal.BeginFetch<Single>(
        (o, e) =>
        {
          var fetched = e.Object;
          context.Assert.IsNotNull(fetched);
          context.Assert.AreEqual(fetched.Id, 0);//DP_Create without criteria called
          context.Assert.IsNull(e.Error);
          context.Assert.IsNull(e.UserState);
          context.Assert.AreEqual("Fetched", fetched.MethodCalled);
          context.Assert.Success();
        });
      context.Complete();
    }

    [TestMethod]
    public void BeginFetch_overload_with_Crieria_only_passed_Results_in_UserState_defaulted_to_Null_and_Id_set()
    {
      var context = GetContext();
      Csla.DataPortal.BeginFetch<Single>(
        new Single.Criteria(5), 
        (o, e) =>
        {
          var fetched = e.Object;
          context.Assert.IsNotNull(fetched);
          context.Assert.AreEqual(fetched.Id, 5);//DP_Create with criteria called
          context.Assert.IsNull(e.Error);
          context.Assert.IsNull(e.UserState);
          context.Assert.AreEqual("Fetched", fetched.MethodCalled);
          context.Assert.Success();
        });
      context.Complete();
    }

    [TestMethod]
    public void BeginFetch_with_Criteria_and_UserState_passed_Results_in_UserState_and_Id_set()
    {
      var context = GetContext();
      object userState = "state";
      Csla.DataPortal.BeginFetch<Single>(
        new Single.Criteria(5), 
        (o, e) =>
        {
          var fetched = e.Object;
          context.Assert.IsNotNull(fetched);
          context.Assert.AreEqual(fetched.Id, 5);//DP_Create with criteria called
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual(userState, e.UserState);
          context.Assert.AreEqual("Fetched", fetched.MethodCalled);
          context.Assert.Success();
        }, 
        userState);
      context.Complete();
    }

    #endregion

    #region BeginSave
    [TestMethod]
    public void BeginSave_overload_called_on_NewObject_without_parameters_Results_in_UserState_dafaulted_to_Null_and_MethodCalled_Inserted()
    {
      var context = GetContext();
      Csla.DataPortal.BeginCreate<Single>((o, e) =>
      {
        var test = e.Object;
        context.Assert.IsNotNull(test);
        context.Assert.AreEqual("Created", e.Object.MethodCalled);
        test.Saved += ((o1, e1) =>
        {
          var actual = e1.NewObject;
          context.Assert.IsNotNull(actual);
          //if force update was set to false we result in Inserted object otherwise Updated
          context.Assert.AreEqual("Inserted", ((Single)actual).MethodCalled);
          context.Assert.IsNull(e1.Error);
          context.Assert.IsNull(e1.UserState);
          context.Assert.Success();
        });
        test.BeginSave();

      });
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_overload_called_on_NewObject_with_callback_parameter_set_Results_in_UserState_defaulted_to_Null_and_id_to_0_and_MethodCalled_Inserted()
    {
      var context = GetContext();
      Csla.DataPortal.BeginCreate<Single>(
        (o, e) =>
        {
          var created = e.Object;
          context.Assert.IsNotNull(created);
          context.Assert.IsNull(e.Error);
          context.Assert.IsNull(e.UserState);
          context.Assert.AreEqual("Created", created.MethodCalled);
          created.BeginSave((o1, e1) =>
          {
            var saved = (Single)e1.NewObject;
            context.Assert.IsNotNull(saved);
            context.Assert.AreEqual(saved.Id, 0);//DP_Create without criteria called
            context.Assert.AreEqual("Inserted", saved.MethodCalled);
            context.Assert.IsNull(e1.Error);
            context.Assert.IsNull(e1.UserState);
            context.Assert.Success();
          });
        });
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_overload_called_on_NewObject_with_UserState_parameter_set_Results_in_UserState_set_and_MethodCalled_Inserted()
    {
      var userState = "user";
      var context = GetContext();
      Csla.DataPortal.BeginCreate<Single>((o, e) =>
      {
        var test = e.Object;
        context.Assert.IsNotNull(test);
        context.Assert.AreEqual("Created", e.Object.MethodCalled);
        test.Saved += ((o1, e1) =>
        {
          var actual = (Single)e1.NewObject;
          context.Assert.IsNotNull(actual);
          //if force update was set to false we result in Inserted object otherwise Updated
          context.Assert.AreEqual("Inserted", actual.MethodCalled);
          context.Assert.IsNull(e1.Error);
          context.Assert.AreEqual(userState, e1.UserState);
          context.Assert.Success();
        });

        test.BeginSave(userState);

      });
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_overload_called_on_NewObject_with_UserState_and_calllback_Results_in_UserState_set_and_MethodCalled_Inserted()
    {
      var context = GetContext();
      object expectedUserState = "state";
      Csla.DataPortal.BeginCreate<Single>(
        (o, e) =>
        {
          var created = e.Object;
          context.Assert.IsNotNull(created);
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual(expectedUserState, e.UserState);
          context.Assert.AreEqual("Created", created.MethodCalled);
          created.BeginSave((o1, e1) =>
          {
            var saved = (Single)e1.NewObject;
            context.Assert.IsNotNull(saved);
            context.Assert.AreEqual("Inserted", saved.MethodCalled);
            context.Assert.IsNull(e1.Error);
            context.Assert.AreEqual(expectedUserState, e1.UserState);
            context.Assert.Success();
          }, expectedUserState);
        }, 
        expectedUserState);
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_overload_called_on_NewObject_with_ForceUpdate_callback_and_UserState_Parameters_set_Results_in_those_params_set_on_server()
    {
      var userState = "user";
      var forceUpdate = true;
      var context = GetContext();
      Csla.DataPortal.BeginCreate<Single>((o, e) =>
      {
        context.Assert.IsNotNull(e.Object);
        context.Assert.AreEqual("Created", e.Object.MethodCalled);
        e.Object.BeginSave(
          forceUpdate, 
          (o1, e1) =>
          {
            var actual = (Single)e1.NewObject;
            context.Assert.IsNotNull(actual);
            //if force update was set to false we result in Inserted object otherwise Updated
            context.Assert.AreEqual("Updated", actual.MethodCalled);
            context.Assert.IsNull(e1.Error);
            context.Assert.AreEqual(userState, e1.UserState);
            context.Assert.Success();
          }, 
          userState);
      });
      context.Complete();
    }


    [TestMethod]
    public void BeginSave_overload_called_on_FetchedObject_without_parameters_Results_in_UserState_defaulted_to_Null_and_MethodCalled_Updated()
    {
      var context = GetContext();
      Csla.DataPortal.BeginFetch<Single>(
        (o, e) =>
        {
          var created = e.Object;
          context.Assert.IsNotNull(created);
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual("Fetched", created.MethodCalled);
          created.MethodCalled = "";
          created.BeginSave((o1, e1) =>
          {
            var saved = (Single)e1.NewObject;
            context.Assert.IsNotNull(saved);
            context.Assert.AreEqual("Updated", saved.MethodCalled);
            context.Assert.IsNull(e1.Error);
            context.Assert.IsNull(e1.UserState);
            context.Assert.Success();
          });
        });
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_overload_called_on_FetchedObject_with_UserState_results_in_UserState_set_and_MethodCalled_Updated()
    {
      var context = GetContext();
      object userState = "state";
      Csla.DataPortal.BeginFetch<Single>(
        (o, e) =>
        {
          var fetched = e.Object;
          context.Assert.IsNotNull(fetched);
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual(userState, e.UserState);
          context.Assert.AreEqual("Fetched", fetched.MethodCalled);
          fetched.MethodCalled = "";
          fetched.BeginSave((o1, e1) =>
          {
            var saved = (Single)e1.NewObject;
            context.Assert.IsNotNull(saved);
            context.Assert.AreEqual(saved.Id, 0);//DP_Create without criteria called
            context.Assert.AreEqual("Updated", saved.MethodCalled);
            context.Assert.IsNull(e1.Error);
            context.Assert.AreEqual(userState, e1.UserState);
            context.Assert.Success();
          }, 
          userState);
        }, 
        userState);
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_overload_called_on_DeletedObject_with_UserState_results_in_UserState_set_on_server()
    {
      var context = GetContext();
      object userState = "state";
      Csla.DataPortal.BeginFetch<Single>(
        (o, e) =>
        {
          var fetched = e.Object;
          context.Assert.IsNotNull(fetched);
          context.Assert.IsNull(e.Error);
          context.Assert.AreEqual(userState, e.UserState);
          context.Assert.AreEqual("Fetched", fetched.MethodCalled);
          fetched.MethodCalled = "";
          fetched.Delete();
          fetched.BeginSave((o1, e1) =>
          {
            var saved = e1.NewObject;
            context.Assert.IsNotNull(saved);
            context.Assert.AreEqual("SelfDeleted", ((Single)saved).MethodCalled);
            context.Assert.IsNull(e1.Error);
            context.Assert.AreEqual(userState, e1.UserState);
            context.Assert.Success();
          }, 
          userState);
        }, 
        userState);
      context.Complete();
    }

    [TestMethod]
    public void BeginSave_called_on_DeletedObject_without_UserState_results_in_UserState_defaulted_to_Null_server()
    {
      var context = GetContext();
      Csla.DataPortal.BeginFetch<Single>(
        (o, e) =>
                            {
                              context.Assert.IsNotNull(e.Object);
                              context.Assert.IsNull(e.Error);
                              context.Assert.IsNull(e.UserState);
                              context.Assert.AreEqual("Fetched", e.Object.MethodCalled);
                              e.Object.MethodCalled = "";
                              e.Object.Delete();
                              e.Object.BeginSave((o1, e1) =>
                                                   {
                                                     context.Assert.IsNotNull(e1.NewObject);
                                                     context.Assert.AreEqual("SelfDeleted", ((Single)e1.NewObject).MethodCalled);
                                                     context.Assert.IsNull(e1.Error);
                                                     context.Assert.IsNull(e1.UserState);
                                                     context.Assert.Success();
                                                   });
                            });
      context.Complete();
    }

    #endregion

    #region Delete

    [TestMethod]
    public void Delete_called_with_UserState_results_in_UserState_set_on_server()
    {
      var context = GetContext();
      object userState = "state";
      Single.DeleteObject(5, (o1, e1) =>
                               {
                                 context.Assert.IsNull(e1.Error);
                                 context.Assert.AreEqual(userState, e1.UserState);
                                 context.Assert.Success();
                               }, userState);
      context.Complete();
    }

    [TestMethod]
    public void Delete_called_without_UserState_results_in_UserState_defaulted_to_Null_server()
    {
      var context = GetContext();
      Single.DeleteObject(5, (o1, e1) =>
                               {
                                 context.Assert.IsNull(e1.Error);
                                 context.Assert.IsNull(e1.UserState);
                                 context.Assert.Success();
                               });
      context.Complete();
    }

    #endregion

    #region ExecuteCommand

#if !SILVERLIGHT
    [TestMethod]
    public void ExecuteCommand_called_with_UserState_results_in_UserState_set_on_server()
    {
      var context = GetContext();
      object userState = "state";
      var command = new CommandObject();
      command.ExecuteServerCodeAsunch((o1, e1) =>
                                        {
                                          context.Assert.IsNull(e1.Error);
                                          context.Assert.AreEqual("Executed", e1.Object.AProperty);
                                          context.Assert.AreEqual(userState, e1.UserState);
                                          context.Assert.Success();
                                        }, userState);
      context.Complete();
    }

    [TestMethod]
    public void ExecuteCommand_called_without_UserState_results_in_UserState_defaulted_to_Null_server()
    {
      var context = GetContext();
      var command = new CommandObject();
      command.ExecuteServerCodeAsunch((o1, e1) =>
                                        {
                                          context.Assert.IsNull(e1.Error);
                                          context.Assert.AreEqual("Executed", e1.Object.AProperty);
                                          context.Assert.IsNull(e1.UserState);
                                          context.Assert.Success();
                                        });
      context.Complete();
    }

#endif

    #endregion


    /// <summary>
    /// Create is an exception - called with SingleCriteria, if BO does not have DP_Create() overload
    /// with that signature, ends up calling parameterless DP_Create() - this is by design
    /// </summary>
    [TestMethod]
    public void BeginCreate_with_SingleCriteria_Calling_BO_Without_DP_CREATE_Returns_no_Error_info()
    {
      var context = GetContext();
      CustomerWO_DP_XYZ.CreateCustomer((o, e) =>
      {
        context.Assert.IsNull(e.Error);
        context.Assert.Success();
      });
      context.Complete();
    }

  }
}