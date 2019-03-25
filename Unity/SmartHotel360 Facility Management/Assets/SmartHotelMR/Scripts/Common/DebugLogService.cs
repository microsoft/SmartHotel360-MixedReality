using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using UnityEngine;

namespace SmartHotelMR
{
    public class DebugLogService : Singleton<DebugLogService>
    {
#if DEBUG && !WINDOWS_UWP
        private const string LogFile = "SmartHotelMR_Log.txt";
        private const int MaxLogLength = 64; // This value is in kilobytes

        private Queue<DebugMessage> _logQueue = new Queue<DebugMessage>();
        private object _lock = new object();

        private string _logPath;
        private bool _monitoring;
        private Thread _queueThread;
        private AutoResetEvent _event;

        private void Start()
        {
            PerformLogCheck();

            _event = new AutoResetEvent(false);
            _queueThread = new Thread(new ThreadStart(ProcessQueueThread));
            _queueThread.IsBackground = true;
            _queueThread.Start();

            Application.logMessageReceivedThreaded += HandleLog;
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        public IEnumerator GetLogText(Action<string> callback)
        {
            bool processed = false;
            string data = string.Empty;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, e) =>
            {
                try
                {
                    if (File.Exists(_logPath))
                    {
                        using (FileStream stream = File.Open(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                e.Result = reader.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No debug log found, creating...");
                        e.Result = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("GetLogText: " + ex.ToString());
                    e.Result = string.Empty;
                }
            };

            worker.RunWorkerCompleted += (s, args) =>
            {
                data = args.Result.ToString();
                processed = true;
            };

            worker.RunWorkerAsync();

            yield return new WaitUntil(() => processed);

            if (callback != null)
                callback(data);
        }

        public void ClearLog()
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    using (FileStream stream = File.Open(_logPath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite))
                    {
                        print("DebugLog: Log Cleared");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("ClearLog: " + ex.ToString());
            }
        }

        private void PerformLogCheck()
        {
            try
            {
                if (string.IsNullOrEmpty(_logPath))
                    _logPath = Path.Combine(Application.persistentDataPath, LogFile);

                var info = new FileInfo(_logPath);

                if (info.Length / 1024 >= MaxLogLength)
                {
                    print(string.Format("DebugLog: Truncating log, current size is {0}k", (info.Length / 1024).ToString()));
                    TruncateLog();
                }
            }
            catch (Exception ex)
            {
                print("DebugLog: PerformLogCheck Error - " + ex.ToString());
            }
        }

        private void TruncateLog()
        {
            try
            {
                int truncatedLength = ((MaxLogLength - 10) * 1024);

                byte[] data = new byte[truncatedLength];

                using (FileStream stream = File.Open(_logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.Seek(-truncatedLength, SeekOrigin.End);

                    stream.Read(data, 0, truncatedLength);

                    stream.SetLength(0);
                    stream.Seek(0, SeekOrigin.Begin);

                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch (Exception e)
            {
                print("DebugLog: TruncateLog Error - " + e.ToString());
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (logString.StartsWith("DebugLog"))
                return;

            string timestampedMsg = string.Format("{0} - {1}:\t{2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), logString);

            var msg = new DebugMessage() { Message = timestampedMsg };

            if (type == LogType.Exception || type == LogType.Error)
            {
                msg.StackTrace = stackTrace;
            }

            lock (_lock)
            {
                _logQueue.Enqueue(msg);
                _event.Set();
            }
        }

        private void ProcessQueueThread()
        {
            try
            {
                while (true)
                {
                    using (FileStream stream = File.Open(_logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            while (_logQueue.Count > 0)
                            {
                                DebugMessage msg;

                                lock (_lock)
                                {
                                    msg = _logQueue.Dequeue();
                                }

                                writer.WriteLine(msg.Message);

                                if (!string.IsNullOrEmpty(msg.StackTrace))
                                {
                                    writer.WriteLine("\t\t" + msg.StackTrace);
                                }

                                writer.WriteLine();
                            }

                            writer.Flush();
                        }
                    }

                    PerformLogCheck();
                    _event.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("DebugLog: ProcessQueueThread: " + ex.ToString());
            }
        }
#endif
    }

    public struct DebugMessage
    {
        public string Message;
        public string StackTrace;
    }
}