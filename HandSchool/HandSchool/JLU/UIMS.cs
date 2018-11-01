﻿using HandSchool.Internal;
using HandSchool.JLU.JsonObject;
using HandSchool.Models;
using HandSchool.Services;
using HandSchool.ViewModels;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using JsonException = Newtonsoft.Json.JsonReaderException;

namespace HandSchool.JLU
{
    class UIMS : NotifyPropertyChanged, ISchoolSystem
    {
        internal const string config_file = "jlu.config.json";
        internal const string config_username = "jlu.uims.username.txt";
        internal const string config_password = "jlu.uims.password.txt";
        internal const string config_usercache = "jlu.user.json";
        internal const string config_teachterm = "jlu.teachingterm.json";

        #region Login Information

        public string Username { get; set; }
        public string Password { get; set; }
        public bool NeedLogin { get; private set; }
        
        [Settings("提示", "保存使设置永久生效，部分设置重启后生效。", -233)]
        public string Tips => "用户名为教学号，新生默认密码为身份证后六位（x小写）。";
        
        public string FormName => "UIMS教务管理系统";

        private string proxy_server;
        [Settings("服务器", "通过此域名访问UIMS，但具体路径地址不变。\n如果在JLU.NET等公用WiFi下访问，建议改为 10.60.65.8。")]
        public string ProxyServer
        {
            get => proxy_server;
            set
            {
                var new_val = value.Trim();
                if (new_val.Length == 0) new_val = "uims.jlu.edu.cn";
                SetProperty(ref proxy_server, new_val);
            }
        }

        private bool use_https;
        [Settings("使用SSL连接", "通过HTTPS连接UIMS，不验证证书。连接成功率更高。")]
        public bool UseHttps
        {
            get => use_https;
            set => SetProperty(ref use_https, value);
        }

        public event EventHandler<LoginStateEventArgs> LoginStateChanged;

        private bool is_login = false;
        public bool IsLogin
        {
            get => is_login;
            private set
            {
                SetProperty(ref is_login, value);
                OnPropertyChanged(nameof(WelcomeMessage));
                OnPropertyChanged(nameof(CurrentMessage));
                OnPropertyChanged(nameof(CurrentWeek));
            }
        }

        private bool auto_login = true;
        public bool AutoLogin
        {
            get => auto_login;
            set
            {
                SetProperty(ref auto_login, value);
                if (value) SetProperty(ref save_password, true, nameof(SavePassword));
            }
        }

        private bool save_password = true;
        public bool SavePassword
        {
            get => save_password;
            set
            {
                SetProperty(ref save_password, value);
                if (!value) SetProperty(ref auto_login, true, nameof(AutoLogin));
            }
        }

        public string WelcomeMessage => NeedLogin ? "请登录" : $"欢迎，{AttachInfomation["studName"]}。";
        public string CurrentMessage => NeedLogin ? "登录后可以查看更多内容" : $"{AttachInfomation["Nick"]}第{CurrentWeek}周";

        #endregion
        
        public AwaredWebClient WebClient { get; set; }
        public NameValueCollection AttachInfomation { get; set; }
        public string ServerUri => $"http{(use_https ? "s" : "")}://{proxy_server}/ntms/";
        public string WeatherLocation => "长春";
        public LoginValue LoginInfo { get; set; }
        public int CurrentWeek { get; set; }
        public string CaptchaCode { get; set; } = "";
        public byte[] CaptchaSource { get; set; } = null;

        /// <summary>
        /// 建立访问UIMS的对象。
        /// </summary>
        /// <param name="injectedHandler">事件处理传递方法。</param>
        [ToFix("存在性能问题，瓶颈在JSON的解析上")]
        public UIMS(EventHandler<LoginStateEventArgs> injectedHandler = null)
        {
            if (injectedHandler != null)
            {
                LoginStateChanged += injectedHandler;
            }

            var lp = Core.ReadConfig(config_file);
            SettingsJSON config;
            if (lp != "") config = lp.ParseJSON<SettingsJSON>();
            else config = new SettingsJSON();
            ProxyServer = config.ProxyServer;
            UseHttps = config.UseHttps;
            
            IsLogin = false;
            NeedLogin = false;
            Username = Core.ReadConfig(config_username);
            AttachInfomation = new NameValueCollection();
            if (Username != "") Password = Core.ReadConfig(config_password);
            if (Password == "") SavePassword = false;
            
            try
            {
                ParseLoginInfo(Core.ReadConfig(config_usercache));
                ParseTermInfo(Core.ReadConfig(config_teachterm));
            }
            catch (JsonException)
            {
                AutoLogin = false;
                NeedLogin = true;
            }
        }

