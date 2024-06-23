namespace FinalProject
{
    public class Product
    {
        public string ProductArticleNumber { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductCategory { get; set; }
        public double ProductCostWithDiscount
        {
            get
            {
                if (ProductDiscountAmount > 0)
                    return ProductCost * (1 - ProductDiscountAmount * 0.01)*ProductAmount;
                return ProductCost;
            }
            set { }
        }
        public double ProductCost { get; set; }
        public string ProductManufacturer { get; set; }

        public string ProductStatus { get; set; }
        public int ProductQuantityInStock { get; set; }
        public int ProductDiscountAmount { get; set; }
        public string Color
        {
            get
            {
                return ProductDiscountAmount >= 15 ? ProductQuantityInStock == 0? "Gray": "#7fff00" : 
                    ProductQuantityInStock == 0 ? "Gray" : "White";
            }
            set { }
        }
        public string ProductCostConverter
        {
            get
            {
                if (ProductCost == ProductCostWithDiscount)
                    return "";
                return ProductCost.ToString();
            }
            set { }
        }
        public int ProductAmount { get; set; } = 1;
    }
}
