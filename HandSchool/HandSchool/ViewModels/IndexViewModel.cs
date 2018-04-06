﻿using HandSchool.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HandSchool.ViewModels
{
    public class IndexViewModel : BaseViewModel
    {
        private static IndexViewModel instance = null;

        public static IndexViewModel Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new IndexViewModel();
                    instance.RefreshCommand = new Command(instance.Refresh);
                }
                return instance;
            }
        }

        #region Weather

        string weather;
        string weatherRange;
        string weatherTips;
        string weatherJson;
        
        public string Weather
        {
            get => weather;
            set => SetProperty(ref weather, value);
        }

        public string WeatherRange
        {
            get => weatherRange;
            set => SetProperty(ref weatherRange, value);
        }

        public string WeatherTips
        {
            get => weatherTips;
            set => SetProperty(ref weatherTips, value);
        }
        
        public async Task UpdateWeather()
        {
            try
            {
                var wc = new AwaredWebClient("https://www.sojson.com/open/api/weather/", Encoding.UTF8);
                weatherJson = await wc.GetAsync("json.shtml?city=长春");
                JObject jo = (JObject)JsonConvert.DeserializeObject(weatherJson);
                string high = jo["data"]["forecast"][0]["high"].ToString();
                string low = jo["data"]["forecast"][0]["low"].ToString();
                Weather = jo["data"]["forecast"][0]["type"].ToString();
                WeatherTips = jo["data"]["forecast"][0]["notice"].ToString();
                WeatherRange = $"{high} {low}";
            }
            catch (Exception ex)
            {
                Weather = "天气信息获取失败";
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        #endregion

        #region Next Curriculum

        CurriculumItem curriculum;
        public string NextClass => curriculum?.Name == default(string) ? curriculum?.Name : "无（无课程或未刷新）";
        public string NextTeacher => curriculum?.Teacher;
        public string NextClassroom => curriculum?.Classroom;

        void UpdateNextCurriculum()
        {
            int today = (int)DateTime.Now.DayOfWeek;
            if (today == 0) today = 7;
            int toweek = App.Current.Service.CurrentWeek;
            int tocor = App.Current.Schedule.ClassNext;
            curriculum = App.Current.Schedule.Items.Find((obj) => obj.IfShow(toweek) && obj.WeekDay == today && obj.DayBegin > tocor);
            OnPropertyChanged("NextClass");
            OnPropertyChanged("NextTeacher");
            OnPropertyChanged("NextClassroom");
        }

        #endregion

        #region Refresh Command

        public Command RefreshCommand { get; set; }

        async void Refresh()
        {
            UpdateNextCurriculum();
            await UpdateWeather();
        }

        #endregion

    }
}