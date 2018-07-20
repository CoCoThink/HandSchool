﻿using HandSchool.JLU.InfoQuery;
using HandSchool.Models;
using HandSchool.Services;
using System.Threading.Tasks;

namespace HandSchool.JLU
{
    class Loader : ISchoolWrapper
    {
        public string SchoolName => "吉林大学";
        public string SchoolId => "jlu";

        public void PostLoad()
        {
            Task.Run(() => JsonObject.AlreadyKnownThings.Initialize());
        }

        public void PreLoad()
        {
            Core.App.Service = new UIMS();
            Core.App.DailyClassCount = 11;
            Core.App.GradePoint = new GradeEntrance();
            Core.App.Schedule = new Schedule();
            Core.App.Message = new MessageEntrance();
            Core.App.Feed = new OA();
            var group1 = new InfoEntranceGroup { GroupTitle = "公共信息查询" };
            group1.Add(new InfoEntranceWrapper("学院介绍查询", "查询学院介绍", typeof(CollegeIntroduce)));
            group1.Add(new InfoEntranceWrapper("一键教务评价", "一键教务评价，省去麻烦事", typeof(TeachEvaluate)));
            group1.Add(new InfoEntranceWrapper("查询空教室", "没地方自习?查个教室吧!", typeof(EmptyRoom)));
            group1.Add(new InfoEntranceWrapper("专业培养计划", "看看我们要学什么，提前预习偷偷补课。", typeof(ProgramMaster)));
            Core.App.InfoEntrances.Add(group1);
        }

        public override string ToString()
        {
            return SchoolName;
        }
    }
}
