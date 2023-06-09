using System;

namespace WebApp.Models.BL
{
    public class BaseSlipDTO : IComparable
    {
        public BaseSlipDTO() { }
        public BaseSlipDTO(string id, string customerid, string customer, DateTime date, string warehouseid, string warehouse)
        {
            this.Id = id;
            this.CustomerId = customerid;
            this.Customer = customer;
            this.Date = date;
            this.WarehouseId = warehouseid;
            this.Warehouse = warehouse;
        }
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public DateTime Date { get; set; }
        public string WarehouseId { get; set; }
        public string Customer { get; set; }
        public string Warehouse { get; set; }

        public int CompareTo(object obj)
        {
            BaseSlipDTO slipDTO = obj as BaseSlipDTO;
            return this.Date.CompareTo(slipDTO.Date);
        }
    }
}