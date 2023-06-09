
namespace WebApp.Models.BL.ProductBL
{
    public class ProductDTO
    {
        public ProductDTO() 
        {

        }

        public ProductDTO(string id, string name, string warehouseid, string unit, string warehouse)
        {
            this.Id = id;
            this.Name = name;
            this.WarehouseId = warehouseid;
            this.Unit = unit;
            this.Warehouse = warehouse;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string WarehouseId { get; set; }
        public string Warehouse { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }

    }
}