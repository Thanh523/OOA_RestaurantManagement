using RestaurantManagement.DAL;
using RestaurantManagement.DTO;

namespace RestaurantManagement.BUS;

public sealed class DiningTableService
{
    public static IReadOnlyList<string> AvailableStatuses { get; } =
    [
        "Trống",
        "Đã đặt",
        "Đang phục vụ",
        "Đang dọn"
    ];

    private readonly DiningTableRepository _repository;

    public DiningTableService(DiningTableRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<DiningTable>> GetAllAsync(
        string? code = null,
        string? area = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(code, area, status, cancellationToken);

    public Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default) =>
        _repository.GetStatusCountsAsync(cancellationToken);

    public Task<DiningTable?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        _repository.GetByCodeAsync(code.Trim(), cancellationToken);

    public async Task AddAsync(DiningTable table, CancellationToken cancellationToken = default)
    {
        table = NormalizeAndValidate(table);

        if (await _repository.GetByCodeAsync(table.Code, cancellationToken) is not null)
        {
            throw new ArgumentException($"Mã bàn '{table.Code}' đã tồn tại.");
        }

        await _repository.AddAsync(table, cancellationToken);
    }

    public async Task<bool> UpdateAsync(DiningTable table, CancellationToken cancellationToken = default)
    {
        table = NormalizeAndValidate(table);
        return await _repository.UpdateAsync(table, cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(
        string code,
        string status,
        CancellationToken cancellationToken = default)
    {
        status = status.Trim();
        ValidateStatus(status);
        return await _repository.UpdateStatusAsync(code.Trim(), status, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        code = code.Trim();
        if (await _repository.DeleteAsync(code, cancellationToken))
        {
            return true;
        }

        var existingTable = await _repository.GetByCodeAsync(code, cancellationToken);
        if (existingTable is not null && existingTable.Status != AvailableStatuses[0])
        {
            throw new InvalidOperationException(
                "Chỉ xóa được bàn đang ở trạng thái Trống. Hãy kiểm tra trạng thái hoặc dữ liệu đang liên kết.");
        }

        return false;
    }

    private static DiningTable NormalizeAndValidate(DiningTable table)
    {
        var code = table.Code.Trim();
        var area = table.Area.Trim();
        var status = table.Status.Trim();

        if (code.Length == 0 || code.Length > 20)
        {
            throw new ArgumentException("Mã bàn phải có từ 1 đến 20 ký tự.");
        }

        if (area.Length == 0 || area.Length > 50)
        {
            throw new ArgumentException("Khu vực phải có từ 1 đến 50 ký tự.");
        }

        if (table.Capacity <= 0)
        {
            throw new ArgumentException("Sức chứa phải lớn hơn 0.");
        }

        ValidateStatus(status);
        return new DiningTable(code, area, table.Capacity, status);
    }

    private static void ValidateStatus(string status)
    {
        if (!AvailableStatuses.Contains(status, StringComparer.Ordinal))
        {
            throw new ArgumentException("Trạng thái bàn không hợp lệ.");
        }
    }
}
