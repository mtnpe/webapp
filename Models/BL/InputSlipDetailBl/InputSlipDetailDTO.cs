using System;

namespace WebApp.Models.BL.InputSlipDetailBL
{
    public class InputSlipDetailDTO : BaseDetailDTO
    {
        public InputSlipDetailDTO() { }
        public InputSlipDetailDTO(string id, string inputslipid, string productid, string nameproduct, int quantity, decimal price)
        {
            this.Id = id;
            this.InputSlipId = inputslipid;
            this.ProductId = productid;
            this.Quantity = quantity;
            this.Price = price;
            this.NameProduct = nameproduct;
        }
        public string InputSlipId { get; set; }

    }
}