using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;
using PIReplay.Core;


namespace PIReplay.CommandLine
{
    /// <summary>
    ///     Command line program "Main"
    ///     logs are both sent to the log file and in the console
    ///     This can be configured in CommandLine.Log4Net.cfg
    /// </summary>
    internal class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        
        private static bool ValidateRunOptions(string[] options)
        { // [server] [TagSearchQuery]

            if (options.Length != 2)
                throw new Exceptions.InvalidParameterException("run", "The number of parameters with this option must be 2.");
            return true;
        }

        private static bool ValidateDeleteHistoryOptions(string[] options)
        { // [server] [delStartTime] [delEndTime] [TagSearchQuery]

            if (options.Length != 4)
                throw new Exceptions.InvalidParameterException("deleteHistory", "The number of parameters with this option must be 4.");

            // check the times passed, exception will be thrown is time passed is not valid
            AFTime.Parse(options[1]);
            AFTime.Parse(options[2]);

            return true;
        }

        private static void Main(string[] args)
        {
            TextWriter writer = Console.Out;

            try
            {
                var options = new CommandLineOptions();
                if (Parser.Default.ParseArguments(args, options))
                {

                    if (args.Length <= 1)
                        Console.Write(options.GetUsage());



                    if (options.Run != null && ValidateRunOptions(options.Run))
                    {
                        _logger.Info("Option Run starting, will run the data replay continuously as a command line application.");

                        var replayer = new Replayer();
                        replayer.RunFromCommandLine(options.Run[0], options.Run[1]);


                    }

                    if (options.deleteHistory != null && ValidateDeleteHistoryOptions(options.deleteHistory))
                    {
                        _logger.Info("Delete History Option Selected, will deleted the specified data.");

                        _logger.Info("This operation cannot be reversed, are you sure you want to delete the data you specified? Press Y to continue...");

                        var keyInfo = Console.ReadKey();
                        if (keyInfo.KeyChar != 'Y')
                        {
                            _logger.Info("Operation canceled");
                        }

                        // getting the tags
                        var piConnection = new PIConnection(options.deleteHistory[0]);

                        piConnection.Connect();

                        var pointsProvider = new PIPointsProvider(options.deleteHistory[3], piConnection.GetPiServer());

                        foreach (var piPoint in pointsProvider.Points)
                        {
                            var st = AFTime.Parse(options.deleteHistory[1]);
                            var et = AFTime.Parse(options.deleteHistory[2]);
                            _logger.InfoFormat("Deleting history for tag: {0} between {1:G} and {2:G}", piPoint.Name, st.LocalTime, et.LocalTime);

                            piPoint.ReplaceValues(new AFTimeRange(st, et), new List<AFValue>());
                        }

                    }

                }

                else
                {
                    options.GetUsage();
                }

            }



            catch (Exception ex)
            {
                Console.SetOut(writer);
                Console.WriteLine("Error: " + ex);
            }
        }
    }
}
