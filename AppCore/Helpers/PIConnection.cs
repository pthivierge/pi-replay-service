using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.PI;

namespace PIReplay.Core
{
    public class PIConnection
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(PIConnection));
        private readonly PIServer _piServer;
        private readonly PIServers _piServers = new PIServers();

        /// <summary>
        ///     Initialize a PI Server Connnection object.
        ///     You should call the Connect method before access data with the PIServer property
        /// </summary>
        /// <param name="server">Name of the PI System (AF Server) to connect to</param>
        public PIConnection(string server)
        {
            if (_piServers.Contains(server))
                _piServer = _piServers[server];
            else
            {
                throw new KeyNotFoundException("Specified PI System does not exist");
            }
        }

        public PIServer GetPiServer()
        {
            return _piServer;
        }

        public bool Connect()
        {
            _logger.InfoFormat("Trying to connect to PI Data Archive {0}. As {1}", _piServer.Name,
                _piServer.CurrentUserName);

            try
            {
                _piServer.Connect();

                _logger.InfoFormat("Connected to {0}. As {1}", _piServer.Name, _piServer.CurrentUserName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }
    }
}
