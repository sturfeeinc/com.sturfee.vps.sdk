using SturfeeVPS.UI;

namespace SturfeeVPS.SDK.Localization
{
    public class SatelliteScanController : ScanController
    {                        
        public override void OnLocalizationFail(string error, string id)
        {
            base.OnLocalizationFail(error, id);
            string errorMsg = SturfeeLocalizationProvider.Instance.GetString(id, error);
            ToastManager.Instance.ShowErrorToast(errorMsg);
        }
    }
}