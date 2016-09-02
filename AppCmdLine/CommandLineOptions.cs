#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
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
