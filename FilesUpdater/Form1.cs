using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FilesUpdater.DTO;
using FilesUpdater.Infrastructure;

namespace FilesUpdater
{
    public partial class Form1 : Form
    {
        private string _logFileName;
        private string _hostErrorsFileName;
        private readonly string _workDirectory;
        private CancellationTokenSource _tokenSource;
        private Task _task;

        public Form1()
        {
            InitializeComponent();

            _tokenSource = new CancellationTokenSource();
            _workDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                _logFileName = Path.Combine(_workDirectory, StartupSettings.Settings.LogFileName);
                _hostErrorsFileName = Path.Combine(_workDirectory, StartupSettings.Settings.HostErrorsFileName);

                textBox1.Text = Path.Combine(_workDirectory, StartupSettings.Settings.SourcePathName);
                textBox2.Text = StartupSettings.Settings.RemouteHostDestinationPath;
                textBox3.Text = StartupSettings.Settings.FileSearchMask;
                textBox4.Text = StartupSettings.Settings.ExecuteScriptParams;
                checkBox1.Checked = StartupSettings.Settings.HandleInternalDirectories;
                checkBox2.Checked = StartupSettings.Settings.ExecuteScript;

                richTextBox1.LoadFile(Path.Combine(_workDirectory, StartupSettings.Settings.RemouteHostSourceFileName), RichTextBoxStreamType.PlainText);
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex?.InnerException.Message}", "Некорректные настройки приложения");
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка инициализации настроек");
                Application.Exit();
            }

