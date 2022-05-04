using System;
using System.Collections.Generic;

namespace penguin_new
{

    public class LaunchArgs
    {
        private List<string> _args;
        public LaunchArgs(string[] _arr)
        {
            _args = new List<string>(_arr);
        }

        /// <summary>
        /// Returns true if the program was launched with the specified launch argument.
        /// </summary>
        /// <param name="_str">Argument to check</param>
        /// <returns></returns>
        public bool GetArgument(string _str)
        {
            return (_args.Contains(_str));
        }
    }
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static LaunchArgs Args;

        public static Main MainThread;
        [STAThread]
        static void Main(string[] _args)
        {
            Args = new LaunchArgs(_args);
            using (MainThread = new Main())
                MainThread.Run();
        }
    }
}