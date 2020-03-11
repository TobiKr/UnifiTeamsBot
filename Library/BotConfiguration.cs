// Copyright (c) Kritten GmbH. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azuregeek.UnifiTeamsBotApp
{
    public class BotConfiguration
    {
        public String AzureADConnectionName { get; set; }
        public String UnifiAPIUsername { get; set; }
        public String UnifiAPIPassword { get; set; }
        public Uri UnifiAPIUri { get; set; }
        public Guid AllowedTenantId { get; set; }
        public Int32 BandwidthUpKbps { get; set; }
        public Int32 BandwidthDownKbps { get; set; }
        public String HotspotSSID { get; set; }
    }
}