        private void ParseLoginInfo(string resp)
        {
            LoginInfo = resp.ParseJSON<LoginValue>();
            AttachInfomation.Add("studId", LoginInfo.userId.ToString());
            AttachInfomation.Add("studName", LoginInfo.nickName);
            AttachInfomation.Add("term", LoginInfo.defRes.teachingTerm.ToString());
        }

        private void ParseTermInfo(string resp)
        {
            var ro = resp.ParseJSON<RootObject<TeachingTerm>>().value[0];
            if (ro.vacationDate < DateTime.Now)
            {
                AttachInfomation.Add("Nick", ro.year + "学年" + (ro.termSeq == "1" ? "寒假" : "暑假"));
                CurrentWeek = (int) Math.Ceiling((decimal) ((DateTime.Now - ro.vacationDate).Days + 1) / 7);
            }
            else
            {
                AttachInfomation.Add("Nick", ro.year + "学年" + (ro.termSeq == "1" ? "秋季学期" : (ro.termSeq == "2" ? "春季学期" : "短学期")));
                CurrentWeek = (int)Math.Ceiling((decimal) ((DateTime.Now - ro.startDate).Days + 1) / 7);
            }
        }

        public async Task<bool> Login()
        {
            if (Username == "" || Password == "")
            {
                NeedLogin = true;
                return false;
            }
            else
            {
                Core.WriteConfig(config_username, Username);
                Core.WriteConfig(config_password, SavePassword ? Password : "");
            }

            if (WebClient != null) WebClient.Dispose();
            WebClient = new AwaredWebClient(ServerUri, Encoding.UTF8);
            var proxy_server_domain = proxy_server.Split(':')[0];
            WebClient.Cookie.Add(new Cookie("loginPage", "userLogin.jsp", "/ntms/", proxy_server_domain));
            WebClient.Cookie.Add(new Cookie("alu", Username, "/ntms/", proxy_server_domain));
            WebClient.Cookie.Add(new Cookie("pwdStrength", "1", "/ntms/", proxy_server_domain));

            // Access Main Page To Create a JSESSIONID
            try
            {
                await WebClient.GetAsync("", "*/*");

                // Set Login Session
                var loginData = new NameValueCollection
                {
                    { "j_username", Username },
                    { "j_password", $"UIMS{Username}{Password}".ToMD5(Encoding.UTF8) },
                    { "mousePath", "NCgABNAQBgNAwBjNBQBkNBgBqNBwBtNBwB1NDAB6OEACCPFQCHRHACKTIQCUTJQCXWLwCbXNACeYOgClaOgCmcPwCpcQQCqcQwCxcQwC0cRQC2cRgC4cRwC7dRwDPdSAGMdSQGNdTAGQdTAGRdTgGUdTwGZdUAGfdVQGidWQGkdWgGpdYgGvdYwGwdZwGzdZwG0daAG0daQG3dawG4dbAG6dbwG7dbwG8dcQG8dcgG9ddAHAddQHBddgHCdeAHDdeAHKdfgHLfgQHNfgwHOfhAHPghgHRghwHRghwHTgigHUgjAHYgjQHYgjwHZgjwHagkAHagkwHcgkwHdhlgHfhlwHihmAHihmgHihnQHlhngHnjoAHpjogHyjqQHzjqwH0jrAH0jrgH3lrwH5lsgH6ltAH7ltwH8ltwH+luQIBluwICluwIDlvQIIlvwIKlwAILlwgINlxAIPlxAIQlxgISlxwIXlyAIlkyQJ6kygJ+kzQKJkzQKMkzwKPk0QKVj0QKaj1gKdj2gKoj2wKrj4QKuj5wKzIqgFL" }
                };

                WebClient.Headers.Set("Referer", ServerUri + "userLogin.jsp?reason=nologin");
                await WebClient.PostAsync("j_spring_security_check", loginData);

                if (WebClient.Location == "error/dispatch.jsp?reason=loginError")
                {
                    string result = await WebClient.GetAsync("userLogin.jsp?reason=loginError", "text/html");
                    LoginStateChanged?.Invoke(this, new LoginStateEventArgs(LoginState.Failed, Regex.Match(result, @"<span class=""error_message"" id=""error_message"">登录错误：(\S+)</span>").Groups[1].Value));
                    IsLogin = false;
                    return false;
                }
                else if (WebClient.Location == "index.do")
                {
                    AttachInfomation.Clear();

                    // Get User Info
                    string resp = await WebClient.PostAsync("action/getCurrentUserInfo.do", "{}");
                    if (resp.StartsWith("<!")) return false;
                    Core.WriteConfig(config_usercache, AutoLogin ? resp : "");
                    ParseLoginInfo(resp);
                    
                    // Get term info
                    resp = await WebClient.PostAsync("service/res.do", "{\"tag\":\"search@teachingTerm\",\"branch\":\"byId\",\"params\":{\"termId\":" + AttachInfomation["term"] + "}}");
                    if (resp.StartsWith("<!")) return false;
                    Core.WriteConfig(config_teachterm, AutoLogin ? resp : "");
                    ParseTermInfo(resp);
                }
                else
                {
                    throw new ContentAcceptException(WebClient.Location, null, null);
                }
            }
            catch (WebException ex)
            {
                LoginStateChanged?.Invoke(this, new LoginStateEventArgs(ex));
                return false;
            }
            catch (ContentAcceptException ex)
            {
                var error = "UIMS服务器似乎出了点问题……";
                if (ex.Current != "")
                    error += "服务器未知响应：" + ex.Data + "，请联系开发者。";
                LoginStateChanged?.Invoke(this, new LoginStateEventArgs(LoginState.Failed, error));
                return false;
            }

            IsLogin = true;
            NeedLogin = false;
            LoginStateChanged?.Invoke(this, new LoginStateEventArgs(LoginState.Succeeded));
            return true;
        }
        
