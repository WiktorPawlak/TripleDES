using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace desViewModel
{
    public class DesController : INotifyPropertyChanged
    {
        public string Key1 { get; set; }
        public string? Key2 { get; set; }
        public string? Key3 { get; set; }
        public string InitVector { get; set; }
        public bool Mode1 { get; set; } = true;
        public bool Mode2 { get; set; }
        public bool Mode3 { get; set; }
        public string? EncryptFileName { get; set; } = "filename...";
        public string? DecryptFileName { get; set; } = "filename...";
        public string? EncryptText { get; set; } = "text...";
        public string? DecryptText { get; set; } = "text...";
        public string? Result { get; set; } = "Result...";
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly Predicate<object> CanExecute = ValidateInput;

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

        public ICommand EncryptTextCommand
        {
            get
            {
                return new RelayCommand<object>(
                    (object commandParam) =>
                    {
                        byte[] inputText = Encoding.ASCII.GetBytes(EncryptText);
                        Result = ToStr(RunSelectedMode(inputText));
                        OnPropertyChanged(nameof(Result));
                    },
                    CanExecute
                    );
            }
        }

        public ICommand EncryptFileCommand
        {
            get
            {
                return new RelayCommand<object>(
                    (object commandParam) =>
                    {
                        byte[] inputStream = FileToByteArray(EncryptFileName);
                        BitArray[] convertedFile = RunSelectedMode(inputStream);
                        ByteArrayToFile(ToBytes(convertedFile), EncryptFileName);
                    },
                    CanExecute
                    );
            }
        }

        private static string GenerateKey() => debug.toStr(debug.genKey());
        private static BitArray ToBitArray(string text) => debug.toBitArray(text);
        private static byte[] ToBytes(BitArray[] bits) => conv.toBytes(bits);
        private static BitArray[] ToBlocks(byte[] bytes) => conv.toBlocks(bytes);
        private static string ToStr(BitArray[] bits) => Encoding.Default.GetString(ToBytes(bits));
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
        private BitArray[] RunSelectedMode(byte[] bytes)
        {
            var initVector = ToBitArray(InitVector);
            var blocks = ToBlocks(bytes);
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
            return tdea.encrypt(initVector, keys.Item1, keys.Item2, keys.Item3, blocks);
        }
        private static byte[] FileToByteArray(string filename)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, filename);

            using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = File.ReadAllBytes(path);
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
        }
        private static void ByteArrayToFile(byte[] array, string filename)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, "enc_" + filename);
            using (FileStream fs = new(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(array, 0, array.Length);
            }
        }
        private static bool ValidateInput(object param)
        {
            return true;
        }
    }
}
