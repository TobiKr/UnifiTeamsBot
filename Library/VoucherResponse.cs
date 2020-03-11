// Copyright (c) Kritten GmbH. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azuregeek.UnifiTeamsBotApp
{
    public class VoucherResponse
    {
        public String VoucherCode { get; set; }
        public DateTime VoucherValidUntil { get; set; }        
        public String VoucherComment { get; set; }
        public Int32 BandwidthUpKbps { get; set; }
        public Int32 BandwidthDownKbps { get; set; }
        public String HotspotSSID { get; set; }
        public String UserGivenName { get; set; }
    }
}
