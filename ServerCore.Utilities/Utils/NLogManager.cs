using System;
using System.Diagnostics;
using System.Text;
using NLog;

namespace ServerCore.Utilities.Utils
{
    public static class NLogManager
    {
        /// <summary>
        /// 
        /// </summary>
        //volatile static Logger Log = LogManager.GetLogger("CardGame");
        //volatile static Logger AuthenLog = LogManager.GetLogger("Authen");

        volatile static Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Writes an Error to the log.
        /// </summary>
        /// <param name="ex"></param>
        public static void Exception(Exception ex)
        {
            Log.Error(string.Format("{0}{1}{2}{3}{4}", GetCalleeString(), Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
        }

        public static void Error(Exception ex)
        {
            Log.Error(string.Format("{0}{1}{2}{3}{4}", GetCalleeString(), Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
        }

        public static void Error(string message)
        {
            Log.Error(GetCalleeString() + Environment.NewLine + "\t" + message);
        }

        /// <summary>
        /// Writes an Error to the log.
        /// </summary>
        /// <param name="ex"></param>
        public static void Info(string message)
        {
            string caller = GetCalleeString();
            Log.Info(GetCalleeString() + Environment.NewLine + "\t" + message);
            //FileTarget target = new FileTarget();
            //target.Layout = "${longdate} ${logger} ${message}";
            //target.FileName = "${basedir}/_LOG/" + "/${date:format=yyyyMMdd}_CardGame.txt";
            //target.ArchiveFileName = "${basedir}/archives/${date:format=yyyyMMdd}_CardGame_Log.txt";
            //target.ArchiveAboveSize = 1024 * 1024 * 100; // archive files greater than 10 KB
            //target.Name = "Debug";
            //target.ConcurrentWrites = true;
            //target.ConcurrentWrites = true;
            //NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);
            //Log.Factory.Configuration.AddTarget("CardGame", target);

            //Log.Info(" " + GetCalleeString() + ": " + message);
        }

        //public static void WarningAuthen(string message)
        //{
        //    AuthenLog.Warn(message);
        //}

        //public static void LogMessageAuthen(string message)
        //{
        //    AuthenLog.Info(message);
        //}

        //public static void ExceptionAuthen(Exception ex)
        //{
        //    AuthenLog.Error(ex);
        //}

        /// <summary>
        /// Writes an Error to the log.
        /// </summary>
        /// <param name="ex"></param>
        public static void Warning(string message)
        {
            Log.Warn(string.Format(":\t{0}{1}\t{2}", GetCalleeString(), Environment.NewLine, message));
        }

        /// <summary>
        /// Writes an Debug to the log.
        /// </summary>
        /// <param name="ex"></param>
        public static void Debug(string message)
        {
            Log.Error(string.Format(":\t{0}{1}\t{2}", GetCalleeString(), Environment.NewLine, message));
        }

        public static string GetValueOfObject(object ob)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (System.Reflection.PropertyInfo piOrig in ob.GetType().GetProperties())
                {
                    object editedVal = ob.GetType().GetProperty(piOrig.Name).GetValue(ob, null);
                    sb.AppendFormat("{0}:{1}\t ", piOrig.Name, editedVal);
                }
            }
            catch
            {
            }
            return sb.ToString();
        }

        private static string GetCalleeString()
        {
            try
            {
                foreach (StackFrame sf in new StackTrace().GetFrames())
                {
                    if (!string.IsNullOrEmpty(sf.GetMethod().ReflectedType.Namespace) && !typeof(NLogManager).FullName.StartsWith(sf.GetMethod().ReflectedType.Namespace))
                    {
                        return string.Format("{0}.{1} ", sf.GetMethod().ReflectedType.Name, sf.GetMethod().Name);
                    }
                }
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }
    }
}