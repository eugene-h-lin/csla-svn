using System;
using System.Collections.Generic;
using System.Text;

#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#endif 

namespace Csla.Test.IdentityConverter
{
  [TestClass]
  public class IdentityConverterTests
  {
    [TestMethod]
    public void ConvertObjRef()
    {
      Csla.Wpf.IdentityConverter conv = new Csla.Wpf.IdentityConverter();
      object test = new object();
      object result = conv.Convert(test, null, null, null);

      Assert.IsTrue(ReferenceEquals(test, result), "Object references should be the same");
    }

    [TestMethod]
    public void ConvertValue()
    {
      Csla.Wpf.IdentityConverter conv = new Csla.Wpf.IdentityConverter();
      object result = conv.Convert(123, null, null, null);

      Assert.AreEqual(123, (int)result, "Value should be 123");
    }

    [TestMethod]
    public void ConvertBackObjRef()
    {
      Csla.Wpf.IdentityConverter conv = new Csla.Wpf.IdentityConverter();
      object test = new object();
      object result = conv.ConvertBack(test, null, null, null);

      Assert.IsTrue(ReferenceEquals(test, result), "Object references should be the same");
    }

    [TestMethod]
    public void ConvertBackValue()
    {
      Csla.Wpf.IdentityConverter conv = new Csla.Wpf.IdentityConverter();
      object result = conv.ConvertBack(123, null, null, null);

      Assert.AreEqual(123, (int)result, "Value should be 123");
    }
  }
}
