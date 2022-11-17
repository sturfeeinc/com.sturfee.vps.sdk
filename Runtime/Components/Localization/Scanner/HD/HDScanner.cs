using SturfeeVPS.Core;
using SturfeeVPS.Core.Proto;
using System.Threading.Tasks;

namespace SturfeeVPS.SDK
{
    public class HDScanner : MultiframeScanner
    {
        public override async Task Initialize(uint requestNum)
        {
            var site = HDSitesManager.CurrentInstance.CurrentSite;
            var location = new GeoLocation{ Latitude = site.latitude, Longitude = site.longitude };

            await WaitForSessionProviders();
            SturfeeDebug.Log($" [HDScanner] :: Providers ready");

            await ValidateToken(TokenUtils.GetVpsToken());

            _localizationService = CreateLocalizationService();
            await _localizationService.Connect(_serviceUrl, TokenUtils.GetVpsToken(), location.Latitude, location.Longitude);                            

            await base.Initialize(requestNum);
        }

        protected override async Task WaitForSessionProviders()
        {
            SturfeeDebug.Log($" [HDScanner] :: Waiting for session providers...");
            await Task.WhenAll(
                WaitForProvider<IPoseProvider>(),
                WaitForProvider<IVideoProvider>()
            );
        }

        protected override Request BuildRequest(uint frameOrder, uint numOfFrames)
        {
            var request = base.BuildRequest(frameOrder, numOfFrames);
            request.SiteId = HDSitesManager.CurrentInstance.CurrentSite.siteId;
            return request;
        }
    }
}