        public string FormatArguments(string args)
        {
            return args
                .Replace("`term`", AttachInfomation["term"])
                .Replace("`studId`", AttachInfomation["studId"]);
        }

        public async Task<bool> RequestLogin()
        {
            if (AutoLogin && !IsLogin) await Login();
            if (!IsLogin) await LoginViewModel.RequestAsync(this);
            return IsLogin;
        }

        public async Task<string> Post(string url, string send)
        {
            if (await RequestLogin() == false)
            {
                throw new WebException("登录超时。", WebExceptionStatus.Timeout);
            }

            try
            {
                var ret = await WebClient.PostAsync(url, FormatArguments(send));

                if (WebClient.Location == "error/dispatch.jsp?reason=nologin")
                {
                    throw new WebException("登录超时。", WebExceptionStatus.Timeout);
                }

                return ret;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                    is_login = false;
                throw ex;
            }
        }

        public async Task<string> Get(string url)
        {
            if (await RequestLogin() == false)
            {
                throw new WebException("登录超时。", WebExceptionStatus.Timeout);
            }

            try
            {
                var ret = await WebClient.GetAsync(url);

                if (WebClient.Location == "error/dispatch.jsp?reason=nologin")
                {
                    throw new WebException("登录超时。", WebExceptionStatus.Timeout);
                }

                return ret;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                    is_login = false;
                throw ex;
            }
        }

        public void SaveSettings()
        {
            var save = new SettingsJSON { ProxyServer = proxy_server, UseHttps = use_https }.Serialize();
            Core.WriteConfig(config_file, save);
        }

        [Settings("清除数据", "将应用数据清空，恢复到默认状态。")]
        public async void ResetSettings(IViewResponse resp)
        {
            if (!await resp.ShowAskMessage("清除数据", "确定要清除数据吗？", "取消", "确认")) return;
            Core.WriteConfig(config_username, "");
            Core.WriteConfig(config_password, "");
            Core.WriteConfig(config_usercache, "");
            Core.WriteConfig(config_username, "");
            Core.WriteConfig(config_file, "");
            Core.WriteConfig(OA.config_oa, "");
            Core.WriteConfig(OA.config_oa_time, "");
            Core.WriteConfig(Schedule.config_kcb, "");
            Core.WriteConfig(Schedule.config_kcb_orig, "");
            Core.WriteConfig(SchoolCard.config_username, "");
            Core.WriteConfig(SchoolCard.config_password, "");
            Core.WriteConfig(SchoolCard.config_school, "");
            Core.WriteConfig(MessageEntrance.config_msgbox, "");
            Core.WriteConfig(GradeEntrance.config_gpa, "");
            Core.WriteConfig(GradeEntrance.config_grade, "");
            Core.WriteConfig("hs.school.bin", "");
            await resp.ShowMessage("清除数据", "重置应用成功！重启应用后生效。");
        }

        public async Task<bool> PrepareLogin()
        {
            await Task.Run(() => { });
            return true;
        }

        class SettingsJSON
        {
            public string ProxyServer { get; set; } = "uims.jlu.edu.cn";
            public bool UseHttps { get; set; } = true;
        }
    }
}
