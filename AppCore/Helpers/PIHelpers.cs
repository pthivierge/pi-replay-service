using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace PIReplay.Core.Helpers
{
    public static class PIHelpers
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PIHelpers));
        public static void DeleteValues(PIPoint piPoint, AFTime st, AFTime et)
        {
            var timerange = new AFTimeRange(st, et);

            if (piPoint.Server.Supports(PIServerFeature.DeleteRange))
            {
                var errors = piPoint.ReplaceValues(timerange, new List<AFValue>());

                if (errors != null && errors.HasErrors)
                {
                    _logger.Error(errors.Errors);
                    _logger.Error(errors.PIServerErrors);
                }
            }

            else
            {   // fallback on the old fashion way of doing the delete
                _logger.Warn("The ReplaceValues method is not implemented on the target PI DataArchive, falling back to the previous longer method ( read and remove)");
                var dataToDelete = piPoint.RecordedValues(timerange, AFBoundaryType.Inside, string.Empty, false);
                piPoint.UpdateValues(dataToDelete, AFUpdateOption.Remove);

            }



        }
    }
}
