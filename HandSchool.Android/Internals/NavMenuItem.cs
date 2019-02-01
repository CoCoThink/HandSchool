﻿using HandSchool.Internal;
using HandSchool.Models;
using HandSchool.Views;
using System;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace HandSchool.Droid
{
    public enum NavMenuItemType
    {
        Others,
        FragmentCore,
        Fragment,
        Activity,
        Presenter,
        Object,
    }

    public class NavMenuItemV2 : NavigationMenuItem
    {
        public NavMenuItemType Type { get; }

        public int DrawableId { get; }

        public static readonly int[] IconList = new[]
        {
            Resource.Drawable.ic_nav_home,
            Resource.Drawable.ic_nav_sched,
            Resource.Drawable.ic_nav_feed,
            Resource.Drawable.ic_nav_info,
            Resource.Drawable.ic_nav_feed,
            Resource.Drawable.ic_nav_grade,
            Resource.Drawable.ic_nav_card,
            Resource.Drawable.ic_nav_settings,
            Resource.Drawable.ic_nav_about,
        };

        public IViewPresenter CreatePresenter()
        {
            if (Type != NavMenuItemType.Presenter) throw new InvalidCastException();
            return Core.Reflection.CreateInstance<IViewPresenter>(PageType);
        }

        public ViewFragment CreateFragment()
        {
            if (Type != NavMenuItemType.Fragment) throw new InvalidCastException();
            return Core.Reflection.CreateInstance<ViewFragment>(PageType);
        }

        public ViewObject CreateObject()
        {
            if (Type != NavMenuItemType.Object) throw new InvalidCastException();
            return Core.Reflection.CreateInstance<ViewObject>(PageType);
        }

        public SupportFragment CreateFragmentCore()
        {
            if (Type != NavMenuItemType.FragmentCore) throw new InvalidCastException();
            return Core.Reflection.CreateInstance<SupportFragment>(PageType);
        }

        public BaseActivity CreateActivity()
        {
            if (Type != NavMenuItemType.Activity) throw new InvalidCastException();
            return Core.Reflection.CreateInstance<BaseActivity>(PageType);
        }

        public static NavMenuItemType Judge(Type PageType)
        {
            if (typeof(IViewPresenter).IsAssignableFrom(PageType))
                return NavMenuItemType.Presenter;
            else if (typeof(ViewFragment).IsAssignableFrom(PageType))
                return NavMenuItemType.Fragment;
            else if (typeof(SupportFragment).IsAssignableFrom(PageType))
                return NavMenuItemType.FragmentCore;
            else if (typeof(ViewObject).IsAssignableFrom(PageType))
                return NavMenuItemType.Object;
            else if (typeof(BaseActivity).IsAssignableFrom(PageType))
                return NavMenuItemType.Activity;
            else
                return NavMenuItemType.Others;
        }

        public NavMenuItemV2(string title, string dest, string category, MenuIcon icon) : base(title, dest, category)
        {
            Type = Judge(PageType);
            DrawableId = IconList[(int)icon];
        }
    }
}