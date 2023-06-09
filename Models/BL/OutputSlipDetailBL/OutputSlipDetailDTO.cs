using System;

namespace WebApp.Models.BL.OutputSlipDetailBL
{
    public class OutputSlipDetailDTO : BaseDetailDTO
    {
        public OutputSlipDetailDTO() { }
        public OutputSlipDetailDTO(string id, string outputslipid, string productid, string nameproduct, int quantity, decimal price)
        {
            this.Id = id;
            this.OutputSlipId = outputslipid;
            this.ProductId = productid;
            this.Quantity = quantity;
            this.Price = price;
            this.NameProduct = nameproduct;
        }
        public string OutputSlipId { get; set; }

    }
}