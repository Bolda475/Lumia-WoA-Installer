﻿namespace Intaller.Wpf.ViewModels
{
    public class MessageViewModel
    {
        public string Title { get; }
        public string Text { get; }

        public MessageViewModel(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}