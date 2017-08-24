using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ClientSmpp
{
    static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        static void Main(string[] args)
		{
			if (Environment.UserInteractive)
			{
				string parameter = string.Concat(args);
				switch (parameter)
				{
					case "--install":
					ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
					break;
					case "--uninstall":
					ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
					break;
				}
			}
			else
			{
				ServiceBase[] servicesToRun = new ServiceBase[] 
								  { 
									  new Service1() 
								  };
				ServiceBase.Run(servicesToRun);
			}
		}
    }
}
