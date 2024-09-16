using FluentValidation;

namespace ToDo.Models.Validators
{
    public class TodoItemValidator : AbstractValidator<TodoItem> 
    {
		public TodoItemValidator()
		{
			RuleFor(x => x.Id).NotNull();
			RuleFor(x => x.Name).NotEmpty().WithMessage("Please specify a task name");
			RuleFor(x => x.Name).Length(0, 50).WithMessage("Please enter a task name no longer than 50 characters");
		}
	}
}
