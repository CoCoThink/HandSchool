﻿using HandSchool.Internals;
using HandSchool.Models;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HandSchool.Views
{
    public class CurriculumLabel : StackLayout
    {
        public CurriculumItemBase Context { get; }
        public int ColorId { get; private set; }

#if false
        private void Layout_SizeChanged(object sender, EventArgs e)
        {

            foreach (Span Item in (Children[0] as Label).FormattedText.Spans)
                Item.FontSize = (Children[0] as Label).FontSize;
#if __IOS__
                des.FontSize *= 0.8;
#endif
        //每次都重置一次初始大小 因为此方法在横竖屏切换时可能被多次调用
            var Height = this.Height-10;
            var Width = this.Width-10;
            double NowFontSize = (Children[0] as Label).FontSize;

            while (GetTotalHeight(Width, NowFontSize) >Height&&NowFontSize>1)
                NowFontSize--;

            foreach (Span Item in (Children[0] as Label).FormattedText.Spans)
            {
                Item.FontSize =NowFontSize;
#if __IOS__
            des.FontSize =NowFontSize;
#endif
            }
        }

        private double GetTotalHeight(double Width,double NowFontSize)
        {
            double TotalHeight = 0;
            foreach(var i in Children)
            {
                TotalHeight += GetLabelHeight(i as Label, Width, NowFontSize);
            }
            TotalHeight += (Children.Count - 1 )* NowFontSize;
            return TotalHeight;
        }

        private double GetLabelHeight(Label label,double Width,double NowFontSize)
        {
            
            double TotalHeight = 0;
            double Padding = NowFontSize * 0.42;
            double TextHeight = NowFontSize + Padding;
            string Text = label.FormattedText.ToString();
            string[] SplitedText = Text.Split('\n');
            foreach (string Item in SplitedText)
            {
                TotalHeight += ((int)(1 + Item.Length * (TextHeight-Padding) / Width)) * TextHeight;
            }
            return TotalHeight;
        }
#endif

        private FormattedString GetDisplay()
        {
            var formattedString = new FormattedString();
            var desc = Context.ToDescription();

            // this.SizeChanged += Layout_SizeChanged;

            foreach (var item in desc)
            {
                if (formattedString.Spans.Count > 0)
                {
                    formattedString.Spans.Add(new Span { Text = "\n\n" });
                }

                var tit = new Span
                {
                    FontAttributes = FontAttributes.Bold,
                    ForegroundColor = Color.White,
                    Text = item.Title,
                };
                
                var des = new Span
                {
                    ForegroundColor = Color.FromRgba(255, 255, 255, 220),
                    Text = item.Description,
                };
                
                if (Core.Platform.RuntimeName == "iOS")
                    des.FontSize *= 0.8;
                formattedString.Spans.Add(tit);
                formattedString.Spans.Add(new Span { Text = "\n" });
                formattedString.Spans.Add(des);
            }

            return formattedString;
        }

        public CurriculumLabel(CurriculumItemBase value, int id)
        {
            BindingContext = Context = value;
            ColorId = id;
            Padding = new Thickness(10);
            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;
            WidthRequest = 200;
            
            Children.Add(new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FormattedText = GetDisplay(),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            });

            Grid.SetColumn(this, Context.WeekDay);
            Grid.SetRow(this, Context.DayBegin);
            Grid.SetRowSpan(this, Context.DayEnd - Context.DayBegin + 1);
            BackgroundColor = GetColor();

            if (Context is CurriculumItem)
            {
                GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new CommandAction(EditCurriculum),
                    NumberOfTapsRequired = 2,
                });
            }
        }
        
        private async Task EditCurriculum()
        {
            var page = Core.New<ICurriculumPage>();
            page.SetNavigationArguments(Context as CurriculumItem, false);

            if (await page.ShowAsync())
                ViewModels.ScheduleViewModel.Instance.SendRefreshComplete();
        }

        public Color GetColor()
        {
            // thanks to brady
            return Color.FromHex(ScheduleColors[ColorId % 8]);
        }

        static readonly string[] ScheduleColors =
        {
            "#59e09e", "#f48fb1", "#ce93d8", "#ff8a65",
            "#9fa8da", "#42a5f5", "#80deea", "#c6de7c"
        };
    }
}