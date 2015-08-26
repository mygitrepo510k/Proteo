using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using System.IO;

namespace MWF.Mobile.Core.Services
{

    public class DiagnosticsService : IDiagnosticsService
    {
        
        private readonly Repositories.IRepositories _repositories = null;
        private readonly ILoggingService _loggingService = null;
        private readonly IReachability _reachability = null;
        private readonly IHttpService _httpService = null;
        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IUpload _upload = null;

        public DiagnosticsService(
            Repositories.IRepositories repositories,
            ILoggingService loggingService,
            IReachability reachability,
            IHttpService httpService, 
            IDeviceInfo deviceInfo,
            IUpload upload)
        {
            _repositories = repositories;
            _loggingService = loggingService;
            _reachability = reachability;
            _httpService = httpService;
            _deviceInfo = deviceInfo;
            _upload = upload;
        }

        /// <summary>
        /// This method uploads the databse to the FTP server under the Android Device ID
        /// </summary>
        /// <param name="databasePath">The locations of the mySql Database</param>
        public bool UploadDiagnostics(string databasePath)
        {

            if (!_reachability.IsConnected())
                return false;

            var config = _repositories.ConfigRepository.Get();

            if (config == null && string.IsNullOrWhiteSpace(config.FtpUrl))
            {
                Mvx.Resolve<ICustomUserInteraction>().Alert("Your Ftp Url has not been setup, you cannot upload support data unless it has been setup.");
                return false;
            }

            bool success = false;

            try
            {
                var uri = string.Format("{0}/{1}/{2}",config.FtpUrl ,_deviceInfo.AndroidId, Path.GetFileName(databasePath));
                success = _upload.UploadFile(new Uri(uri),config.FtpUsername, config.FtpPassword, databasePath);
            }
            catch(Exception ex)
            {
                _loggingService.LogEvent(ex);
            }

            return success;
           
        }

    }

}
