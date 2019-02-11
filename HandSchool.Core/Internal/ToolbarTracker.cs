﻿using HandSchool.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace HandSchool.Internals
{
    /// <summary>
    /// 用于跟踪任务栏菜单改变。
    /// </summary>
    public class ToolbarMenuTracker
    {
        ObservableCollection<MenuEntry> _inner;
        NotifyCollectionChangedEventHandler _handler;
        EventHandler _todo;

        /// <summary>
        /// 保存的列表
        /// </summary>
        public ObservableCollection<MenuEntry> List
        {
            get
            {
                return _inner;
            }

            set
            {
                if (_handler != null && _inner != null)
                {
                    _inner.CollectionChanged -= _handler;
                }

                _inner = value;

                if (value != null && _handler != null)
                {
                    value.CollectionChanged += _handler;
                }

                _todo?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 菜单改变时发生。
        /// </summary>
        public event EventHandler Changed
        {
            add
            {
                _todo = value;
                _handler = new NotifyCollectionChangedEventHandler(value);
                if (_inner != null) _inner.CollectionChanged += _handler;
            }

            remove
            {
                if (_inner != null) _inner.CollectionChanged -= _handler;
                _handler = null;
                _todo = null;
            }
        }
    }
}