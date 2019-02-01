﻿using HandSchool.Models;
using HandSchool.ViewModels;
using System.Threading.Tasks;

namespace HandSchool.Internal
{
    public static class ViewModelExtensions
    {
        /// <summary>
        /// 对于表单请求登录。
        /// </summary>
        /// <param name="form">请求的表单。</param>
        /// <returns>登录是否成功。</returns>
        public static async Task<bool> RequestLogin(this ILoginField form)
        {
            if (form.AutoLogin && !form.IsLogin) await form.Login();
            if (!form.IsLogin) await LoginViewModel.RequestAsync(form);
            return form.IsLogin;
        }

        /// <summary>
        /// 通过弹出对话框，提示连接超时的信息。
        /// </summary>
        /// <param name="baseVm">应该显示的视图模型。</param>
        public static Task ShowTimeoutMessage(this BaseViewModel baseVm)
        {
            return baseVm.RequestMessageAsync("错误", "连接超时，请重试。", "知道了");
        }
    }
}