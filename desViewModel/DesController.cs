using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand GenerateKey
        {
            get
            {
                return new RelayCommand<object>(
                    (object o) =>
                    {
                        string generatedValue = (o as string) switch
                        {
                            "KEY1" => RunDiagnostics(),
                            "KEY2" => StartSystem(),
                            "KEY3" => StopSystem(),
                            "INITV" => ResetToReady(),
                            _ => throw new ArgumentException("Unknown comman parameter", nameof(o)),
                        };
                        OnPropertyChanged(nameof(Result));
                    });
            }
        }

    }
}
