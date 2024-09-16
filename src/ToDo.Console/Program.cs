using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using ToDo.API.Client;

namespace ToDo_Console
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            try
            {
                ClientOptions clientOptions = new ClientOptions
                {
                    ApiUrl = "https://localhost:44397",
                };

                using Client client = new Client(clientOptions);

                // Some validation testing
                try
                {
                    var getTest = await client.TodoItemsGetByIdAsync(1);
                }

                catch (ApiException ex)
                {
                    DisplayProblemDetails(ex);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                try
                {
                    var updateTest = await client.TodoItemsAddAsync(
                        new TodoItem { Name = "", IsComplete = false });
                }

                catch (ApiException ex)
                {
                    DisplayProblemDetails(ex);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                // Add
                var firstItem = await client.TodoItemsAddAsync(
                    new TodoItem { Name = "To do task 1", IsComplete = false });
                Console.WriteLine($"New Task 1 ID is: {firstItem.Id}");

                var secondItem = await client.TodoItemsAddAsync(
                    new TodoItem { Name = "To do task 2", IsComplete = false });
                Console.WriteLine($"New Task 2 ID is: {secondItem.Id}");

                Console.WriteLine();
                Console.WriteLine();

                // Update
                var firstItemToUpdate = await client.TodoItemsGetByIdAsync(firstItem.Id);
                Console.WriteLine($"GetByID for ID: {firstItemToUpdate.Id}");
                firstItemToUpdate.IsComplete = true;
                await client.TodoItemsUpdateAsync(firstItemToUpdate.Id, firstItemToUpdate);
                Console.WriteLine($"Updated IsComplete to true for ID: {firstItemToUpdate.Id}");

                var secondItemToUpdate = await client.TodoItemsGetByIdAsync(secondItem.Id);
                Console.WriteLine($"GetByID for ID: {secondItemToUpdate.Id}");
                secondItemToUpdate.IsComplete = true;
                await client.TodoItemsUpdateAsync(secondItemToUpdate.Id, secondItemToUpdate);
                Console.WriteLine($"Updated IsComplete to true for ID: {secondItemToUpdate.Id}");

                Console.WriteLine();
                Console.WriteLine();

                // GetAll
                Console.WriteLine("GetAll Begin");
                var items = await client.TodoItemsGetAllAsync();
                foreach (var item in items)
                {
                    Console.WriteLine($"Task Name: {item.Name}  Task ID: {item.Id}");
                }
                Console.WriteLine("GetAll End");

                Console.WriteLine();
                Console.WriteLine();

                // Delete
                var deleteItem = items.First();
                await client.TodoItemsDeleteAsync(deleteItem.Id);
                Console.WriteLine($"Deleted item ID: {deleteItem.Id}");

                Console.WriteLine();
                Console.WriteLine();

                // Bulk Updates Start
                ICollection<TodoItem> itemsBulkAdd = new List<TodoItem>();
                ICollection<TodoItem> itemsBulkUpdate = new List<TodoItem>();
                ICollection<TodoItem> itemsBulkDelete = new List<TodoItem>();

                var itemsBulkTest = await client.TodoItemsGetAllAsync();

                // Delete
                var deleteItemBulkTest = itemsBulkTest.First();
                itemsBulkDelete.Add(deleteItemBulkTest);
                await client.TodoItemsBulkDeleteAsync(itemsBulkDelete);
                Console.WriteLine($"Bulk Deleted ID: {deleteItemBulkTest.Id}");

                // Update
                var updateItemBulkTest = itemsBulkTest.Last();
                updateItemBulkTest.Name = "BULK UPDATE CHANGED";
                updateItemBulkTest.IsComplete = true;
                itemsBulkUpdate.Add(updateItemBulkTest);
                await client.TodoItemsBulkUpdateAsync(itemsBulkUpdate);
                Console.WriteLine($"Bulk Updated ID: {updateItemBulkTest.Id}");

                // Add 4 new ToDoItems
                itemsBulkAdd.Add(new TodoItem { Name = "BULK UPDATE ADD 1", IsComplete = false });
                itemsBulkAdd.Add(new TodoItem { Name = "BULK UPDATE ADD 2", IsComplete = false });
                itemsBulkAdd.Add(new TodoItem { Name = "BULK UPDATE ADD 3", IsComplete = false });
                itemsBulkAdd.Add(new TodoItem { Name = "BULK UPDATE ADD 4", IsComplete = false });
                await client.TodoItemsBulkAddAsync(itemsBulkAdd);
                Console.WriteLine("Bulk Added 4 new ToDoItems");

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        public static void DisplayProblemDetails(ApiException ex)
        {
            ValidationProblemDetails validationProblemDetails = GetValidationProblemDetails(ex);

            Console.WriteLine("Status Code");
            Console.WriteLine(validationProblemDetails.Status.ToString());
            Console.WriteLine();

            if (!string.IsNullOrEmpty(validationProblemDetails.Title))
            {
                Console.WriteLine(nameof(validationProblemDetails.Title));
                Console.WriteLine(validationProblemDetails.Title);
                Console.WriteLine();
            }

            if (!string.IsNullOrEmpty(validationProblemDetails.Detail))
            {
                Console.WriteLine(nameof(validationProblemDetails.Detail));
                Console.WriteLine(validationProblemDetails.Detail);
                Console.WriteLine();
            }
            if (validationProblemDetails.Errors != null)
            {
                Console.WriteLine(nameof(validationProblemDetails.Errors));
                foreach (var error in validationProblemDetails.Errors)
                {
                    foreach (var value in error.Value)
                    {
                        Console.WriteLine("  " + error.Key + ": " + value);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static ValidationProblemDetails GetValidationProblemDetails(ApiException ex)
        {
            ValidationProblemDetails validationProblemDetails;

            if (ex.StatusCode == 400)
            {
                validationProblemDetails = ((ApiException<ValidationProblemDetails>)ex).Result;
            }
            else
            {
                validationProblemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(ex.Response);
            }

            return validationProblemDetails; 
        }
    }
}
