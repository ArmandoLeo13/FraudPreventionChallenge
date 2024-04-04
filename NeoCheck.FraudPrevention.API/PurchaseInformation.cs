using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCheck.FraudPrevention.API
{
    public class PurchaseInformationRequest
    {
        public List<PurchaseInformation> Purchases { get; set; }
    }
    public class PurchaseInformation
    {
        public long OrderId { get; set; }
        public long DealId { get; set; }
        public string EmailAddress { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public long CreditCardNumber { get; set; }
    }
}
