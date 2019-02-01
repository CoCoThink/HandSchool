﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using HandSchool.Internal;
using HandSchool.ViewModels;
using HandSchool.Views;
using Xamarin.Forms;

namespace HandSchool
{
    public partial class App : Application
    {
        public App()
        {
            Core.Reflection.ForceLoad(false);
            Core.Initialize();
            InitializeComponent();
            Core.Initialize();
            MainPage = new MainPage();
        }
        
        protected override void OnStart()
		{
            // Handle when your app starts
        }
        
		protected override void OnSleep()
		{
            // Handle when your app sleeps
        }

		protected override void OnResume()
		{
            // Handle when your app resumes
        }
    }
}