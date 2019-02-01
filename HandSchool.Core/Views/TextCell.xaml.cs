﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HandSchool.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TextCell : ViewCell
	{
		public TextCell()
		{
			InitializeComponent();
            Bind(TitleLabel, Label.TextProperty, nameof(Title));
            Bind(DescriptionLabel, Label.TextProperty, nameof(Detail));
            Bind(RightUpLabel, Label.TextProperty, nameof(RightUp));
            Bind(RightDownLabel, Label.TextProperty, nameof(RightDown));
            Bind(RightDownLabel, Label.TextColorProperty, nameof(RightDownColor));
            Bind(RightDownLabel, VisualElement.IsVisibleProperty, nameof(RightDownShow));
        }

        void Bind(BindableObject obj, BindableProperty prop, string path)
        {
            obj.SetBinding(prop, new Binding
            {
                Path = path,
                Source = this,
                Mode = BindingMode.OneWay
            });
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                propertyName: nameof(Title),
                returnType: typeof(string),
                declaringType: typeof(TextCell),
                defaultValue: string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty DetailProperty =
            BindableProperty.Create(
                propertyName: nameof(Detail),
                returnType: typeof(string),
                declaringType: typeof(TextCell),
                defaultValue: string.Empty);

        public string Detail
        {
            get => (string)GetValue(DetailProperty);
            set => SetValue(DetailProperty, value);
        }

        public static readonly BindableProperty RightUpProperty =
            BindableProperty.Create(
                propertyName: nameof(RightUp),
                returnType: typeof(string),
                declaringType: typeof(TextCell),
                defaultValue: string.Empty);

        public string RightUp
        {
            get => (string)GetValue(RightUpProperty);
            set => SetValue(RightUpProperty, value);
        }

        public static readonly BindableProperty RightDownProperty =
            BindableProperty.Create(
                propertyName: nameof(RightDown),
                returnType: typeof(string),
                declaringType: typeof(TextCell),
                defaultValue: string.Empty);

        public string RightDown
        {
            get => (string)GetValue(RightDownProperty);
            set => SetValue(RightDownProperty, value);
        }

        public static readonly BindableProperty RightDownColorProperty =
            BindableProperty.Create(
                propertyName: nameof(RightDownColor),
                returnType: typeof(Color),
                declaringType: typeof(TextCell),
                defaultValue: default(Color));

        public Color RightDownColor
        {
            get => (Color)GetValue(RightDownColorProperty);
            set => SetValue(RightDownColorProperty, value);
        }

        public static readonly BindableProperty RightDownShowProperty =
            BindableProperty.Create(
                propertyName: nameof(RightDownShow),
                returnType: typeof(bool),
                declaringType: typeof(TextCell),
                defaultValue: true);

        public bool RightDownShow
        {
            get => (bool)GetValue(RightDownShowProperty);
            set => SetValue(RightDownShowProperty, value);
        }

        public static readonly BindableProperty PreferedCardViewProperty =
            BindableProperty.Create(
                propertyName: nameof(PreferedCardView),
                returnType: typeof(bool),
                declaringType: typeof(TextCell),
                defaultValue: true);

        public bool PreferedCardView
        {
            get => (bool)GetValue(PreferedCardViewProperty);
            set => SetValue(PreferedCardViewProperty, value);
        }
    }
}