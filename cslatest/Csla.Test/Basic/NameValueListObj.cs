using System;
using System.Collections.Generic;
using System.Text;

namespace Csla.Test.Basic
{
    [Serializable()]
    public class NameValueListObj : NameValueListBase<int, string>
    {
        private NameValueListObj()
        {
            //require factory method
        }

        #region "factory methods"

        public static NameValueListObj GetNameValueListObj()
        {
            return Csla.DataPortal.Fetch<NameValueListObj>(new Criteria(typeof(NameValueListObj)));
        }

        #endregion

        #region "Data Access"

        protected override void DataPortal_Fetch(object criteria)
        {
            Csla.ApplicationContext.GlobalContext.Clear();
            Csla.ApplicationContext.GlobalContext.Add("NameValueListObj", "Fetched");

            this.IsReadOnly = false;
            for (int i = 0; i < 10; i++)
            {
                this.Add(new NameValuePair(i, "element_" + i.ToString()));
            }
            this.IsReadOnly = true;
        }

        #endregion
    }
}
