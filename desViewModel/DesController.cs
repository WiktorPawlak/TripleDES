using System;
using System.ComponentModel;
using System.Diagnostics;
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

        public ICommand GenerateKeyCommand
        {
            get
            {
                return new RelayCommand<object>(
                    (object commandParam) =>
                    {
                        string generatedKey = debug.toStr(debug.genKey());
                        switch (commandParam as string)
                        {
                            case "KEY1":
                                Key1 = generatedKey;
                                OnPropertyChanged(nameof(Key1));
                                break;
                            case "KEY2":
                                Key2 = generatedKey;
                                OnPropertyChanged(nameof(Key2));
                                break;
                            case "KEY3":
                                Key3 = generatedKey;
                                OnPropertyChanged(nameof(Key3));
                                break;
                            case "INITV":
                                InitVector = generatedKey;
                                OnPropertyChanged(nameof(InitVector));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(commandParam),
                                    "Unknown command parameter - lookup unresolved!");
                        };
                        OnPropertyChanged(nameof(Key1));
                    });
            }
        }

    }
}
