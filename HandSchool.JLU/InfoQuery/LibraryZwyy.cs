﻿using HandSchool.Internal;
using HandSchool.Internals;
using HandSchool.Models;
using HandSchool.Views;
using System.Threading.Tasks;

namespace HandSchool.JLU.InfoQuery
{
    [Entrance("JLU", "图书馆座位预约", "在图书馆订阅美好的新一天~", EntranceType.InfoEntrance)]
    class LibraryZwyy : ITapEntrace
    {
        const string LibZwyyJluEduCn = "http://libzwyy.jlu.edu.cn";
        
        public Task Action(INavigate navigate)
        {
            Core.Platform.OpenUrl(LibZwyyJluEduCn);
            return Task.CompletedTask;
        }
    }
}
