﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace HandSchool.Internals
{
    /// <summary>
    /// 反射功能的拓展方法，方便获取 Attribute 信息。
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// 从类成员中反射获取设置属性。
        /// </summary>
        /// <param name="info">成员信息</param>
        /// <returns>设置的特性。</returns>
        public static TAttribute Get<TAttribute>(this MemberInfo info, bool @checked = true) where TAttribute : Attribute
        {
            var ret = info.GetCustomAttribute(typeof(TAttribute), false) as TAttribute;
            if (@checked && ret is null) throw new InvalidOperationException(nameof(TAttribute) + " is not attached to " + info.Name);
            return ret;
        }
        
        /// <summary>
        /// 从类成员中反射获取设置属性。
        /// </summary>
        /// <param name="info">成员信息</param>
        /// <returns>特性</returns>
        public static IEnumerable<TAttribute> Gets<TAttribute>(this ICustomAttributeProvider info) where TAttribute : Attribute
        {
            foreach (var obj in info.GetCustomAttributes(typeof(TAttribute), false))
            {
                if (obj is TAttribute attr) yield return attr;
            }
        }

        /// <summary>
        /// 从类成员中反射获取设置属性。
        /// </summary>
        /// <param name="info">成员信息</param>
        /// <returns>设置的特性。</returns>
        public static bool Has<TAttribute>(this MemberInfo info) where TAttribute : Attribute
        {
            return info.GetCustomAttribute(typeof(TAttribute), false) != null;
        }

        /// <summary>
        /// 获取某对象的类型所在程序集。
        /// </summary>
        /// <param name="type">对象</param>
        /// <returns>程序集</returns>
        public static Assembly GetAssembly(this object type)
        {
            return type.GetType().GetTypeInfo().Assembly;
        }
    }
}