using System.ComponentModel.DataAnnotations;

namespace ToDo.Models
{
    public class TodoItem
    {
        /// <summary>
        /// The id that uniquely identifies the ToToItem. Database generated.
        /// </summary>
        /// <example>1</example>        
        public long Id { get; set; }

        /// <summary>
        /// A description of the task to be completed
        /// </summary>
        /// <example>"Take out the garbage"</example> 
        public string Name { get; set; }

        /// <summary>
        /// Is the todo item complete
        /// </summary>
        /// <example>false</example>        
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// The property RowVersion is used to perform database concurrency checks and should never be changed
        /// </summary>
        /// <example>"AAAAAAAAF3I="</example>        
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
