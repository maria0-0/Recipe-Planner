using System.Text.Json;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

public static class RecipeFile
{
    private static readonly JsonSerializerOptions Options = RecipeMapper.JsonOptions;

    public static readonly string DefaultPath = Path.Combine(AppContext.BaseDirectory, "recipes.json");
    public static readonly string DefaultMealPlanPath = Path.Combine(AppContext.BaseDirectory, "meal-plan.json");
    public static readonly string DefaultFridgePath = Path.Combine(AppContext.BaseDirectory, "fridge.json");

    public static Task SaveAsync(IItemStore store) => SaveAsync(store, DefaultPath);
    public static Task<IItemStore> LoadAsync() => LoadAsync(DefaultPath);

    public static async Task SaveAsync(IItemStore store, string path)
    {
        ArgumentNullException.ThrowIfNull(store);
        var rows = store.All().Select(RecipeMapper.ToDto).ToList();
        await WriteJsonAtomicAsync(rows, path);
    }

    public static async Task<IItemStore> LoadAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return new RecipeStore();
        }

        var rows = await ReadJsonAsync<List<RecipeDto>>(path);
        if (rows is null)
        {
            throw new InvalidDataException("The file contains no data or could not be deserialized.");
        }

        var store = new RecipeStore();
        foreach (var dto in rows)
        {
            var item = RecipeMapper.ToItem(dto)
                ?? throw new InvalidDataException("The file contains items with missing required fields or invalid shape.");
            store.Add(item);
        }

        return store;
    }

    public static Task SaveMealPlanAsync(MealPlan plan) => SaveMealPlanAsync(plan, DefaultMealPlanPath);
    public static Task<MealPlan> LoadMealPlanAsync() => LoadMealPlanAsync(DefaultMealPlanPath);

    public static async Task SaveMealPlanAsync(MealPlan plan, string path)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var rows = plan.Meals.Select(RecipeMapper.ToDto).ToList();
        await WriteJsonAtomicAsync(rows, path);
    }

    public static async Task<MealPlan> LoadMealPlanAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return new MealPlan();
        }

        var rows = await ReadJsonAsync<List<RecipeDto>>(path);
        if (rows is null)
        {
            throw new InvalidDataException("The meal plan file contains no data.");
        }

        var plan = new MealPlan();
        foreach (var dto in rows)
        {
            if (RecipeMapper.ToItem(dto) is Recipe r)
            {
                plan.AddRecipe(r);
            }
        }
        return plan;
    }

    public static Task SaveFridgeAsync(IEnumerable<string> items) => SaveFridgeAsync(items, DefaultFridgePath);
    public static Task<List<string>> LoadFridgeAsync() => LoadFridgeAsync(DefaultFridgePath);

    public static async Task SaveFridgeAsync(IEnumerable<string> items, string path)
    {
        ArgumentNullException.ThrowIfNull(items);
        await WriteJsonAtomicAsync(items.ToList(), path);
    }

    public static async Task<List<string>> LoadFridgeAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return [];
        }

        var items = await ReadJsonAsync<List<string>>(path);
        return items ?? [];
    }

    public static async Task SaveCookingHistoryAsync(IEnumerable<CookingHistoryEntry> history, string path)
    {
        ArgumentNullException.ThrowIfNull(history);
        await WriteJsonAtomicAsync(history.ToList(), path);
    }

    public static async Task<List<CookingHistoryEntry>> LoadCookingHistoryAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return [];
        }

        var history = await ReadJsonAsync<List<CookingHistoryEntry>>(path);
        return history ?? [];
    }

    public static async Task SaveFavoritesAsync(IEnumerable<Recipe> favorites, string path)
    {
        ArgumentNullException.ThrowIfNull(favorites);
        var rows = favorites.Select(RecipeMapper.ToDto).ToList();
        await WriteJsonAtomicAsync(rows, path);
    }

    public static async Task<List<Recipe>> LoadFavoritesAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return [];
        }

        var rows = await ReadJsonAsync<List<RecipeDto>>(path);
        if (rows is null)
        {
            return [];
        }

        var list = new List<Recipe>();
        foreach (var dto in rows)
        {
            if (RecipeMapper.ToItem(dto) is Recipe r)
            {
                r.IsFavorite = true;
                list.Add(r);
            }
        }

        return list;
    }

    private static async Task WriteJsonAtomicAsync<T>(T data, string path)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        if (Path.GetDirectoryName(fullPath) is { Length: > 0 } folder)
        {
            Directory.CreateDirectory(folder);
        }

        var tempPath = Path.Combine(
            Path.GetDirectoryName(fullPath)!,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, data, Options);
            }

            File.Move(tempPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }

    private static async Task<T?> ReadJsonAsync<T>(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The file is corrupt, invalid JSON, or not in the expected shape.", ex);
        }
    }
}
