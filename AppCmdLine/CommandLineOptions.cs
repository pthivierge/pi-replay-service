using CommandLine;
using CommandLine.Text;

namespace PIReplay.CommandLine
{
    public class CommandLineOptions
    {

        [OptionArray('r',"run", HelpText = "Runs the replay process as a command line. Requires the following parameters: [server] [TagSearchQuery] ", Required = false, MutuallyExclusiveSet = "run")]
        public string[] Run { get; set; }

        [OptionArray("deleteHistory", HelpText = "Deletes Historical data for the specified time range of corresponding tags. Takes multiple parameters: [server] [delStartTime] [delEndTime] [TagSearchQuery] e.g --deleteHistory server1 *-1d * \"tag:=Unit1* AND Location1:=1 AND PointSource:=L\"", Required = false, MutuallyExclusiveSet = "deleteHist")]
        public string[] deleteHistory { get; set; }

        [HelpOption]
        public string GetUsage()
        {

            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
