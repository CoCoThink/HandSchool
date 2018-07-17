﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HandSchool.Views
{
    /// <summary>
    /// 基于Xamarin封装的页面文件
    /// </summary>
    /// <remarks>Thanks to 山宏岳</remarks>
    public class PopContentPage : ContentPage
	{
        /// <summary>
        /// 是否已被销毁
        /// </summary>
        public bool Destoried { get; private set; }

        /// <summary>
        /// 是否为模态页面
        /// </summary>
        public bool IsModal { get; set; } = false;

        /// <summary>
        /// 正在销毁事件
        /// </summary>
        public event Action Destorying;

        /// <summary>
        /// 显示窗口，并在窗口关闭时返回
        /// </summary>
        /// <param name="navigation">系统导航</param>
        public Task ShowAsync(INavigation navigation = null)
        {
            Destoried = false;

            if(navigation is null)
            {
                Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(this));
                Disappearing += Page_Disappearing;
                IsModal = true;
            }
            else
            {
                navigation.PushAsync(this);
                if (Parent is NavigationPage)
                {
                    _navpg = Parent as NavigationPage;
                    _navpg.Popped += Page_Popped;
                }
                else
                {
                    Disappearing += Page_Disappearing;
                    System.Diagnostics.Debug.WriteLine("Not support this kind of access, may occured some errors.");
                    System.Diagnostics.Debug.WriteLine("Maybe double tapped but event is one tap.");
                }
            }

            return ContinueTask;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public async Task CloseAsync()
        {
            if (IsModal)
                await Navigation.PopModalAsync();
            else
                await Navigation.PopAsync();
        }

        private NavigationPage _navpg;

        private Task ContinueTask { get; } = new Task(() => { });

        private void Page_Disappearing(object sender, EventArgs e)
        {
            if (Destoried) return;
            Destoried = true;

            Disappearing -= Page_Disappearing;
            Destorying?.Invoke();

            ContinueTask?.Start();
        }

        private void Page_Popped(object sender, NavigationEventArgs e)
        {
            if (Destoried) return;
            Destoried = true;
            
            _navpg.Popped -= Page_Popped;
            Destorying?.Invoke();

            ContinueTask?.Start();
        }
    }
}