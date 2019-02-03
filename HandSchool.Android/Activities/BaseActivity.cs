﻿using Android.OS;
using Android.Support.V7.App;
using HandSchool.ViewModels;
using HandSchool.Views;
using System;
using System.Collections.Generic;
using HandSchool.Internal;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;
using SupportFragment = Android.Support.V4.App.Fragment;
using AToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Support.Design.Widget;
using System.Collections.ObjectModel;

namespace HandSchool.Droid
{
    public class BaseActivity : AppCompatActivity, INavigate
    {
        public BaseViewModel ViewModel
        {
            get => _viewModel;
            set => SetViewModel(value);
        }

        private void SetViewModel(BaseViewModel value)
        {
            if (_viewModel != null)
            {
                //_viewModel.IsBusyChanged -= s;
                // _viewModel.
            }

            _viewModel = value;
        }

        private BaseViewModel _viewModel;

        [BindView(Resource.Id.toolbar)]
        public AToolbar Toolbar { get; set; }

        [BindView(Resource.Id.main_progress_bar)]
        public ProgressBar ProgressBar { get; set; }

        [BindView(Resource.Id.appbar_layout)]
        public AppBarLayout AppBarLayout { get; set; }

        [BindView(Resource.Id.sliding_tabs)]
        public TabLayout Tabbar { get; set; }

        protected int ContentViewResource { get; set; }

        #region Fragment Transaction

        private ToolbarMenuTracker MenuTracker { get; set; }

        private Guid? ViewObjectIdentity;

        private static readonly Dictionary<Guid, ViewObject>
            TransactionSource = new Dictionary<Guid, ViewObject>();

        protected void Transaction(SupportFragment fragment)
        {
            RemoveViewObject();

            if (fragment is IViewCore core)
            {
                SupportActionBar.Title = core.Title;
                ViewModel = core.ViewModel;
            }

            if (fragment is TabbedFragment tabbed)
            {
                tabbed.Tabbar = Tabbar;
            }

            var transition = SupportFragmentManager.BeginTransaction();
            transition.Replace(Resource.Id.frame_layout, fragment);
            transition.Commit();

            if (!(fragment is TabbedFragment))
            {
                Tabbar.Visibility = ViewStates.Gone;
            }

            ReloadToolbarMenu(this, EventArgs.Empty);
        }

        protected void Transaction(ViewFragment fragment)
        {
            fragment.RegisterNavigation(this);
            Transaction(fragment as SupportFragment);
            MenuTracker = fragment.ToolbarMenu;
            MenuTracker.Changed += ReloadToolbarMenu;
        }

        protected void Transaction(ViewObject viewPage)
        {
            var internalPage = new Xamarin.Forms.ContentPage
            {
                Content = viewPage.Content,
                BindingContext = viewPage.ViewModel,
            };

            viewPage.RegisterNavigation(this);
            SupportActionBar.Title = viewPage.Title;
            //viewPage.PropertyChanged += ViewPropChanged;
            ViewModel = viewPage.ViewModel;

            Transaction(internalPage.CreateSupportFragment(this));
            MenuTracker = viewPage.ToolbarTracker;
            MenuTracker.Changed += ReloadToolbarMenu;
            var currentIdentity = Guid.NewGuid();
            TransactionSource.Add(currentIdentity, viewPage);
            ViewObjectIdentity = currentIdentity;
        }

        private void RemoveViewObject()
        {
            // If the activity is using a viewObject...
            if (ViewObjectIdentity.HasValue)
            {
                //var vm = TransactionSource[ViewObjectIdentity.Value].ViewModel;
                //vm.PropertyChanged -= ViewPropChanged;
                TransactionSource.Remove(ViewObjectIdentity.Value);
                ViewObjectIdentity = null;
            }

            if (MenuTracker != null)
            {
                MenuTracker.Changed -= ReloadToolbarMenu;
                MenuTracker = null;
            }
        }

