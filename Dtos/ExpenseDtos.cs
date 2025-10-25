namespace Masarif.Api.Dtos;

public record ExpenseReadDto(
    int Id,
    string Title,
    string Category,
    decimal Amount,
    DateTime ExpenseDate,
    string? Notes
);

public record ExpenseCreateDto(
    string Title,
    string Category,
    decimal Amount,
    DateTime ExpenseDate,
    string? Notes
);

public record ExpenseUpdateDto(
    string Title,
    string Category,
    decimal Amount,
    DateTime ExpenseDate,
    string? Notes
);
