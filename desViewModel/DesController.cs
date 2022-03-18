using System;
using System.ComponentModel;
using System.Windows;

namespace desViewModel
{
    public class DesController : INotifyPropertyChanged
    {
        public string Key1 { get; set; }
        public string? Key2 { get; set; }
        public string? Key3 { get; set; }
        public string InitVector { get; set; }
        public bool Option1 { get; set; } = true;
        public bool Option2 { get; set; }
        public bool Option3 { get; set; }
        public string? EncryptFileName { get; set; } = "filename...";
        public string? DecryptFileName { get; set; } = "filename...";
        public string? EncryptText { get; set; } = "text...";
        public string? DecryptText { get; set; } = "text...";
        public string? Result { get; set; } = "Result...";
        public event PropertyChangedEventHandler? PropertyChanged;



    }
}
