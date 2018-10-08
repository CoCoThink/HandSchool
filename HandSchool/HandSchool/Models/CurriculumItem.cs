﻿using HandSchool.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HandSchool.Models
{
    /// <summary>
    /// 单双周
    /// </summary>
    public enum WeekOddEvenNone { Even, Odd, None }

    /// <summary>
    /// 课程描述
    /// </summary>
    public class CurriculumDescription
    {
        internal CurriculumDescription(string tit, string desc)
        {
            Title = tit;
            Description = desc;
        }

        public readonly string Title;
        public readonly string Description;
    }

    /// <summary>
    /// 课程表项目
    /// </summary>
    public abstract class CurriculumItemBase : NotifyPropertyChanged
    {
        private int _dayBegin;
        private int _dayEnd;
        private int _weekDay;

        /// <summary>
        /// 星期几
        /// </summary>
        public int WeekDay
        {
            get => _weekDay;
            set => SetProperty(ref _weekDay, value);
        }

        /// <summary>
        /// 开始节
        /// </summary>
        public int DayBegin
        {
            get => _dayBegin;
            set => SetProperty(ref _dayBegin, value);
        }

        /// <summary>
        /// 结束节
        /// </summary>
        public int DayEnd
        {
            get => _dayEnd;
            set => SetProperty(ref _dayEnd, value);
        }

        public abstract CurriculumDescription[] ToDescription();
    }

    /// <summary>
    /// 单节课程表项目
    /// </summary>
    public class CurriculumItem : CurriculumItemBase
    {
        private string _name;
        private string _teacher;
        private string _courseID;
        private string _classroom;
        private int _weekBegin;
        private int _weekEnd;
        private WeekOddEvenNone _weekOen;
        private DateTime _selectDate;
        private bool _isCustom;

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 任课教师
        /// </summary>
        public string Teacher
        {
            get => _teacher;
            set => SetProperty(ref _teacher, value);
        }

        /// <summary>
        /// 课程编号
        /// </summary>
        public string CourseID
        {
            get => _courseID;
            set => SetProperty(ref _courseID, value);
        }

        /// <summary>
        /// 上课教室
        /// </summary>
        public string Classroom
        {
            get => _classroom;
            set => SetProperty(ref _classroom, value);
        }

        /// <summary>
        /// 开始周
        /// </summary>
        public int WeekBegin
        {
            get => _weekBegin;
            set => SetProperty(ref _weekBegin, value);
        }

        /// <summary>
        /// 结束周
        /// </summary>
        public int WeekEnd
        {
            get => _weekEnd;
            set => SetProperty(ref _weekEnd, value);
        }

        /// <summary>
        /// 单双周信息
        /// </summary>
        public WeekOddEvenNone WeekOen
        {
            get => _weekOen;
            set => SetProperty(ref _weekOen, value);
        }
        
        /// <summary>
        /// 选课日期
        /// </summary>
        public DateTime SelectDate
        {
            get => _selectDate;
            set => SetProperty(ref _selectDate, value);
        }

        /// <summary>
        /// 是否为自定义课程
        /// </summary>
        public bool IsCustom
        {
            get => _isCustom;
            set => SetProperty(ref _isCustom, value);
        }

        /// <summary>
        /// 课程表项目
        /// </summary>
        public CurriculumItem()
        {
            _name = _teacher = _courseID = _classroom = string.Empty;
            _weekBegin = _weekEnd = 0;
            _weekOen = WeekOddEvenNone.None;
            _selectDate = DateTime.Now;
            _isCustom = false;
        }

        /// <summary>
        /// 在某一周是否显示
        /// </summary>
        /// <param name="week">第x周</param>
        /// <returns>是否显示</returns>
        public bool IfShow(int week)
        {
            bool show = ((int)_weekOen == 2) || ((int)_weekOen == week % 2);
            show &= (week >= _weekBegin) && (week <= _weekEnd);
            return show;
        }

        public override CurriculumDescription[] ToDescription()
        {
            return new CurriculumDescription[1]
            {
                new CurriculumDescription(Name, Classroom)
            };
        }
    }

    public class CurriculumItemSet2 : CurriculumItemBase
    {
        public int DayBegin = 0;
        public int DayEnd = 0;

        public static bool CompareClass(CurriculumItem A, CurriculumItem B)
        {
            if (A.Name != B.Name ||
                        A.DayBegin != B.DayBegin ||
                        A.DayEnd != B.DayEnd ||
                        A.Teacher != B.Teacher ||
                        A.Classroom != B.Classroom ||
                        A.WeekBegin != B.WeekBegin
                        )
                return false;
            else
                return true;
        }

        public static bool operator ==(CurriculumItemSet2 A, CurriculumItemSet2 B)
        {
            if (B is null || B.CurriculumItemList.Count != A.CurriculumItemList.Count)
            {
                return false;
            }
            else
            {
                var Temp = B.CurriculumItemList;
                for (int i = 0; i < A.CurriculumItemList.Count; i++)
                    if (!CompareClass(Temp[i], B.CurriculumItemList[i]))
                        return false;
                return true;
            }
        }

        public static bool operator !=(CurriculumItemSet2 A, CurriculumItemSet2 B)
        {
            return !(A == B);
        }

        public override bool Equals(object obj)
        {
            return this == obj as CurriculumItemSet2;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void MergeClasses()
        {
            for (int i = 0; i < CurriculumItemList.Count; i++)
            {
                for (int j = i + 1; j < CurriculumItemList.Count; j++)
                {
                    if (CompareClass(CurriculumItemList[i], CurriculumItemList[j]))
                        CurriculumItemList.RemoveAt(j);
                }
            }
        }

        public List<CurriculumItem> CurriculumItemList = new List<CurriculumItem>();

        public override CurriculumDescription[] ToDescription()
        {
            throw new NotImplementedException();
        }
    }
}
