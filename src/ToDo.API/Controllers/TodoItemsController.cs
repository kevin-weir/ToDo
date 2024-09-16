using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using ToDo.Models;
using ToDo.API.Helpers;
using NSwag.Annotations;

namespace ToDo.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoItemsController : ControllerBase
    {
        public TodoItemsController(TodoContext context, ILogger<TodoItemsController> logger, IDistributedCache cache)
        {
            this.context = context;
            this.logger = logger;
            this.cache = cache;
        }

        private readonly TodoContext context;
        private readonly ILogger<TodoItemsController> logger;

        private readonly IDistributedCache cache;
        private const string CacheKey = nameof(TodoItemsController) + nameof(TodoItemsGetAll) + nameof(TodoItem);

        /// <summary>
        /// Returns a list of all ToDoItems
        /// </summary>
        /// <returns>Returns a list of all ToDoItems</returns>
        /// <response code="200">Success</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsGetAll))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> TodoItemsGetAll()
        {
            List<TodoItem> result;

            var cachedResult = await cache.GetAsync(CacheKey);
            if (cachedResult != null)
            {
                result = BinarySerialization<List<TodoItem>>.ByteArrayToObject(cachedResult);
            }
            else
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(4)
                };

                result = await context.TodoItems.AsNoTracking().ToListAsync();
                await cache.SetAsync(CacheKey, BinarySerialization<List<TodoItem>>.ObjectToByteArray(result), options);
            }

            return result;
        }

        /// <summary>
        /// Gets a specific ToDoItem
        /// </summary>
        /// <param name="id">The id that uniquely identifies the ToDoItem</param>
        /// <returns>Returns a specific ToDoItem</returns>
        /// <response code="200">Success</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsGetById))]
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> TodoItemsGetById(long id)
        {
            var todoItem = await context.TodoItems.FindAsync(id);
            if (todoItem == null) {return NotFound();}

            return todoItem;
        }

        /// <summary>
        /// Creates a TodoItem
        /// </summary>
        /// <returns>A newly created TodoItem</returns>
        /// <response code="201">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsAdd))]
        [HttpPost]
        public async Task<ActionResult<TodoItem>> TodoItemsAdd(TodoItem todoItem)
        {
            var modelErrors = new ModelStateDictionary();

            if (todoItem.Id > 0)
            {
                modelErrors.AddModelError("Id", "Do not specify value for ID. ID is database generated.");
            }

            if (todoItem.RowVersion != null)
            {
                modelErrors.AddModelError("RowVersion", "Do not specify a value for RowVersion when creating a TodoItem. " +
                    "RowVersion is database generated and used to perform concurrency checks.");
            }

            if (modelErrors.Count != 0)
            {
                return ValidationProblem(modelErrors);
            }

            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            // Remove the cached results using cacheKey
            await cache.RemoveAsync(CacheKey);

            return CreatedAtAction(nameof(TodoItemsGetById), new { id = todoItem.Id }, todoItem);
        }

        /// <summary>
        /// Updates a specific ToDoItem
        /// </summary>
        /// <param name="id">The id that uniquely identifies the ToDoItem</param>
        /// <param name="todoItem"></param>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsUpdate))]
        [HttpPut("{id}")]
        public async Task<ActionResult> TodoItemsUpdate(long id, TodoItem todoItem)
        {
            try
            {
                if (id != todoItem.Id) 
                {
                    var modelErrors = new ModelStateDictionary();
                    modelErrors.AddModelError("Id", "The Parameter ID and the ID from the body do not match.");
                    return ValidationProblem(modelErrors);
                }

                context.Entry(todoItem).State = EntityState.Modified;
                await context.SaveChangesAsync();

                // Remove the cached results using cacheKey
                await cache.RemoveAsync(CacheKey);
            }

            catch (DbUpdateConcurrencyException)
            {
                if (TodoItemExists(id))
                { 
                    return Conflict();
                }
                else
                {
                    return NotFound();
                }
            }

            return Ok();
        }

        /// <summary>
        /// Deletes a specific TodoItem
        /// </summary>
        /// <param name="id">The id that uniquely identifies the ToDoItem</param>
        /// <returns>Returns the deleted ToDoItem</returns>
        /// <response code="200">Success</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsDelete))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> TodoItemsDelete(long id)
        {
            var todoItem = await context.TodoItems.FindAsync(id);
            if (todoItem == null) {return NotFound();}

            context.TodoItems.Remove(todoItem);
            await context.SaveChangesAsync();

            // Remove the cached results using cacheKey
            await cache.RemoveAsync(CacheKey);

            return Ok();
        }

        /// <summary>
        /// Bulk Add TodoItems
        /// </summary>
        /// <returns>Returns Success (200) if successful</returns>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsBulkAdd))]
        [HttpPost(nameof(TodoItemsBulkAdd))]
        public async Task<ActionResult> TodoItemsBulkAdd(ICollection<TodoItem> todoItems)
        {
            foreach (var item in todoItems)
            {
                context.TodoItems.Add(item);
            }

            await context.SaveChangesAsync();

            // Remove the cached results using cacheKey
            await cache.RemoveAsync(CacheKey);

            return Ok();
        }

        /// <summary>
        /// Bulk Update TodoItems
        /// </summary>
        /// <returns>Returns Success (200) if successful</returns>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsBulkUpdate))]
        [HttpPost(nameof(TodoItemsBulkUpdate))]
        public async Task<ActionResult> TodoItemsBulkUpdate(ICollection<TodoItem> todoItems)
        {
            foreach (var item in todoItems)
            {
                context.TodoItems.Update(item);
            }

            await context.SaveChangesAsync();

            // Remove the cached results using cacheKey
            await cache.RemoveAsync(CacheKey);

            return Ok();
        }

        /// <summary>
        /// Bulk Delete TodoItems
        /// </summary>
        /// <returns>Returns Success (200) if successful</returns>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Server Error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [OpenApiOperation(nameof(TodoItemsBulkDelete))]
        [HttpPost(nameof(TodoItemsBulkDelete))]
        public async Task<ActionResult> TodoItemsBulkDelete(ICollection<TodoItem> todoItems)
        {
            foreach (var item in todoItems)
            {
                context.TodoItems.Remove(item);
            }

            await context.SaveChangesAsync();

            // Remove the cached results using cacheKey
            await cache.RemoveAsync(CacheKey);

            return Ok();
        }

        private bool TodoItemExists(long id)
        {
            return context.TodoItems.Any(e => e.Id == id);
        }
    }
}
