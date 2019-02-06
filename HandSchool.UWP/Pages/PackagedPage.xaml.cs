﻿using HandSchool.Internals;
using System.Diagnostics;
using Windows.UI.Xaml;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using WNavigationEventArgs = Windows.UI.Xaml.Navigation.NavigationEventArgs;

namespace HandSchool.Views
{
    internal sealed partial class PackagedPage : ViewPage
    {
        public PackagedPage()
        {
            InitializeComponent();
            SizeChanged += FrameworkElement_SizeChanged;
        }
        
        public ViewObject Packager { get; set; }

        public ContentPage InternalPage { get; set; }

        protected override void OnPageLoaded(RoutedEventArgs args, MainPage mainPage)
        {
            if (Packager is null)
            {
                base.OnPageLoaded(args, mainPage);
            }
            else
            {
                ViewModel = Packager.ViewModel;
                Packager.RegisterNavigation(Navigation);
                foreach (var entry in Packager.ToolbarMenu)
                    AddToolbarEntry(entry);
                base.OnPageLoaded(args, mainPage);
                Packager.Pushed.Start();
            }
        }
        
        protected override void OnNavigatedTo(WNavigationEventArgs e)
        {
            if (e.Parameter is IViewPresenter presenter)
            {
                Debug.Assert(presenter.PageCount == 1);
                Packager = (ViewObject)presenter.GetAllPages()[0];
                ProcessPackager();
            }
            else if (e.Parameter is System.ValueTuple<System.Type, object> paramed)
            {
                Packager = Core.Reflection.CreateInstance<ViewObject>(paramed.Item1);
                Packager.SetNavigationArguments(paramed.Item2);
                ProcessPackager();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Packager.SendAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Packager.SendDisappearing();

            // Do some basic cleaning.
            (Content as Windows.UI.Xaml.Controls.Grid).Children.Clear();
            Platform.GetRenderer(InternalPage).Dispose();
            Platform.SetRenderer(InternalPage, null);
            ViewModel.View = null;
            ViewModel = null;
            Packager.ViewModel = null;
            Packager = null;
            System.GC.Collect();
        }

        private void ProcessPackager()
        {
            InternalPage = new ContentPage
            {
                Content = Packager.Content,
                BindingContext = Packager.ViewModel,
            };
            
            Content = new Windows.UI.Xaml.Controls.Grid
            {
                Children = { InternalPage.CreateFrameworkElement() }
            };
        }
        
        private void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (Content is FrameworkElement element)
            {
                InternalPage?.Layout(new Rectangle(0, 0, element.ActualWidth, element.ActualHeight));
            }
        }
    }
}
