using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dukebox.Logging
{
    public class Logger
    {
        private readonly static string _fileName;
        private readonly static bool _append;
        private readonly static double _maxLogSizeInMb;

        private static Mutex _logFileMutex;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        static Logger()
        {
            _fileName = Dukebox.Logging.Properties.Settings.Default.LogFileName;
            _append = Dukebox.Logging.Properties.Settings.Default.AppendLogEntries;
            _maxLogSizeInMb = Dukebox.Logging.Properties.Settings.Default.MaxLogSizeInMegabytes;

            _logFileMutex = new Mutex();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void log(string message, bool async = true)
        {
            if (Dukebox.Logging.Properties.Settings.Default.LoggingEnabled)
            {
                if (async)
                {
                    Thread fileIoThread = new Thread(() => WriteToFile(message));
                    fileIoThread.Start();
                }
                else
                {
                    WriteToFile(message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static void log(object obj, bool async = true)
        {
            log(obj.ToString(), async);
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static void WriteToFile(string message)
        {
            _logFileMutex.WaitOne();

            bool firstWrite = true;
            string assemblyName = System.Reflection.Assembly.GetCallingAssembly().GetName().Name;

            bool testStreamDisposed = false;
            FileStream testOfStream = null;
            StreamWriter testOutFile = null;

            try
            {
                FileInfo logInfo = new FileInfo(_fileName);
                testOfStream = new FileStream(_fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                testOutFile = new StreamWriter(testOfStream);

                double logSizeInMB = (((double)logInfo.Length) / 1024) / 1024;
                firstWrite = false;

                // Archive the log file it is too large in size.
                if (logSizeInMB >= _maxLogSizeInMb)
                {
                    testOutFile.WriteLine(assemblyName + "[" + DateTime.Now + "] - Log file '" + _fileName + "' archived.");

                    // Close handle to old log file so it can be moved.
                    testOutFile.Close();
                    testOfStream.Close();
                    testStreamDisposed = true;

                    File.Move(_fileName, DateTime.Now + " - " + _fileName);

                    // First write to new log file!
                    firstWrite = true;
                }
                else if (logSizeInMB == 0)
                {
                    File.Delete(_fileName);
                }
            }
            catch (Exception ex)
            {} // Log file has not been created yet.
            finally
            {           
                if (!testStreamDisposed)
                {
                    testOutFile.Close(); 
                    testOfStream.Close();
                }
            }

            StreamWriter outFile = null;

            try
            {
                FileStream ofStream = new FileStream(_fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                outFile = new StreamWriter(ofStream);
                if (firstWrite)
                {
                    outFile.WriteLine(assemblyName + "[" + DateTime.Now + "] - Log file '" + _fileName + "' created.");
                }

                outFile.WriteLine(assemblyName + "[" + DateTime.Now + "] - " + message);
            }
            catch (IOException ex)
            {} // Unable to add to log file.
            finally
            {
                ((IDisposable)outFile).Dispose();
            }

            _logFileMutex.ReleaseMutex();

            return;
        }
    }
}
