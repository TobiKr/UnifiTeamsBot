// Copyright (c) Kritten GmbH. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Newtonsoft.Json;

namespace azuregeek.UnifiTeamsBotApp
{
    public class AdaptiveCardHandler
    {
        public static Attachment CreateHotspotVoucherAdaptiveCard(VoucherResponse response)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            // Define Adaptive Card Container
            List<AdaptiveElement> adaptiveElements = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet()
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                            Width = AdaptiveColumnWidth.Auto,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveImage()
                                {
                                    Style = AdaptiveImageStyle.Person,
                                    Url = new Uri("https://img.favpng.com/1/24/3/wi-fi-wireless-network-icon-clip-art-png-favpng-QhKK6rDubd8RMqLN2zZB4mt83.jpg"),
                                    Size = AdaptiveImageSize.Large
                                }
                            }
                        },
                        new AdaptiveColumn()
                        {
                            Width = AdaptiveColumnWidth.Stretch,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Text = $"Voucher Code: {response.VoucherCode}",
                                    Wrap = true,
                                    Weight = AdaptiveTextWeight.Bolder
                                },
                                new AdaptiveTextBlock()
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Text = $"Netzwerk-Bezeichnung: {response.HotspotSSID}",
                                    Wrap = true,
                                    IsSubtle = true
                                },
                                new AdaptiveTextBlock()
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Text = $"Geschwindigkeit: {(response.BandwidthDownKbps/1024).ToString()}Mbit Down / {(response.BandwidthUpKbps/1024).ToString()}Mbit Up",
                                    Wrap = true,
                                    IsSubtle = true
                                },
                                new AdaptiveTextBlock()
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Text = $"Gültig bis: {response.VoucherValidUntil.ToString("dd.MM.yyyy HH:mm")}",
                                    Wrap = true,
                                    IsSubtle = true
                                }
                            }
                        }                       
                    }
                }
            };

            AdaptiveContainer adaptiveCardContainer = new AdaptiveContainer();
            adaptiveCardContainer.Items = adaptiveElements;

            // Define Adaptive Card Heading

            AdaptiveTextBlock headerTextBlock = new AdaptiveTextBlock()
            {                
                Text = $"WiFi Voucher für {response.UserGivenName}",
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            };

            // Put adaptive card together
            card.Body.Add(headerTextBlock);
            card.Body.Add(adaptiveCardContainer);

            // Create return value
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;
        }
        
        public static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
