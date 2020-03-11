// Copyright (c) Kritten GmbH. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace azuregeek.UnifiTeamsBotApp
{
    public static class JwtTokenUtil
    {
        // Extracts Azure Tenant ID from Azure AD JWT Token and returns it as GUID
        public static Guid GetAzureTenantIdFromToken(String jwtInput)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(jwtInput);
            var tid = token.Claims.Where(c => c.Type == "tid").Select(x => x.Value).FirstOrDefault();

            return new Guid(tid);
        }
    }
}