            ChangeUserInterface(false);
        }

        private void UpdateProgress(object state)
        {
            try
            {
                if (state == null)
                    throw new ArgumentNullException("Отсутствует состояние");

                var uiState = state as ProgressState;
                if (uiState == null)
                    throw new NullReferenceException("Ошибка получения состояния");

                toolStripProgressBar1.Value = uiState.ProgressValue;
                toolStripStatusLabel3.Text = uiState.HostName;
                WriteMessageToLog(uiState);

                if (uiState.State == EventType.Complete)
                    ChangeUserInterface(false);
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText($"Ошибка обновления данных формы.\n{ex.Message}");
            }
        }

        private void AddMessageHistory(object state)
        {
            try
            {
                if (state == null)
                    throw new ArgumentNullException("Отсутствует состояние");

                if (!(state is ProgressState uiState))
                    throw new NullReferenceException("Ошибка получения состояния");

                WriteMessageToLog(uiState);

                if (uiState.State == EventType.Complete)
                    ChangeUserInterface(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления записи в лог:\n{ex.Message}");
            }
        }

        private void ChangeUserInterface(bool isEnabled)
        {
            button1.Enabled = !isEnabled;
            button2.Enabled = isEnabled;

            toolStripStatusLabel1.Visible = isEnabled;
            toolStripStatusLabel3.Visible = isEnabled;
            toolStripProgressBar1.Visible = isEnabled;

            if (!isEnabled)
                return;

            richTextBox2.Clear();
            toolStripStatusLabel3.Text = "";
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Minimum = 0;
        }

        private void WriteMessageToLog(ProgressState uiState)
        {
            switch (uiState.State)
            {
                case EventType.Error:
                    richTextBox2.AppendText($"{uiState.HostName}\t{uiState.Message}\n");
                    Logger.WriteLog(_hostErrorsFileName, uiState.HostName);
                    Logger.WriteLog(_logFileName, Logger.WrapMessage(uiState.HostName, uiState.Message));
                    break;
                case EventType.Info:
                case EventType.Complete:
                    richTextBox2.AppendText(string.IsNullOrEmpty(uiState.HostName)
                        ? $"{uiState.Message}\n"
                        : $"{uiState.HostName}\t{uiState.Message}\n");
                    Logger.WriteLog(_logFileName, Logger.WrapMessage(uiState.HostName, uiState.Message));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("Директория с исходными файлами не существует.");
                return;
            }

            try
            {
                if (File.Exists(_logFileName))
                {
                    File.SetAttributes(_logFileName, FileAttributes.Normal);
                    File.Delete(_logFileName);
                }

                if (File.Exists(_hostErrorsFileName))
                {
                    File.SetAttributes(_hostErrorsFileName, FileAttributes.Normal);
                    File.Delete(_hostErrorsFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления лог файла:\n{ex.Message}");
                return;
            }

            try
            {
                //TODO: Проверить корректность реализации с токенами отмены
                _tokenSource = new CancellationTokenSource();
                var token = _tokenSource.Token;

                var data = new TaskParams()
                {
                    Token = token,
                    Context = SynchronizationContext.Current,
                    DestinationHostNames = richTextBox1.Lines,
                    SourcePath = textBox1.Text,
                    DestinationPath = textBox2.Text,
                    SearchPattern = textBox3.Text,
                    ExecuteScript = checkBox2.Checked,
                    ScriptParams = textBox4.Text,
                    ScriptTimeout = StartupSettings.Settings.ExecuteScriptTimeout,
                    ExecuteInternalDirectories = checkBox1.Checked
                };

                toolStripProgressBar1.Maximum = richTextBox1.Lines.Length;
                ChangeUserInterface(true);

                Action<object> action = RunCopyFiles;
                _task = new Task(action, data, token);
                _task.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска потока обработки данных:\n{ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                _tokenSource.Cancel();
                _task.Wait();
            }
            catch (AggregateException ex)
            {
                MessageBox.Show($"Ошибка выполнения потока обработки данных:\n{ex.InnerExceptions[0]?.Message}");
            }
            catch (Exception iex)
            {
                MessageBox.Show($"Ошибка выполнения потока обработки данных:\n{iex.Message}");
            }

            ChangeUserInterface(false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox1.Text;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void RunCopyFiles(object paramsData)
        {
            if (!(paramsData is TaskParams data))
                throw new NullReferenceException("data");

            var uiContext = data.Context;
            if (uiContext == null)
                throw new NullReferenceException("uiContext");

            uiContext.Post(UpdateProgress, new ProgressState(EventType.Info, String.Empty, "Запуск процесса обработки"));
            var progressVal = 1;

            foreach (var host in data.DestinationHostNames)
            {
                try
                {
                    if (data.Token.IsCancellationRequested)
                    {
                        data.Token.ThrowIfCancellationRequested();
                    }

                    if (string.IsNullOrEmpty(host.Trim()))
                    {
                        progressVal++;
                        continue;
                    }

                    HandleData(data, host, data.SourcePath, Path.Combine($@"\\{host}", data.DestinationPath));
                    uiContext.Post(UpdateProgress, new ProgressState(EventType.Info, progressVal, host, "Копирование файлов и директорий завершено"));
                }
                catch (OperationCanceledException)
                {
                    uiContext.Post(UpdateProgress, new ProgressState(EventType.Complete, host, "Принудительная остановка процесса обработки"));
                    return;
                }
                catch (Exception ex)
                {
                    uiContext.Post(UpdateProgress, new ProgressState(EventType.Error, progressVal++, host, ex.Message));
                }
            }

            uiContext.Post(UpdateProgress, new ProgressState(EventType.Complete, String.Empty, "Процесс обработки завершен"));
        }

        private void HandleData(TaskParams data, string host, string sourcePath, string destPath)
        {
            if (data.ExecuteInternalDirectories)
                HandleDirectories(data, host, sourcePath, destPath);

            // Создание файлов необходимо выполнять после создания директории,
            // либо реализовать создание директорий перед рекурсивным вызовом HandleData
            HandleFiles(data, host, sourcePath, destPath);
        }

        private void HandleDirectories(TaskParams data, string host, string sourcePath, string destPath)
        {
            try
            {
                var directories = Directory.EnumerateDirectories(sourcePath);
                foreach (var directory in directories)
                {
                    if (data.Token.IsCancellationRequested)
                    {
                        data.Token.ThrowIfCancellationRequested();
                    }

                    var sections = directory.Split(new[] {@"\"}, StringSplitOptions.RemoveEmptyEntries);
                    var dir = sections[sections.Length - 1];
                    HandleData(data, host, directory, Path.Combine(destPath, dir));
                }

                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                data.Context.Post(AddMessageHistory, new ProgressState(EventType.Error, host, ex.Message));
            }
        }

        private void HandleFiles(TaskParams data, string host, string sourcePath, string destPath)
        {
            try
            {
                var files = Directory.EnumerateFiles(sourcePath, data.SearchPattern);

                foreach (var file in files)
                {
                    if (data.Token.IsCancellationRequested)
                    {
                        data.Token.ThrowIfCancellationRequested();
                    }

                    var fileName = file.Substring(sourcePath.Length + 1);
                    var destFileName = Path.Combine(destPath, fileName);

                    try
                    {
                        CopyFile(file, destFileName);
                    }
                    catch (Exception ex)
                    {
                        //System.UnauthorizedAccessException || System.IO.IOException
                        try
                        {
                            if (!data.ExecuteScript || Path.GetExtension(fileName) != ".exe")
                            {
                                data.Context.Post(AddMessageHistory, new ProgressState(EventType.Error, host, ex.Message));
                                continue;
                            }

                            PrepareCommandScript(data, host, fileName);
                            CopyFile(file, destFileName);
                        }
                        catch (Exception iex)
                        {
                            data.Context.Post(AddMessageHistory, new ProgressState(EventType.Error, host, iex.Message));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                data.Context.Post(AddMessageHistory, new ProgressState(EventType.Error, host, ex.Message));
            }
        }

        private static void CopyFile(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName))
            {
                File.SetAttributes(destFileName, FileAttributes.Normal);
                File.Delete(destFileName);
            }

            File.Copy(sourceFileName, destFileName, true);
        }

        private void PrepareCommandScript(TaskParams data, string host, string fileName)
        {
            var @params = new Dictionary<string, string> {{"HostName", host}, {"ProcessName", fileName}};
            ExecuteCommandScript(data.ScriptParams, @params);
            data.Context.Post(AddMessageHistory, new ProgressState(EventType.Info, host, "Выполнение скрипта завершено"));
            Thread.Sleep(data.ScriptTimeout);
        }

        private static void ExecuteCommandScript(string command, Dictionary<string, string> @params)
        {
            var cmd = command;
            foreach (var param in @params)
            {
                cmd = cmd.Replace($"[{param.Key}]", param.Value);
            }

            var p = new ProcessStartInfo("cmd.exe") { Arguments = $"/c {cmd}" };
            Process.Start(p);
        }
    }
}
