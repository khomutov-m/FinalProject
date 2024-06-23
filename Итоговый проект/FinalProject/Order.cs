using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDeliveryDate { get; set; }
        public int OrderPickupPoint { get; set; }
        public int OrderUserId { get; set; }
        public int ReceiptCode { get; set; } = (new Random()).Next(100, 1000);
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public double OrderTotalCost { get => DataAccessLayer.GetOrderTotalCost(OrderId); set { } }
        public int OrderTotalDiscount { get => DataAccessLayer.GetOrderTotalDiscount(OrderId); set { } }
        public string OrderList { get => DataAccessLayer.GetOrderList(OrderId); set { } }
        public string OrderUserFullName { get => DataAccessLayer.GetUserFullName(OrderId); set { } }
    }
}
