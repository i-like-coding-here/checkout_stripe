using Newtonsoft.Json;

namespace CheckoutSession.core.Models.Dtos.Requests
{
    public class CreateCustomerRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tenant_Id")]
        public Guid tenant_Id { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("address")]
        public AddressRequest Address { get; set; }
    }
}
