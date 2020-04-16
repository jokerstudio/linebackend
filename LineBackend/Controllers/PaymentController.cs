using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LineBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize(string transactionId, string orderId)
        {
            using (var client = new HttpClient())
            {
                var baseUrl = "https://api-pay.line.me";
                var requestUrl = $"/v3/payments/{transactionId}/confirm";
                var channelId = "1653957456";
                var channelSecret = "c35bfea4d5ae857d9f1c15614c48e935";
                var nonce = Guid.NewGuid().ToString();

                var product = new ConfirmInfo
                {
                    Amount = 0.02,
                    Currency = "THB"
                };

                var payload = JsonConvert.SerializeObject(product, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                var data = channelSecret + requestUrl + payload + nonce;
                var hashHMAC = HashHMAC(channelSecret, data);

                client.DefaultRequestHeaders.Add("X-LINE-ChannelId", channelId);
                client.DefaultRequestHeaders.Add("X-LINE-Authorization-Nonce", nonce);
                client.DefaultRequestHeaders.Add("X-LINE-Authorization", Convert.ToBase64String(hashHMAC));

                var stringContent = new StringContent(payload, Encoding.UTF8, "application/json");
                await client.PostAsync(baseUrl + requestUrl, stringContent);
            }

            return Ok();
        }

        private byte[] HashHMAC(string key, string data)
        {
            var hash = new HMACSHA256(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(key))));
            return hash.ComputeHash(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(data))));
        }
    }


    public class ConfirmInfo
    {
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}