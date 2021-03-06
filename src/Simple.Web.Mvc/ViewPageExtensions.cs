﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Simple.Web.Mvc
{
    public static class ViewPageExtensions
    {
        public static object RouteParam(this ViewUserControl page, string name)
        {
            return page.RouteParam<object>(name);
        }

        public static T RouteParam<T>(this ViewUserControl page, string name)
        {
            object obj = page.ViewContext.RouteData.Values[name];

            if (obj is T)
                return (T)obj;
            else if (obj != null)
                return (T)Convert.ChangeType(obj, typeof(T));
            else
                return default(T);
        }

        public static object RouteParam(this ViewPage page, string name)
        {
            return page.RouteParam<object>(name);
        }

        public static T RouteParam<T>(this ViewPage page, string name)
        {
            object obj = page.ViewContext.RouteData.Values[name];

            if (obj is T)
                return (T)obj;
            else if (obj != null)
                return (T)Convert.ChangeType(obj, typeof(T));
            else
                return default(T);
        }
    }
}
