using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private readonly DSAProvider _dsaProvider = new DSAProvider();
        private bool _isFileOpen;
        private string _fileContent = "";
        private BigInteger _r;
        private BigInteger _s;
        private bool _isSignatureGenerated = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog().Value)
            {
                string fileName = openFileDialog.FileName;
                if (fileName != null && File.Exists(fileName))
                {
                    try
                    {
                        _fileContent = File.ReadAllText(fileName);
                        ShowSourceText();
                        ClearResults();
                        _isSignatureGenerated = false;
                        _isFileOpen = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Из файла невозможно прочитать данные по причине: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Файла не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearResults()
        {
            tbResultText.Text = "";
        }

        private void ShowSourceText()
        {
            tbSourceText.Text = _fileContent;
        }

        private void ButtonGenerateSignature_Click(object sender, RoutedEventArgs e)
        {
            BigInteger q, p, k, h, x;
            if (!_isFileOpen)
            {
                MessageBox.Show("Сначала откройте файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!AreParametersFilled(out q, out p, out k, out h, out x))
            {
                MessageBox.Show("Параметры должны быть числами!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                try
                {
                    _dsaProvider.SetParameters(p, q, h, x, k);
                    var result = _dsaProvider.GenerateSignature(Encoding.UTF8.GetBytes(_fileContent));
                    _r = result.R;
                    _s = result.S;
                    _isSignatureGenerated = true;
                    ShowSignature(result);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"{ex.Message}", "Некорректные параметры", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show($"{ex.Message}", "Некорректные параметры", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowSignature(DSAGenerateSignatureResult dSAGenerateSignatureResult)
        {
            tbResultText.Text = $"Хеш = {dSAGenerateSignatureResult.Hash}\ng = {dSAGenerateSignatureResult.G}\ny = {dSAGenerateSignatureResult.Y}\nr = {_r}\ns = {_s}";
        }

        private bool AreParametersFilled(out BigInteger q, out BigInteger p, out BigInteger k, out BigInteger h, out BigInteger x)
        {
            string qString = tbQ.Text;
            string pString = tbP.Text;
            string kString = tbK.Text;
            string hString = tbH.Text;
            string xString = tbX.Text;

            q = p = k = h = x = 0;

            bool isCorrect = BigInteger.TryParse(qString, out q)
                             && BigInteger.TryParse(pString, out p)
                             && BigInteger.TryParse(kString, out k)
                             && BigInteger.TryParse(hString, out h)
                             && BigInteger.TryParse(xString, out x);
            return isCorrect;
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (_isSignatureGenerated && _isFileOpen)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (saveFileDialog.ShowDialog().Value)
                {
                    string fileName = saveFileDialog.FileName;
                    if (fileName != null)
                    {
                        try
                        {
                            WriteFileWithSignarure(fileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Из файла невозможно прочитать данные по причине: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Файла не существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Сначала создайте подпись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteFileWithSignarure(string fileName)
        {
            using var fileStreame = new StreamWriter(fileName);
            fileStreame.Write($"{_fileContent}\n{_r} {_s}");
        }

        private void ButtonVerificateSignature_Click(object sender, RoutedEventArgs e)
        {
            BigInteger q, p, k, h, x;
            if (!_isFileOpen)
            {
                MessageBox.Show("Сначала откройте файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!AreParametersFilled(out q, out p, out k, out h, out x))
            {
                MessageBox.Show("Параметры должны быть числами!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                try
                {
                    _dsaProvider.SetParameters(p, q, h, x, k);
                    _isSignatureGenerated = false;
                    BigInteger r, s;
                    string fileContentWithoutSignature;
                    bool hasSignature;
                    (fileContentWithoutSignature, r, s, hasSignature) = TrimSignatureFromFile();
                    _r = r;
                    _s = s; 
                    if (!hasSignature)
                    {
                        MessageBox.Show("В файле отсутствует подпись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else 
                    { 
                        var result = _dsaProvider.VerifySignature(Encoding.UTF8.GetBytes(fileContentWithoutSignature), r, s);
                        ShowVerificationResult(result);
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"{ex.Message}", "Некорректные параметры", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show($"{ex.Message}", "Некорректные параметры", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private (string fileContentWithoutSignature, BigInteger r, BigInteger s, bool success) TrimSignatureFromFile()
        {
            string signatureLine, fileContentWithoutSignature;
            (fileContentWithoutSignature, signatureLine) = RemoveLastLine(_fileContent);
            BigInteger r = 0, s = 0;
            bool success = true;
            string[] parts = signatureLine.Split(' ');
            if (parts.Length != 2)
            {
                success = false;
            }
            else
            {
                if (!BigInteger.TryParse(parts[0], out r) || !BigInteger.TryParse(parts[1], out s))
                {
                    success = false;
                }
            }
            return (fileContentWithoutSignature, r, s, success);
        }

        private void ShowVerificationResult(DSAVerificationResult result)
        {
            if (!result.IsSignatureInBounds)
            {
                tbResultText.Text = "r и s за пределом допустимых границ\nПодпись некорректна";
            }
            else
            {
                string resultText = $"Хеш = {result.Hash}\nw = {result.W}\nu1 = {result.U1}\nu2 = {result.U2}\nv = {result.V}\ns = {_s}\nr = {_r}\n";
                if (result.Result)
                {
                    resultText += "v == r => Подпись КОРРЕКТНА";
                }
                else
                {
                    resultText += "v != r => Подпись НЕКОРРЕКТНА";
                }
                tbResultText.Text = resultText;
            }
        }

        public static (string originalWithoutLastLine, string lastLine) RemoveLastLine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return (string.Empty, string.Empty);

            int lastNewLinePos = -1;
            char prevChar = '\0';

            // Ищем последний перенос строки с конца
            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (text[i] == '\n')
                {
                    // Проверяем, является ли это частью "\r\n"
                    if (i > 0 && text[i - 1] == '\r')
                        lastNewLinePos = i - 1; // Захватываем оба символа
                    else
                        lastNewLinePos = i; // Только '\n'
                    break;
                }
                else if (text[i] == '\r')
                {
                    lastNewLinePos = i; // Только '\r'
                    break;
                }
                prevChar = text[i];
            }

            if (lastNewLinePos == -1)
            {
                // Переносов нет: вся строка — последняя
                return (string.Empty, text);
            }
            else
            {
                // Вычисляем длину переноса (1 для \r или \n, 2 для \r\n)
                int newLineLength = (text[lastNewLinePos] == '\r' &&
                                   lastNewLinePos + 1 < text.Length &&
                                   text[lastNewLinePos + 1] == '\n') ? 2 : 1;

                string originalWithoutLastLine = text.Substring(0, lastNewLinePos);
                string lastLine = text.Substring(lastNewLinePos + newLineLength);

                return (originalWithoutLastLine, lastLine);
            }
        }
    }
}