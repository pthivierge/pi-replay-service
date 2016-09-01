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
