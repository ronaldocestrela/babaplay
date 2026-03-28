using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Financial.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; } = CategoryType.Income;
}

public enum CategoryType
{
    Income = 0,
    Expense = 1
}
