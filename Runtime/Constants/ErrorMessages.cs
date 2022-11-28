using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.SDK
{
    internal class ErrorMessages
    {
        public static (string, string) TileLoadingError = ("TileLoadingError", "Tile loading error");
        public static (string, string) TileLoadingErrorFromCache = ("TileLoadingErrorFromCache", "Tile loading error. Center Reference NUll in cache");
        public static (string, string) TileDownloadingError = ("TileDownloadingError", "Tile downloading error");

        public static (string, string) NoCoverageArea = ("NoCoverageArea", "VPS not available at this location");

        // localization service
        public static (string, string) SocketConnectionFail = ("SocketConnectionFail", "Socket connection failed");
        public static (string, string) LocalizationService_NoError = ("NoError", "no error");
        public static (string, string) LocalizationService_UnexpectedServerError = ("UnexpectedServerError", "unexpected server error");
        public static (string, string) LocalizationService_ProtocolError = ("ProtocolError", "protocol error");
        public static (string, string) LocalizationService_ValidationError = ("ValidationError", "validation error");
        public static (string, string) LocalizationService_OutOfCoverageError = ("OutOfCoverageError", "requested location is out of coverage");


        public static (string, string) ScanInitializationError = ("scan-init-error", "Error while initializing scan. Try agian later");

        #region Providers
        public static (string, string) ProvidersTimeout = ("ProvidersTimeout", " Session TimeOut");

        public static (string, string) GpsNotReadyForScan = ("GpsNotreadyForScan", " GPS Provider not ready for scan");
        public static (string, string) GpsProviderNotSupported = ("GpsProviderNotSupported", " GPS Error");
        public static (string, string) GpsProviderTimeout = ("GpsProviderTimeout", " GPS TimeOut");

        public static (string, string) PoseNotReadyForScan = ("PoseNotreadyForScan", " Pose Provider not ready for scan");
        public static (string, string) PoseProviderNotSupported = ("PoseProviderNotSupported", " IMU Error");
        public static (string, string) PoseProviderTimeout = ("PoseProviderTimeout", " Pose/IMU TimeOut");

        public static (string, string) VideoNotReadyForScan = ("VideoNotreadyForScan", " Video Provider not ready for scan");
        public static (string, string) VideoProviderNotSupported = ("VideoProviderNotSupported", " Camera Error");
        public static (string, string) VideoProviderTimeout = ("VideoProviderTimeout", " Video Camera TimeOut");

        #endregion

        #region Http
        public static (string, string) Error400 = ("Error400", "Invalid URL request parameters");
        public static (string, string) Error403 = ("Error403", "Invalid Token !");
        public static (string, string) Error500 = ("Error500", "Server Connection Failed. Please try again");

        public static (string, string) HttpErrorGeneric = ("HttpErrorGeneric", "HTTP Connection Error. Please try again !");
        #endregion
    }
}
