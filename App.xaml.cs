using System;
using System.Linq;
using System.Windows;

namespace Options.File.Checker.WPF
{
    public partial class App : Application
    {
        public bool Debug { get; private set; } = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check if debug mode was enabled.
            if (e.Args.Contains("-debug"))
            {
                Debug = true;
            }
        }
    }
}
