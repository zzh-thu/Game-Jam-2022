using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class ConveyorLogisticsUtils
    {
        internal static GlobalLogisticsSettings _settings;
        public static GlobalLogisticsSettings settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = GlobalLogisticsSettings.GetOrCreateSettings();
                }
                return _settings;
            }
        }
    }
}