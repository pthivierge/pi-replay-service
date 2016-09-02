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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.PI;

namespace PIReplay.Core
{
    public class PIPointsProvider
    {
        public IEnumerable<PIPoint> Points { get; set; } = null;


        public PIPointsProvider(string query, PIServer server)
        {
           

            var queries = PIPointQuery.ParseQuery(server, query);

            if(queries==null || queries.Count<1)
                throw new Exception("The query passed was invalid, it would not find any PI Point");

            Points = PIPoint.FindPIPoints(server, queries);
        }

        public IEnumerable<List<PIPoint>> GetPointsByChunks(int chunkSize)
        {

            var points=new List<PIPoint>();
          
            foreach (var piPoint in Points)
            {

                points.Add(piPoint);
                if (points.Count >= chunkSize)
                {
                    yield return points;
                    points=new List<PIPoint>();
                }
            }

            if(points.Count>0)
                yield return points;
        }


    }
}
