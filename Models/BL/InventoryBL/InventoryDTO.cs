
using System;

namespace WebApp.Models.BL.InventoryBL
{
    public class InventoryDTO
    {
        public InventoryDTO() { }
        public InventoryDTO(string warehouseId, DateTime dateTimeStart, DateTime dateTimeEnd)
        {
            this.WarehouseId = warehouseId;
            this.DateTimeStart = dateTimeStart;
            this.DateTimeEnd = dateTimeEnd;
        }
        public string WarehouseId { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }

    }
}