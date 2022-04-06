using System.Collections;
using System.ComponentModel;
using System.Windows.Input;

namespace desViewModel
{
    public class DesController : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Key1 { get; set; }
        public string? Key2 { get; set; }
        public string? Key3 { get; set; }
        public string InitVector { get; set; }
        public bool Mode1 { get; set; } = true;
        public bool Mode2 { get; set; }
        public bool Mode3 { get; set; }
        public string? EncryptFileName { get; set; } = "filename...";
        public string? DecryptFileName { get; set; } = "filename...";
        public string? FilePath { get; set; }
        public string? EncryptText { get; set; } = "text...";
        public string? DecryptText { get; set; } = "text...";
        public string? Result { get; set; } = "Result...";
        private byte[]? fileResult = null;
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
                        string generatedKey = GenerateKey();
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
                    });
            }
        }

        public ICommand CypherTextCommand
        {
            get
            {
                return new RelayCommand<string>(
                    (string codingOption) =>
                    {
                        ValidateInput(codingOption);
                        string inputString = EncryptText;
                        if (codingOption.StartsWith("DECRYPT"))
                        {
                            inputString = DecryptText;
                        }
                        RunSelectedMode(mode: codingOption, inputText: inputString);
                    }
                    );
            }
        }

        public ICommand CypherFileCommand
        {
            get
            {
                return new RelayCommand<string>(
                    (string codingOption) =>
                    {
                        ValidateInput(codingOption);

                        GetPath();

                        byte[] inputStream = FileToByteArray();
                        RunSelectedMode(mode: codingOption, bytes: inputStream);
                        ByteArrayToFile(fileResult, codingOption);
                    }
                    );
            }
        }

        private static string GenerateKey() => debug.toStr(debug.genKey());
        private static BitArray ToBitArray(string text) => debug.toBitArray(text);
        private static BitArray[] ToBlocks(byte[] bytes) => conv.toBlocks(bytes);

        private string GetSelectedMode()
        {
            if (Mode1 == true)
            {
                return "Mode1";
            }
            else if (Mode2 == true)
            {
                return "Mode2";
            }
            else
            {
                return "Mode3";
            }
        }
        private void RunSelectedMode(string mode, byte[]? bytes = null, string? inputText = null)
        {
            var initVector = ToBitArray(InitVector);
            var keys = GetSelectedMode() switch
            {
                "Mode1" => Tuple.Create
                (
                    ToBitArray(Key1),
                    ToBitArray(Key2),
                    ToBitArray(Key3)
                ),
                "Mode2" => Tuple.Create
                (
                    ToBitArray(Key1),
                    ToBitArray(Key2),
                    ToBitArray(Key1)
                ),
                "Mode3" => Tuple.Create
                (
                    ToBitArray(Key1),
                    ToBitArray(Key1),
                    ToBitArray(Key1)
                ),
                _ => throw new ArgumentOutOfRangeException(GetSelectedMode(),
                  "Unknown mode!"),
            };
            if (bytes == null)
            {
                if (mode.StartsWith("ENCRYPT"))
                {
                    Result = tdea.encryptString(initVector, keys.Item1, keys.Item2, keys.Item3, inputText);
                }
                else
                {
                    Result = tdea.decryptString(initVector, keys.Item1, keys.Item2, keys.Item3, inputText);
                }
                OnPropertyChanged(nameof(Result));
            }
            else
            {
                var blocks = ToBlocks(bytes);
                if (mode.StartsWith("ENCRYPT"))
                {
                    fileResult = tdea.encryptBytes(initVector, keys.Item1, keys.Item2, keys.Item3, bytes);
                }
                else
                {
                    fileResult = tdea.decryptBytes(initVector, keys.Item1, keys.Item2, keys.Item3, bytes);
                }
                OnPropertyChanged(nameof(Result));
            }
        }
        private byte[] FileToByteArray()
        {
            return File.ReadAllBytes(FilePath);
        }
        private void ByteArrayToFile(byte[] array, string codingOption)
        {
            string path = FilePath.Substring(0, FilePath.LastIndexOf('\\')) + "\\";
            if (codingOption.StartsWith("DECRYPT"))
            {
                path += DecryptFileName;
            }
            else
            {
                path += EncryptFileName;
            }
            File.WriteAllBytes(path, array);
        }
        private bool ValidateInput(object textBox)
        {
            if (InitVector == null)
            {
                throw new ArgumentNullException(nameof(InitVector), "InitVector was null!");
            }
            switch (GetSelectedMode())
            {
                case "Mode1":
                    if (Key1 == null | Key2 == null | Key3 == null)
                    {
                        throw new ArgumentNullException("One of the keys was null for KEYING MODE 1!");
                    }
                    break;
                case "Mode2":
                    if (Key1 == null | Key2 == null)
                    {
                        throw new ArgumentNullException("One of the keys was null for KEYING MODE 2!");
                    }
                    break;
                case "Mode3":
                    if (Key1 == null)
                    {
                        throw new ArgumentNullException("First key was null for KEYING MODE 3!");
                    }
                    break;
            }

            switch (textBox as string)
            {
                case "ENCRYPT FILE":
                    if (string.IsNullOrEmpty(EncryptFileName))
                    {
                        throw new ArgumentNullException("File name for encryption was null!");
                    }
                    //if (!File.Exists(FilePath))
                    //{
                    //    throw new FileNotFoundException("File at specified path does not exits - " + FilePath);
                    //}
                    break;
                case "DECRYPT FILE":
                    if (string.IsNullOrEmpty(DecryptFileName))
                    {
                        throw new ArgumentNullException("File name for decryption was null!");
                    }
                    //if (!File.Exists(FilePath))
                    //{
                    //    throw new FileNotFoundException("File at specified path does not exits - " + FilePath);
                    //}
                    break;
                case "ENCRYPT TEXT":
                    if (string.IsNullOrEmpty(EncryptText))
                    {
                        throw new ArgumentNullException("Text for encryption was null!");
                    }
                    break;

                case "DECRYPT TEXT":
                    if (string.IsNullOrEmpty(DecryptText))
                    {
                        throw new ArgumentNullException("Text for decryption was null!");
                    }
                    break;
            }
            return true;
        }
        public void GetPath()
        {
            var fileDialog = new OpenFileDialog()
            {
                Title = "Podaj ścieżkę pliku do enkrypcji/dekrypcji",
                CheckFileExists = false,
                CheckPathExists = false,
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = fileDialog.FileName;
            }
        }
    }
}
