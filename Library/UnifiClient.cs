// Copyright (c) Kritten GmbH. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniFiSharp;

namespace azuregeek.UnifiTeamsBotApp
{
    public class UnifiClient
    {
        private readonly UniFiApi _uniFiApi;

        public UnifiClient(string username, string password, Uri uri)
        {
            // Create a new Api instance to connect with the UniFi Controller
            _uniFiApi = new UniFiApi(uri, username, password);            
        }

        public async Task<VoucherResponse> CreateGuestVoucher(VoucherRequest request)
        {
            VoucherResponse response = new VoucherResponse();
            String vocherNote = "Req. by " + request.UserPrincipalName + " for purpose " + request.NewVoucherComment;
            Int32 voucherValidMinutes = request.NewVoucherValidHours * 60;

            //Authenticate against API
            await _uniFiApi.Authenticate();

            //Create Voucher
            UniFiSharp.Json.JsonHotspotVoucher createdVoucher = await _uniFiApi.HotspotVoucherAdd(null, request.BandwidthDownKbps, request.BandwidthUpKbps, voucherValidMinutes.ToString(), 1, vocherNote, 1);
            UniFiSharp.Json.JsonHotspotVoucher voucher = (await _uniFiApi.HotspotVoucherGet(createdVoucher.createTime)).First();

            // Verify if voucher was created for current user - as unifi responds with create time as identifier only and pull in data from voucher to response object
            if (voucher.note.Contains(request.UserPrincipalName) && voucher.createTime == createdVoucher.createTime)
            {
                response.VoucherCode = voucher.code.Substring(0, 5) + "-" + voucher.code.Substring(5, 5);
                response.VoucherComment = voucher.note;
                // Calculate validity based on Unix Timestamp + create Date + Duration in Minutes
                response.VoucherValidUntil = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(voucher.createTime).AddMinutes(voucher.duration);
                response.BandwidthDownKbps = (Int32)voucher.qosRateMaxDown;
                response.BandwidthUpKbps = (Int32)voucher.qosRateMaxUp;
                response.UserGivenName = request.UserGivenName;
            }
            else
                throw new Exception("Voucher Code received does not match created voucher code");
            return response;
        }
    }
}
