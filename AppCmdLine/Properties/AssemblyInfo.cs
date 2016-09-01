using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "PIReplayCommandLine.log4net.cfg.xml", Watch = true)]

//Company shipping the assembly

[assembly: AssemblyCompany("OSIsoft, LLC")]

//Friendly name for the assembly

[assembly: AssemblyTitle("PI Data Replayer - Inserts oldest values into snapshot Command Line")]

//Short description of the assembly

[assembly: AssemblyDescription("PI Data Replayer - Inserts oldest values into snapshot command line version of the application.")]
[assembly: AssemblyConfiguration("")]

//Product Name

[assembly: AssemblyProduct("PI Data Replayer - Inserts oldest values into snapshot")]

//Copyright information

[assembly: AssemblyCopyright("Copyright OSIsoft, LLC © 2016")]

//Enumeration indicating the target culture for the assembly

[assembly: AssemblyCulture("")]

//

[assembly: ComVisible(false)]


//Version number expressed as a string

[assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
