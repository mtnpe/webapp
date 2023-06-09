using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebApp.Helpers.Kafka;
using WebApp.Models.BL.CustomerBL;
using WebApp.Models.BL.WarehouseBL;
using WebApp.Models.BL.ProductBL;
using WebApp.Models.BL.OutputSlipBL;
using WebApp.Models.BL.OutputSlipDetailBL;
using WebApp.Models.BL.InputSlipBL;
using WebApp.Models.BL.InputSlipDetailBL;
using WebApp.Models.BL.UserBL;

namespace WebApp.Models.DL
{
    public class AppRepository
    {
        private readonly string _connectionString;

        public AppRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async void KafkaLog(string sqlQuery)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                MessageTimeoutMs = 10000
            };

            var kafkaProducer = new KafkaProducer(producerConfig, "sql-queries");
            var produceTask = kafkaProducer?.ProduceAsync(sqlQuery);

            if (produceTask != null && await Task.WhenAny(produceTask, Task.Delay(producerConfig.MessageTimeoutMs.Value)) == produceTask)
            {
                await produceTask;
            }
        }

        #region User

        public async Task<List<UserDTO>> UsersToListAsync()
        {
            var sqlQuery = "SELECT * FROM dbo.[User]";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<UserDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new UserDTO
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            Password = reader.GetString(2),
                            Role = reader.GetString(3),
                        });
                    }

                    KafkaLog(sqlQuery);

                    return result;
                }
            }
        }

        public async void UsersAdd(UserDTO userDTO)
        {
            var sqlQuery = $"INSERT INTO dbo.[User] (UserName, Password, Role) VALUES ('{userDTO.UserName}', '{userDTO.Password}', '{userDTO.Role}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void UsersDelete(UserDTO userDTO)
        {
            var sqlQuery = $"DELETE FROM dbo.[User] WHERE Id='{userDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void UsersUpdate(UserDTO userDTO)
        {
            var sqlQuery = $"UPDATE dbo.[User] SET UserName='{userDTO.UserName}', Password='{userDTO.Password}', Role='{userDTO.Role}' WHERE Id='{userDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion


        #region Customer

        public async Task<List<CustomerDTO>> CustomersToListAsync()
        {
            var sqlQuery = "SELECT * FROM Customer";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<CustomerDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new CustomerDTO
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            Address = reader.GetString(2),
                            Phone = reader.GetString(3),
                        });
                    }

                    KafkaLog(sqlQuery);

                    return result;
                }
            }
        }

        public async void CustomersAdd(CustomerDTO customerDTO)
        {
            var sqlQuery = $"INSERT INTO Customer (Id, Name, Address, Phone) VALUES ('{customerDTO.Id}', '{customerDTO.Name}', '{customerDTO.Address}', '{customerDTO.Phone}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void CustomersDelete(CustomerDTO customerDTO)
        {
            var sqlQuery = $"DELETE FROM Customer WHERE Id='{customerDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void CustomersUpdate(CustomerDTO customerDTO)
        {
            var sqlQuery = $"UPDATE Customer SET Name='{customerDTO.Name}', Address='{customerDTO.Address}', Phone='{customerDTO.Phone}' WHERE Id='{customerDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

        #region Warehouse

        public async Task<List<WarehouseDTO>> WarehousesToListAsync()
        {
            var sqlQuery = "SELECT * FROM Warehouse";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<WarehouseDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new WarehouseDTO
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            Address = reader.GetString(2),
                            NameKeeper = reader.GetString(3),
                        });
                    }

                    KafkaLog(sqlQuery);
                    return result;
                }
            }
        }

        public async void WarehousesAdd(WarehouseDTO warehouseDTO)
        {
            var sqlQuery = $"INSERT INTO Warehouse (Id, Name, Address, NameKeeper) VALUES ('{warehouseDTO.Id}', '{warehouseDTO.Name}', '{warehouseDTO.Address}', '{warehouseDTO.NameKeeper}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void WarehousesDelete(WarehouseDTO warehouseDTO)
        {
            var sqlQuery = $"DELETE FROM Warehouse WHERE Id='{warehouseDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void WarehousesUpdate(WarehouseDTO warehouseDTO)
        {
            var sqlQuery = $"UPDATE Warehouse SET Name='{warehouseDTO.Name}', Address='{warehouseDTO.Address}', NameKeeper='{warehouseDTO.NameKeeper}' WHERE Id='{warehouseDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

        #region Product

        public async Task<List<ProductDTO>> ProductsToListAsync()
        {
            var sqlQuery = "SELECT P.Id, P.Name, P.WarehouseId, P.Unit, W.Name AS Warehouse " +
                   "FROM Product AS P " +
                   "INNER JOIN Warehouse AS W ON P.WarehouseId = W.Id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<ProductDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new ProductDTO
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            WarehouseId = reader.GetString(2),
                            Unit = reader.GetString(3),
                            Warehouse = reader.GetString(4),
                        });
                    }
                    KafkaLog(sqlQuery);
                    return result;
                }
            }
        }

        public async void ProductsAdd(ProductDTO productDTO)
        {
            var sqlQuery = $"INSERT INTO Product (Id, Name, WarehouseId, Unit) VALUES ('{productDTO.Id}', '{productDTO.Name}', '{productDTO.WarehouseId}', '{productDTO.Unit}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void ProductsDelete(ProductDTO productDTO)
        {
            var sqlQuery = $"DELETE FROM Product WHERE Id='{productDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void ProductsUpdate(ProductDTO productDTO)
        {
            var sqlQuery = $"UPDATE Product SET Name='{productDTO.Name}', WarehouseId='{productDTO.WarehouseId}', Unit='{productDTO.Unit}' WHERE Id='{productDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

        #region InputSlip

        public async Task<List<InputSlipDTO>> InputSlipsToListAsync()
        {
            var sqlQuery = "SELECT OS.*, C.Name, W.Name " +
                           "FROM InputSlip OS " +
                           "INNER JOIN Customer C ON OS.CustomerId = C.Id " +
                           "INNER JOIN Warehouse W ON OS.WarehouseId = W.Id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<InputSlipDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new InputSlipDTO
                        {
                            Id = reader.GetString(0),
                            CustomerId = reader.GetString(1),
                            Date = reader.GetDateTime(2),
                            WarehouseId = reader.GetString(3),
                            Customer = reader.GetString(4),
                            Warehouse = reader.GetString(5)
                        });
                    }
                    KafkaLog(sqlQuery);
                    return result;
                }
            }
        }

        public async void InputSlipsAdd(InputSlipDTO inputSlipDTO)
        {
            var sqlQuery = $"INSERT INTO InputSlip (Id, CustomerId, Date, WarehouseId) VALUES ('{inputSlipDTO.Id}', '{inputSlipDTO.CustomerId}', '{inputSlipDTO.Date.ToString("yyyy-MM-dd HH:mm:ss")}', '{inputSlipDTO.WarehouseId}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void InputSlipsDelete(InputSlipDTO inputSlipDTO)
        {
            var sqlQuery = $"DELETE FROM InputSlip WHERE Id='{inputSlipDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void InputSlipsUpdate(InputSlipDTO inputSlipDTO)
        {
            var sqlQuery = $"UPDATE InputSlip SET CustomerId='{inputSlipDTO.CustomerId}', Date='{inputSlipDTO.Date.ToString("yyyy-MM-dd HH:mm:ss")}', WarehouseId='{inputSlipDTO.WarehouseId}' WHERE Id='{inputSlipDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

        #region InputSlipDetail

        public async Task<List<InputSlipDetailDTO>> InputSlipDetailsToListAsync()
        {
            var sqlQuery = "SELECT * FROM InputSlipDetail";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<InputSlipDetailDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new InputSlipDetailDTO
                        {
                            Id = reader.GetString(0),
                            InputSlipId = reader.GetString(1),
                            ProductId = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            Price = reader.GetDecimal(4),
                        });
                    }
                    KafkaLog(sqlQuery);
                    return result;
                }
            }
        }

        public async void InputSlipDetailsAdd(InputSlipDetailDTO inputSlipDetailDTO)
        {
            var sqlQuery = $"INSERT INTO InputSlipDetail (Id, InputSlipId, ProductId, Quantity, Price) VALUES ('{inputSlipDetailDTO.Id}', '{inputSlipDetailDTO.InputSlipId}', '{inputSlipDetailDTO.ProductId}', '{inputSlipDetailDTO.Quantity}', '{inputSlipDetailDTO.Price}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void InputSlipDetailsDelete(InputSlipDetailDTO inputSlipDetailDTO)
        {
            var sqlQuery = $"DELETE FROM InputSlipDetail WHERE Id='{inputSlipDetailDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void InputSlipsDetailsUpdate(InputSlipDetailDTO inputSlipDetailDTO)
        {
            var sqlQuery = $"UPDATE InputSlipDetail SET InputSlipId='{inputSlipDetailDTO.InputSlipId}', ProductId='{inputSlipDetailDTO.ProductId}', Quantity='{inputSlipDetailDTO.Quantity}, Price='{inputSlipDetailDTO.Price}' WHERE Id='{inputSlipDetailDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

        #region OutputSlip

        public async Task<List<OutputSlipDTO>> OutputSlipsToListAsync()
        {
            var sqlQuery = "SELECT OS.*, C.Name, W.Name " +
                           "FROM OutputSlip OS " +
                           "INNER JOIN Customer C ON OS.CustomerId = C.Id " +
                           "INNER JOIN Warehouse W ON OS.WarehouseId = W.Id";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<OutputSlipDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new OutputSlipDTO
                        {
                            Id = reader.GetString(0),
                            CustomerId = reader.GetString(1),
                            Date = reader.GetDateTime(2),
                            WarehouseId = reader.GetString(3),
                            Customer = reader.GetString(4),
                            Warehouse = reader.GetString(5)
                        });
                    }
                    KafkaLog(sqlQuery);
                    return result;
                }
            }
        }

        public async void OutputSlipsAdd(OutputSlipDTO outputSlipDTO)
        {
            var sqlQuery = $"INSERT INTO OutputSlip (Id, CustomerId, Date, WarehouseId) VALUES ('{outputSlipDTO.Id}', '{outputSlipDTO.CustomerId}', '{outputSlipDTO.Date.ToString("yyyy-MM-dd HH:mm:ss")}', '{outputSlipDTO.WarehouseId}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void OutputSlipsDelete(OutputSlipDTO outputSlipDTO)
        {
            var sqlQuery = $"DELETE FROM OutputSlip WHERE Id='{outputSlipDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void OutputSlipsUpdate(OutputSlipDTO outputSlipDTO)
        {
            var sqlQuery = $"UPDATE OutputSlip SET CustomerId='{outputSlipDTO.CustomerId}', Date='{outputSlipDTO.Date.ToString("yyyy-MM-dd HH:mm:ss")}', WarehouseId='{outputSlipDTO.WarehouseId}' WHERE Id='{outputSlipDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

        #region OutputSlipDetail

        public async Task<List<OutputSlipDetailDTO>> OutputSlipDetailsToListAsync()
        {
            var sqlQuery = "SELECT * FROM OutputSlipDetail";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sqlQuery, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<OutputSlipDetailDTO>();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new OutputSlipDetailDTO
                        {
                            Id = reader.GetString(0),
                            OutputSlipId = reader.GetString(1),
                            ProductId= reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            Price = reader.GetDecimal(4),
                        });
                    }
                    KafkaLog(sqlQuery);
                    return result;
                }
            }
        }

        public async void OutputSlipDetailsAdd(OutputSlipDetailDTO outputSlipDetailDTO)
        {
            var sqlQuery = $"INSERT INTO OutputSlipDetail (Id, OutputSlipId, ProductId, Quantity, Price) VALUES ('{outputSlipDetailDTO.Id}', '{outputSlipDetailDTO.OutputSlipId}', '{outputSlipDetailDTO.ProductId}', '{outputSlipDetailDTO.Quantity}', '{outputSlipDetailDTO.Price}')";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);

        }

        public async void OutputSlipDetailsDelete(OutputSlipDetailDTO outputSlipDetailDTO)
        {
            var sqlQuery = $"DELETE FROM OutputSlipDetail WHERE Id='{outputSlipDetailDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        public async void OutputSlipsDetailsUpdate(OutputSlipDetailDTO outputSlipDetailDTO)
        {
            var sqlQuery = $"UPDATE OutputSlipDetail SET OutputSlipId='{outputSlipDetailDTO.OutputSlipId}', ProductId='{outputSlipDetailDTO.ProductId}', Quantity='{outputSlipDetailDTO.Quantity}, Price='{outputSlipDetailDTO.Price}' WHERE Id='{outputSlipDetailDTO.Id}'";

            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sqlQuery, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            KafkaLog(sqlQuery);
        }

        #endregion

    }
}
