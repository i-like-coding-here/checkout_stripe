using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutSession.core.Models.Dtos.Responses
{
    public class CheckoutSessionResponse
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        //public string Url { get; set; }
        public string PaymentStatus { get; set; }
    }

}
