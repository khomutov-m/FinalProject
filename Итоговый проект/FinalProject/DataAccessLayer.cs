using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace FinalProject
{
    public static class DataAccessLayer
    {
        public static string DataSource { get; set; } = @"ISPP2114\SQLEXPRESS";
        public static string UserID { get; set; } = "ispp2114";
        public static string Password { get; set; } = "2114";
        public static string InitialCatalog { get; set; } = "ispp2114";
        public static string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
                {
                    DataSource = DataSource,
                    UserID = UserID,
                    Password = Password,
                    InitialCatalog = InitialCatalog,
                    TrustServerCertificate = true,
                };
                return builder.ConnectionString;
            }
        }

        public static int GetCountAllProducts()
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "SELECT COUNT(*) FROM ExamProduct";
            SqlCommand command = new(query, connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public static List<Product> GetProducts(string conditionByDiscountRange, string searchText, int sortIndex)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "SELECT * FROM ExamProduct";
            if (conditionByDiscountRange != "")
            {
                query += $" WHERE {conditionByDiscountRange}";
                if (!String.IsNullOrEmpty(searchText))
                    query += $" AND ProductName LIKE @searchText";
            }
            else
                query += $" WHERE ProductName LIKE @searchText";
            if (sortIndex == 0)
                query += $" ORDER BY ProductCost";
            else
                query += $" ORDER BY ProductCost DESC";
            SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@searchText", $"%{searchText}%");
            var reader = command.ExecuteReader();
            List<Product> products = new();
            while (reader.Read())
            {
                Product product = new Product()
                {
                    ProductArticleNumber = reader.GetString("ProductArticleNumber"),
                    ProductName = reader.GetString("ProductName"),
                    ProductDescription = reader.GetString("ProductDescription"),
                    ProductCost = Convert.ToDouble(reader.GetDecimal("ProductCost")),
                    ProductDiscountAmount = reader.GetByte("ProductDiscountAmount"),
                    ProductCategory = reader.GetString("ProductCategory"),
                    ProductManufacturer = reader.GetString("ProductManufacturer"),
                    ProductStatus = reader.GetString("ProductStatus"),
                    ProductQuantityInStock = reader.GetInt32("ProductQuantityInStock"),
                };
                    products.Add(product);
            }
            return products;
        }

        public static List<Order> GetOrders()
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "SELECT * FROM ExamOrder";
            SqlCommand command = new(query, connection);
            var reader = command.ExecuteReader();
            List<Order> orders = new();
            while (reader.Read())
            {
                Order order = new Order
                {
                    OrderId = Convert.ToInt32(reader.GetValue("OrderID")),
                    OrderStatus = reader.GetString("OrderStatus"),
                    OrderDate = reader.GetDateTime("OrderDate"),
                    OrderDeliveryDate = reader.GetDateTime("OrderDeliveryDate"),
                    OrderPickupPoint = Convert.ToInt32(reader.GetValue("OrderPickupPoint")),
                    OrderUserId = Convert.ToInt32(reader.GetValue("OrderUserId"))
                };
                orders.Add(order);
            }
            return orders;
        }

        public static void AddOrder(out Order order)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var identityQuery = "SELECT IDENT_CURRENT('ExamOrder')+1;";
            SqlCommand identityCommand = new(identityQuery, connection);
            order = new Order { OrderStatus = "Новый", OrderPickupPoint = 3 };
            order.OrderUserId = App.CurrentUser.UserId;
            order.OrderId = Convert.ToInt32(identityCommand.ExecuteScalar());
            order.OrderDate = DateTime.Now;
            order.OrderDeliveryDate = DateTime.Now.AddDays(1);
            var orderQuery = "AddOrder";
            SqlCommand orderCommand = new(orderQuery, connection);
            orderCommand.Parameters.AddWithValue("@orderStatus", order.OrderStatus);
            orderCommand.Parameters.AddWithValue("@orderDate", order.OrderDate);
            orderCommand.Parameters.AddWithValue("@orderDeliveryDate", order.OrderDeliveryDate);
            orderCommand.Parameters.AddWithValue("@orderPickupPoint", order.OrderPickupPoint);
            orderCommand.Parameters.AddWithValue("@orderUserId", order.OrderUserId);
            orderCommand.Parameters.AddWithValue("@orderPickupCode", order.ReceiptCode);
            orderCommand.CommandType = CommandType.StoredProcedure;
            orderCommand.ExecuteNonQuery();
        }

        public static void AddProductToOrder(Product product, Order order)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "AddProductToOrder";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@orderId", order.OrderId);
            command.Parameters.AddWithValue("@article", product.ProductArticleNumber);
            command.Parameters.AddWithValue("@amount", product.ProductAmount);
            command.ExecuteNonQuery();
        }

        public static void RemoveProductFromOrder(Product product, Order order)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "RemoveProductFromOrder";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@article", product.ProductArticleNumber);
            command.Parameters.AddWithValue("@orderId", order.OrderId);
            command.ExecuteNonQuery();
        }

        public static User GetUser(string login)
        {
            User user = null;
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "SELECT * FROM ExamUser WHERE UserLogin = @login";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@login", login);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                user = new User()
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    UserLogin = reader["UserLogin"].ToString(),
                    UserPassword = reader["UserPassword"].ToString(),
                    UserSurname = reader["UserSurname"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    UserPatronymic = reader["UserPatronymic"].ToString(),
                    UserRole = Convert.ToInt32(reader["UserRole"])
                };
            }
            return user;
        }

        public static double GetOrderTotalCost(int orderId)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "GetOrderTotalCost";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@orderId", orderId);
            var result = Convert.ToDouble(command.ExecuteScalar());
            return result;
        }

        public static int GetOrderTotalDiscount(int orderId)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "GetOrderTotalDiscount";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@orderId", orderId);
            var result = Convert.ToInt32(command.ExecuteScalar());
            return result;
        }

        public static string GetOrderList(int orderId)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "GetOrderList";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@orderId", orderId);
            var result = Convert.ToString(command.ExecuteScalar());
            return result;
        }

        public static string GetUserFullName(int orderId)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "SELECT CONCAT_WS(' ',UserSurname, UserName, UserPatronymic) FROM ExamUser WHERE UserID = @orderId";
            SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            return Convert.ToString(command.ExecuteScalar());

        }

        public static void UpdateOrder(Order order)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "UpdateOrder";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@orderId", order.OrderId);
            command.Parameters.AddWithValue("@orderStatus", order.OrderStatus);
            command.Parameters.AddWithValue("@orderPickupPoint", order.OrderPickupPoint);
            command.ExecuteNonQuery();
        }

        public static void DeleteProduct(Product product)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "DeleteProduct";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@article", product.ProductArticleNumber);
            command.ExecuteNonQuery();
        }

        public static void ChangeProduct(Product product)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "ChangeProduct";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            GetProductParameters(product, command);
            command.ExecuteNonQuery();
        }

        private static void GetProductParameters(Product product, SqlCommand command)
        {
            command.Parameters.AddWithValue("@article", product.ProductArticleNumber);
            command.Parameters.AddWithValue("@productName", product.ProductName);
            command.Parameters.AddWithValue("@productDescription", product.ProductDescription);
            command.Parameters.AddWithValue("@productCategory", product.ProductCategory);
            command.Parameters.AddWithValue("@productManufacturer", product.ProductManufacturer);
            command.Parameters.AddWithValue("@productCost", product.ProductCost);
            command.Parameters.AddWithValue("@productDiscountAmount", product.ProductDiscountAmount);
            command.Parameters.AddWithValue("@productQuantityInStock", product.ProductQuantityInStock);
            command.Parameters.AddWithValue("@productStatus", product.ProductStatus);
        }

        public static void AddProduct(Product product)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "AddProduct";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            GetProductParameters(product, command);
            command.ExecuteNonQuery();
        }

        public static List<PickupPoint> GetPickupPoints()
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "SELECT * FROM ExamPickupPoint";
            SqlCommand command = new(query, connection);
            var reader = command.ExecuteReader();
            List<PickupPoint> pickupPoints = new();
            while (reader.Read())
            {
                PickupPoint pickupPoint = new PickupPoint()
                {
                    PickupPointId = Convert.ToInt32(reader.GetInt16("PickupPointId")),
                    PickupPointAddress = reader.GetString("PickupPointAddress"),
                    PickupPointMailIndex = reader.GetInt32("PickupPointMailIndex"),
                };
                pickupPoints.Add(pickupPoint);
            }
            return pickupPoints;
        }
        public static void RemoveOrder(int orderId)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "DeleteOrder";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@orderId", orderId);
            command.ExecuteNonQuery();
        }
        public static void UpdateProduct(string productArticle, int productQuantity)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            var query = "UpdateProduct";
            SqlCommand command = new(query, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@productArticle", productArticle);
            command.Parameters.AddWithValue("@productQuantity", productQuantity);
            command.ExecuteNonQuery();
        }
    }
}
