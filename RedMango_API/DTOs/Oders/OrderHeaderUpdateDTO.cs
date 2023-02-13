using RedMango_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RedMango_API.DTOs.Oders
{
    public class OrderHeaderUpdateDTO
    {
        public int OrderHeaderId { get; set; }
         public string PickupName { get; set; }
        public string PickupPhoneNumber { get; set; }
        public string PickupEmail { get; set; }


         public string StripePaymentIntentID { get; set; }
        public string Status { get; set; }
    }
}
