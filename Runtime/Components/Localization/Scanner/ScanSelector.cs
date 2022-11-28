using SturfeeVPS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public class ScanSelector : MonoBehaviour
    {
        public List<Scanner> Scanners;

        private Action<Scanner> _onScanSelected;

        public virtual void SelectScanner(Action<Scanner> onScanSelected)
        {
            _onScanSelected = onScanSelected;

            if (!Scanners.Any())
            {
                throw new Exception("No Scanners added to Scan Selector. Please add atleast one in inspector");
            }

            if(Scanners.Count == 1)
            {
                onScanSelected?.Invoke(Scanners.First());
                return;
            }
            Debug.Log(GetInstanceID());
            
            if(Scanners.Count > 1)
            {
                // check for duplicates
                HashSet<ScanType> scanTypes = new HashSet<ScanType>();  
                foreach(var scanner in Scanners)
                {
                    Debug.Log(scanner.name);
                    if(scanTypes.Contains(scanner.ScanType))
                    {
                        throw new Exception("Duplicate scanners added");
                    }
                    scanTypes.Add(scanner.ScanType);
                }

                ResolveMultiframeScanner();

                // TODO: add code here if there are other scanners other than multiframe scanners
            }
        }

        private void ResolveMultiframeScanner()
        {
            HDSitesManager.CurrentInstance.ShowSitesBrowser(
                (site) =>
                {
                    if(site == null)
                    {
                        SturfeeDebug.Log($"[ScanSelector] :: No HD Site selected. Using Satellite scanner");
                        _onScanSelected?.Invoke(Scanners.Find(x => x.ScanType == ScanType.Satellite));
                    }
                    else
                    {
                        SturfeeDebug.Log($"[ScanSelector] :: HD Site selected {site.siteName}. Using HD scanner");
                        _onScanSelected?.Invoke(Scanners.Find(x => x.ScanType == ScanType.HD));
                    }
                }
            );
        }
    }
}
