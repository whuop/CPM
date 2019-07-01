using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSCmd
{
    public static class CharArrayExtensions
    {
        public static char[] SubArray(this char[] input, int startIndex, int length)
        {
            List<char> result = new List<char>();
            for (int i = startIndex; i < length; i++)
            {
                result.Add(input[i]);
            }

            return result.ToArray();
        }
    }

    static class StringExtensions
    {
        public static bool HasLineBreaks(this string expression)
        {
            if (expression == null)
                return false;
            return expression.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length > 1;
        }
    }

    class ProcessObject
    {
        private string m_fileName;
        public string FileName { get { return m_fileName; } }

        public bool IsRunning { get; private set; }

        #region Threading

        private Process m_process;
        private readonly object m_lock = new object();
        private SynchronizationContext m_context;
        private string m_pendingWriteData;

        #endregion

        public int ExitCode
        {
            get { return m_process.ExitCode; }
        }

        public event EventHandler<string> StandardOutputReceived;
        public event EventHandler<string> StandardErrorReceived;
        public event EventHandler ProcessExited;

        public ProcessObject(string fileName, params string[] arguments)
        {
            m_process = CreateProcess(fileName, arguments);
            m_fileName = fileName;
        }

        private Process CreateProcess(string fileName, params string[] args)
        {
            var processStartInfo = new ProcessStartInfo(fileName);

            //  Set arguments
            string arguments = string.Join(" ", args);
            processStartInfo.Arguments = arguments;

            //  Set options
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.StandardOutputEncoding = Encoding.UTF8;

            // Redirect inputs and outputs
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            
            
            //  Create the process
            var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = processStartInfo;

            process.Exited += ProcessExited;
            return process;
        }

        public void ExecuteAsync()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Process is still Running. Please wait for the process to complete.");
            }
            
            this.m_process.Start();

            this.m_context = SynchronizationContext.Current;
            IsRunning = true;

            new Task(this.ReadOutputAsync).Start();
            new Task(this.WriteInputTaskAsync).Start();
            new Task(this.ReadOutputErrorAsync).Start();
        }

        public void Kill()
        {
            IsRunning = false;
            m_process.Kill();
            m_process = null;
        }

        public void Write(string data)
        {
            if (data == null)
                return;

            lock(m_lock)
            {
                this.m_pendingWriteData = data;
            }
        }

        public void Writeline(string data)
        {
            this.Write(data + Environment.NewLine);
        }

        private async void ReadOutputAsync()
        {
            var stringBuilder = new StringBuilder();
            var buff = new char[1024];
            int length;

            while(!this.m_process.HasExited)
            {
                stringBuilder.Clear();

                length = await this.m_process.StandardOutput.ReadAsync(buff, 0, buff.Length);
                stringBuilder.Append(buff.SubArray(0, length));

                //  Send output to main thread
                this.OnStandardTextReceived(stringBuilder.ToString());
                Thread.Sleep(1);
            }

            IsRunning = false;
        }

        protected virtual void OnStandardTextReceived(string e)
        {
            EventHandler<string> handler = this.StandardOutputReceived;

            if (handler != null)
            {
                if (this.m_context != null)
                {
                    this.m_context.Post(delegate { handler(this, e); }, null);
                }
                else
                {
                    handler(this, e);
                }
            }
        }

        private async void ReadOutputErrorAsync()
        {
            var sb = new StringBuilder();
            do
            {
                sb.Clear();
                var buff = new char[1024];
                int length = await this.m_process.StandardError.ReadAsync(buff, 0, buff.Length);
                sb.Append(buff.SubArray(0, length));
                this.OnErrorTextReceived(sb.ToString());
                Thread.Sleep(1);
            }
            while (this.m_process.HasExited == false);
        }

        protected virtual void OnErrorTextReceived(string e)
        {
            EventHandler<string> handler = this.StandardErrorReceived;

            if (handler != null)
            {
                if (this.m_context != null)
                {
                    this.m_context.Post( delegate{ handler(this, e); }, null);
                }
                else
                {
                    handler(this, e);
                }
            }
        }

        private async void WriteInputTaskAsync()
        {
            while(this.m_process.HasExited == false)
            {
                Thread.Sleep(1);

                if (this.m_pendingWriteData != null)
                {
                    await this.m_process.StandardInput.WriteLineAsync(this.m_pendingWriteData);
                    await this.m_process.StandardInput.FlushAsync();

                    lock(m_lock)
                    {
                        this.m_pendingWriteData = null;
                    }
                }
            }
        }

        protected virtual void ProcessExitedAsync()
        {
            EventHandler handler = ProcessExited;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnProcessExited(object sender, EventArgs eventArgs)
        {
            ProcessExitedAsync();
        }

    }
}