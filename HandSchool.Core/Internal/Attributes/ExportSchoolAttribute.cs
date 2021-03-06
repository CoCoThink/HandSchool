﻿using System;

namespace HandSchool.Internals
{
    /// <summary>
    /// 学校程序集导出学校的标志。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ExportSchoolAttribute : Attribute
    {
        /// <summary>
        /// 注册的学校类型
        /// </summary>
        public Type RegisterType { get; }

        /// <summary>
        /// 注册学校加载器，表示此学校已可用。
        /// </summary>
        /// <param name="registerType">注册的学校加载器。</param>
        public ExportSchoolAttribute(Type registerType)
        {
            RegisterType = registerType;
        }
    }
}