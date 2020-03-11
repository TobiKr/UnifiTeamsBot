// Copyright (c) Kritten GmbH. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azuregeek.UnifiTeamsBotApp
{
    public class VoucherRequest
    {
        public String UserGivenName { get; set; }
        public String UserPrincipalName { get; set; }
        public Int32 UserActiveVouchers { get; set; }
        public String NewVoucherComment { get; set; }
        public Int32 NewVoucherValidHours { get; set; }
        public Int32 BandwidthUpKbps { get; set; }
        public Int32 BandwidthDownKbps { get; set; }
    }
}
