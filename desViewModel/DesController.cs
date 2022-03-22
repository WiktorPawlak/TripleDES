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
                return new RelayCommand<object>(
                    (object codingOption) =>
                    {
                        ValidateInput(codingOption);
                        string inputString = EncryptText;
                        if ((codingOption as string).StartsWith("DECRYPT"))
                        {
                            inputString = DecryptText;
                        }
                        byte[] inputText = Encoding.BigEndianUnicode.GetBytes(inputString);
                        Result = ToStr(RunSelectedMode(inputText, codingOption as string));
                        OnPropertyChanged(nameof(Result));
                    }
                    );
            }
        }

        public ICommand CypherFileCommand
        {
            get
            {
                return new RelayCommand<object>(
                    (object codingOption) =>
                    {
                        ValidateInput(codingOption);
                        string inputFilename = EncryptFileName;
                        string filePrefix = "encrypted_";
                        if ((codingOption as string).StartsWith("DECRYPT"))
                        {
                            inputFilename = DecryptFileName;
                            filePrefix = "decrypted_";
                        }
                        byte[] inputStream = FileToByteArray(inputFilename);
                        BitArray[] convertedFile = RunSelectedMode(inputStream, codingOption as string);
                        ByteArrayToFile(ToBytes(convertedFile), inputFilename, filePrefix);
                    }
                    );
            }
        }

        private static string GenerateKey() => debug.toStr(debug.genKey());
        private static BitArray ToBitArray(string text) => debug.toBitArray(text);
        private static byte[] ToBytes(BitArray[] bits) => conv.toBytes(bits);
        private static BitArray[] ToBlocks(byte[] bytes) => conv.toBlocks(bytes);
        private static string ToStr(BitArray[] bits) => Encoding.BigEndianUnicode.GetString(ToBytes(bits));

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
        private BitArray[] RunSelectedMode(byte[] bytes, string codingMode)
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
            if (codingMode.StartsWith("ENCRYPT"))
            {
                return tdea.encrypt(initVector, keys.Item1, keys.Item2, keys.Item3, blocks);
            }
            else
            {
                return tdea.decrypt(initVector, keys.Item1, keys.Item2, keys.Item3, blocks);
            }
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
        private static void ByteArrayToFile(byte[] array, string filename, string filePrefix)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, filePrefix + filename);
            using (FileStream fs = new(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(array, 0, array.Length);
            }
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

            string workingDirectory = Environment.CurrentDirectory;

            switch (textBox as string)
            {
                case "ENCRYPT FILE":
                    if (string.IsNullOrEmpty(EncryptFileName))
                    {
                        throw new ArgumentNullException("File name for encryption was null!");
                    }
                    string path = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, EncryptFileName);
                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException("File at specified path does not exits - " + path);
                    }
                    break;
                case "DECRYPT FILE":
                    if (string.IsNullOrEmpty(DecryptFileName))
                    {
                        throw new ArgumentNullException("File name for decryption was null!");
                    }
                    string path2 = Path.Combine(Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName, DecryptFileName);
                    if (!File.Exists(path2))
                    {
                        throw new FileNotFoundException("File at specified path does not exits - " + path2);
                    }
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
    }
}
