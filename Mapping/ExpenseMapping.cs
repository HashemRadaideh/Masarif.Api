using Masarif.Api.Contracts;
using Masarif.Api.Models;

namespace Masarif.Api.Mapping;

public static class ExpenseMapping
{
    public static ExpenseReadDto ToReadDto(this Expense e) =>
        new(e.Id, e.Title, e.Category, e.Amount, e.ExpenseDate, e.Notes);

    public static Expense ToEntity(this ExpenseCreateDto d) => new()
    {
        Title = d.Title, Category = d.Category, Amount = d.Amount,
        ExpenseDate = d.ExpenseDate, Notes = d.Notes
    };

    public static void Apply(this Expense e, ExpenseUpdateDto d)
    {
        e.Title = d.Title;
        e.Category = d.Category;
        e.Amount = d.Amount;
        e.ExpenseDate = d.ExpenseDate;
        e.Notes = d.Notes;
    }
}

