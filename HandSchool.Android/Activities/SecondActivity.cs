﻿using Android.App;
using HandSchool.Views;
using System;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace HandSchool.Droid
{
    [Activity(Theme = "@style/AppTheme.NoActionBar")]
    [BindView(Resource.Layout.activity_popup)]
    public class SecondActivity : BaseActivity
    {
        protected override void OnNavigatedParameter(object obj)
        {
            if (obj is SupportFragment fragment)
            {
                Transaction(fragment);
            }
            else if (obj is ViewObject viewObject)
            {
                Transaction(viewObject);
            }
            else if (obj is ValueTuple<Type, object> param2)
            {
                var type = NavMenuItemV2.Judge(param2.Item1);

                switch (type)
                {
                    case NavMenuItemType.FragmentCore:
                        var fg = Core.Reflection.CreateInstance<SupportFragment>(param2.Item1);
                        Transaction(fg);
                        break;

                    case NavMenuItemType.Fragment:
                        var vf = Core.Reflection.CreateInstance<ViewFragment>(param2.Item1);
                        vf.SetNavigationArguments(param2.Item2);
                        Transaction(vf);
                        break;

                    case NavMenuItemType.Object:
                        var vo = Core.Reflection.CreateInstance<ViewObject>(param2.Item1);
                        vo.SetNavigationArguments(param2.Item2);
                        Transaction(vo);
                        break;

                    case NavMenuItemType.Presenter:
                        var vp = Core.Reflection.CreateInstance<IViewPresenter>(param2.Item1);
                        Transaction(new TabbedFragment(vp));
                        break;

                    default:
                        break;
                }
            }
            
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
    }
}