        private void ReloadToolbarMenu(object sender, EventArgs args)
        {
            InvalidateOptionsMenu();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (MenuTracker != null)
            {
                foreach (var entry in MenuTracker.List)
                {
                    if (entry.HiddenForPull) continue;

                    var menuItem = menu.Add(entry.Title);
                    
                    if (entry.Order != Xamarin.Forms.ToolbarItemOrder.Secondary)
                        menuItem.SetShowAsAction(ShowAsAction.Always);
                    menuItem.SetOnMenuItemClickListener(new MenuEntryClickedListener(entry));
                }
            }

            return base.OnCreateOptionsMenu(menu);
        }

        #endregion

        #region Activity Transaction

        const string BroadcastedArgument = "PARAMGUID";

        private static readonly Dictionary<Guid, object>
            ArgumentBroadcastSource = new Dictionary<Guid, object>();

        protected virtual void OnNavigatedParameter(object obj) { }

        Task INavigate.PushAsync(string pageType, object param)
        {
            var type = Core.Reflection.TryGetType(pageType);

            if (type is null)
            {
                Core.Logger.WriteLine("NavImpl", pageType + " not found.");
                return Task.CompletedTask;
            }

            return (this as INavigate).PushAsync(type, param);
        }
        
        Task INavigate.PushAsync(Type pageType, object param)
        {
            pageType = Core.Reflection.TryGetType(pageType);

            if (typeof(AppCompatActivity).IsAssignableFrom(pageType))
            {
                // When the page type is aimed at an activity.
                var intent = new Intent(this, pageType);
                var guid = Guid.NewGuid();
                ArgumentBroadcastSource.Add(guid, param);
                intent.PutExtra(BroadcastedArgument, guid.ToByteArray());
                StartActivity(intent);
            }
            else
            {
                // When the page type is aimed at an fragment.
                var intent = new Intent(this, typeof(SecondActivity));
                var guid = Guid.NewGuid();
                var param2 = (pageType, param);
                ArgumentBroadcastSource.Add(guid, param2);
                intent.PutExtra(BroadcastedArgument, guid.ToByteArray());
                StartActivity(intent);
            }
            
            return Task.CompletedTask;
        }

        #endregion
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(ContentViewResource);

            foreach (var prop in GetType().GetProperties())
            {
                if (prop.Has<BindViewAttribute>())
                {
                    var attr = prop.Get<BindViewAttribute>();
                    prop.SetValue(this, FindViewById(attr.ResourceId));
                }
            }
            
            if (!(Toolbar.Parent is CollapsingToolbarLayout))
                SetSupportActionBar(Toolbar);
            Toolbar.SetNavigationOnClickListener(new ToolbarBackListener(this));
            PlatformImplV2.Instance.SetViewResponseImpl(new Elements.ViewResponseImpl(this));

            if (Intent.HasExtra(BroadcastedArgument))
            {
                // notice that this activity conveys an argument.
                var guid = new Guid(Intent.GetByteArrayExtra(BroadcastedArgument));
                OnNavigatedParameter(ArgumentBroadcastSource[guid]);
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveViewObject();
            PlatformImplV2.Instance.SetViewResponseImpl(null);
        }

        private class ToolbarBackListener : Java.Lang.Object, View.IOnClickListener
        {
            public WeakReference<BaseActivity> Activity { get; }

            public ToolbarBackListener(BaseActivity activity)
            {
                Activity = new WeakReference<BaseActivity>(activity);
            }

            public void OnClick(View v)
            {
                if (!Activity.TryGetTarget(out var activity)) return;

                if (activity.SupportFragmentManager.BackStackEntryCount > 0)
                {
                    activity.SupportFragmentManager.PopBackStack();
                }
                else
                {
                    activity.Finish();
                }
            }
        }

        private class MenuEntryClickedListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
        {
            public MenuEntryClickedListener(MenuEntry item)
            {
                Reference = new WeakReference<MenuEntry>(item);
            }

            public WeakReference<MenuEntry> Reference { get; }

            public bool OnMenuItemClick(IMenuItem item)
            {
                if (Reference.TryGetTarget(out var target))
                {
                    target.Command?.Execute(null);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}