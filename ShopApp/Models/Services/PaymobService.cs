using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ShopApp.Services
{
    public class PaymobService
    {
        private readonly IConfiguration _config;
        // يستخدم لارسال الطلبات الي الانترنت
        private readonly HttpClient _http;

        public PaymobService(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient();
        }

        //  http وتعيده داخل stringcontent حتي يتم ارسال طلب  json تحويل اي كائن الي    
          private StringContent CreateContent(object obj)
        {     // بشفر ال object to json 
            var json = JsonSerializer.Serialize(obj);       // معناه انا ارسل بيانات json
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // json to object تحويل ال 
        private JsonDocument JsonDoc(string json) =>
            JsonDocument.Parse(json);
      
        // انا كده بسجل نفسي عند خدمه الدفع  ويجيب التوكين
        public async Task<string> GetAuthTokenAsync()
        {
            // المفتاح الخاص بي من حساب paymob
            var req = new { api_key = _config["PayMob:ApiKey"] };
            // ارسل طلب ل  paymob  jsonمع المفتاح بصيغه  
            var res = await _http.PostAsync("https://accept.paymob.com/api/auth/tokens", CreateContent(req));
        
            var json = await res.Content.ReadAsStringAsync();
            return JsonDoc(json).RootElement.GetProperty("token").GetString();
        }
          // انشاء طلب للسيرفر اني اجيب رقم الاردر
        public async Task<int> CreateOrderAsync(string token, decimal amountCents)
        {
            var req = new
            {
                auth_token = token,
                delivery_needed = false,
                amount_cents = (int)(amountCents),
                currency = "EGP",
                items = new object[] { }
            };

            var res = await _http.PostAsync("https://accept.paymob.com/api/ecommerce/orders", CreateContent(req));
            var json = await res.Content.ReadAsStringAsync();
            return JsonDoc(json).RootElement.GetProperty("id").GetInt32();
        }
            // الحصول علي مفتاح الدفع
        public async Task<string> GetPaymentKeyAsync(string token, int orderId, decimal amountCents, string name, string email, string phone)
        {
            var req = new
            {
                auth_token = token,
                amount_cents = (int)(amountCents),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    first_name = name,
                    last_name = name,
                    email = email ?? "test@example.com",
                    phone_number = phone,
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    city = "Cairo",
                    country = "EG",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = int.Parse(_config["PayMob:IntegrationId"]),
               
                callback_url = $"{_config["Domain"]}Order/OrderConfirmationSuccses"

            };

            var res = await _http.PostAsync("https://accept.paymob.com/api/acceptance/payment_keys", CreateContent(req));
            var json = await res.Content.ReadAsStringAsync();
            return JsonDoc(json).RootElement.GetProperty("token").GetString();
        }

    }

}
