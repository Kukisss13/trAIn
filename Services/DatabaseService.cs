using SQLite;
using TreninkovyPlanovac.Models;

namespace TreninkovyPlanovac.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetConnection()
    {
        if (_db != null)
            return _db;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "treninky.db");
        _db = new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<Cviceni>();
        await _db.CreateTableAsync<TreninkovyPlan>();
        await _db.CreateTableAsync<PolozkaPlanu>();
        await _db.CreateTableAsync<Uzivatel>();
        await _db.CreateTableAsync<HistorieTreninku>();
        await _db.CreateTableAsync<ChatZprava>();

        return _db;
    }

    // Cvičení
    public async Task<List<Cviceni>> GetCviceniAsync()
    {
        var db = await GetConnection();
        return await db.Table<Cviceni>().ToListAsync();
    }

    public async Task<Cviceni> GetCviceniByIdAsync(int id)
    {
        var db = await GetConnection();
        return await db.Table<Cviceni>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> UlozCviceniAsync(Cviceni cviceni)
    {
        var db = await GetConnection();
        if (cviceni.Id != 0)
            return await db.UpdateAsync(cviceni);
        return await db.InsertAsync(cviceni);
    }

    public async Task<int> SmazCviceniAsync(Cviceni cviceni)
    {
        var db = await GetConnection();
        return await db.DeleteAsync(cviceni);
    }

    // Tréninkové plány
    public async Task<List<TreninkovyPlan>> GetPlanyAsync()
    {
        var db = await GetConnection();
        return await db.Table<TreninkovyPlan>().OrderByDescending(p => p.Datum).ToListAsync();
    }

    public async Task<int> UlozPlanAsync(TreninkovyPlan plan)
    {
        var db = await GetConnection();
        if (plan.Id != 0)
            return await db.UpdateAsync(plan);
        return await db.InsertAsync(plan);
    }

    public async Task<TreninkovyPlan?> GetPlanAsync(int id)
    {
        var db = await GetConnection();
        return await db.Table<TreninkovyPlan>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SmazPlanAsync(TreninkovyPlan plan)
    {
        var db = await GetConnection();
        return await db.DeleteAsync(plan);
    }

    // Položky plánu
    public async Task<List<PolozkaPlanu>> GetPolozkyPlanuAsync(int planId)
    {
        var db = await GetConnection();
        return await db.Table<PolozkaPlanu>()
            .Where(p => p.TreninkovyPlanId == planId)
            .OrderBy(p => p.Poradi)
            .ToListAsync();
    }

    public async Task<int> UlozPolozkuAsync(PolozkaPlanu polozka)
    {
        var db = await GetConnection();
        if (polozka.Id != 0)
            return await db.UpdateAsync(polozka);
        return await db.InsertAsync(polozka);
    }

    public async Task<int> SmazPolozkuAsync(PolozkaPlanu polozka)
    {
        var db = await GetConnection();
        return await db.DeleteAsync(polozka);
    }

    public async Task SmazPolozkyPlanuAsync(int planId)
    {
        var db = await GetConnection();
        await db.ExecuteAsync("DELETE FROM PolozkaPlanu WHERE TreninkovyPlanId = ?", planId);
    }

    public async Task<TreninkovyPlan?> GetPlanByIdAsync(int id)
    {
        var db = await GetConnection();
        return await db.Table<TreninkovyPlan>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    // Uživatel
    public async Task<Uzivatel?> GetPrihlasenyUzivatelAsync()
    {
        var db = await GetConnection();
        return await db.Table<Uzivatel>().FirstOrDefaultAsync();
    }

    public async Task<int> UlozUzivateleAsync(Uzivatel uzivatel)
    {
        var db = await GetConnection();
        if (uzivatel.Id != 0)
            return await db.UpdateAsync(uzivatel);
        return await db.InsertAsync(uzivatel);
    }

    public async Task OdhlasUzivateleAsync()
    {
        var db = await GetConnection();
        await db.DeleteAllAsync<Uzivatel>();
    }

    public async Task<Uzivatel> GetNeboVytvorProfilAsync()
    {
        var db = await GetConnection();
        var uzivatel = await db.Table<Uzivatel>().FirstOrDefaultAsync();
        if (uzivatel == null)
        {
            uzivatel = new Uzivatel();
            await db.InsertAsync(uzivatel);
        }
        return uzivatel;
    }

    // Historie tréninků
    public async Task<int> UlozHistoriiAsync(HistorieTreninku historie)
    {
        var db = await GetConnection();
        if (historie.Id != 0)
            return await db.UpdateAsync(historie);
        return await db.InsertAsync(historie);
    }

    public async Task<List<HistorieTreninku>> GetHistoriiAsync()
    {
        var db = await GetConnection();
        return await db.Table<HistorieTreninku>().OrderByDescending(h => h.DatumCviceni).ToListAsync();
    }

    // Chat
    public async Task<List<ChatZprava>> GetChatHistoriiAsync()
    {
        var db = await GetConnection();
        return await db.Table<ChatZprava>().OrderBy(z => z.Cas).ToListAsync();
    }

    public async Task<int> UlozChatZpravuAsync(ChatZprava zprava)
    {
        var db = await GetConnection();
        return await db.InsertAsync(zprava);
    }

    public async Task SmazChatHistoriiAsync()
    {
        var db = await GetConnection();
        await db.DeleteAllAsync<ChatZprava>();
    }
}
