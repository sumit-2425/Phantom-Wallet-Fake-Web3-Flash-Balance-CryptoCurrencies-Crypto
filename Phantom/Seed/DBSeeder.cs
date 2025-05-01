using System.Text.Json;
using Backend.Models;
using Backend.Parameters;

namespace Backend.Seeds;
public class DbSeeder
{
    // Use in processing user.json
    private static Dictionary<string, Guid> pharmacyNameToId = new();
    private static Dictionary<Guid, Dictionary<string, Guid>> maskNameToIdFromPharmacy = new();
    private static Dictionary<string, Guid> maskTypeToId = new();

    public static async Task SeedAsync(DataContext context, IWebHostEnvironment env)
    {
        // Process pharmacies.json
        string pharmaciesJsonPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "pharmacies.json"));
        await ProcessPharmaciesJson(pharmaciesJsonPath, context);

        // Process users.json
        string usersJsonPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "users.json"));
        await ProcessUsersJson(usersJsonPath, context);
    }

    private static async Task ProcessPharmaciesJson(string filePath, DataContext context)
    {
        var pharmaciesJson = (List<PharmacyJson>)ReadJsonFile(filePath);

        // Seed the database based on pharmacies.json
        foreach (var pharmacyJson in pharmaciesJson)
        {
            // Create and add pharmacy
            var pharmacy = await SavePharmacyAndOpeningHours(pharmacyJson, context);

            // Process and add masks and mask types
            foreach (var maskJson in pharmacyJson.Masks)
            {
                await SaveMaskAndMaskType(context, maskJson, pharmacy);
            }
        }
    }

    /// <summary>
    /// Saves the pharmacy and its opening hours to the database.
    /// </summary>
    /// <param name="pharmacyJson">The pharmacy JSON object.</param>
    /// <param name="context">The data context.</param>
    /// <returns>The saved pharmacy object.</returns>
    private static async Task<Pharmacy> SavePharmacyAndOpeningHours(PharmacyJson pharmacyJson, DataContext context)
    {
        var pharmacy = new Pharmacy()
        {
            Name = pharmacyJson.Name,
            CashBalance = pharmacyJson.CashBalance,
        };

        // Process and add opening hours
        var openingHours = ProcessOpeningHoursString(pharmacyJson.OpeningHours);
        Console.WriteLine($"1. {pharmacy.Id}");
        foreach (var (week, startTime, endTime) in openingHours)
        {
            pharmacy.OpeningHours.Add(new OpeningHour()
            {
                Week = (ushort)week,
                OpenTime = startTime,
                CloseTime = endTime,
            });
        }

        context.Pharmacies.Add(pharmacy);
        await context.SaveChangesAsync();
        Console.WriteLine($"2. {pharmacy.Id}");

        // Store pharmacy ID for reference
        pharmacyNameToId.Add(pharmacy.Name, pharmacy.Id);
        maskNameToIdFromPharmacy.Add(pharmacy.Id, new Dictionary<string, Guid>());

        return pharmacy;
    }

    /// <summary>
    /// Saves the mask and its type to the database.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <param name="maskJson">The mask JSON object.</param>
    /// <param name="pharmacy">The pharmacy object.</param>
    private static async Task SaveMaskAndMaskType(DataContext context, MaskJson maskJson, Pharmacy pharmacy)
    {
        var (name, color, quantityPerPack) = ProcessMaskName(maskJson.Name);
        if (!maskTypeToId.ContainsKey(maskJson.Name))
        {
            MaskType maskType = new()
            {
                Name = name,
                Color = color,
                Quantity = quantityPerPack,
            };
            context.MaskTypes.Add(maskType);
            await context.SaveChangesAsync();
            maskTypeToId.Add(maskJson.Name, maskType.Id);
        }

        var maskTypeId = maskTypeToId[maskJson.Name];
        Mask mask = new()
        {
            Price = maskJson.Price,
            PharmacyId = pharmacy.Id,
            MaskTypeId = maskTypeId,
        };
        context.Masks.Add(mask);
        await context.SaveChangesAsync();
        maskNameToIdFromPharmacy[pharmacy.Id].Add(maskJson.Name, mask.Id);
    }

    private static async Task ProcessUsersJson(string filePath, DataContext context)
    {
        var usersJson = (List<UserJson>)ReadJsonFile(filePath);

        // Seed the database based on users.json
        foreach (var userJson in usersJson)
        {
            User user = new()
            {
                Name = userJson.Name,
                CashBalance = userJson.CashBalance,
            };

            // Add transactions for the user
            foreach (var purchaseHistory in userJson.PurchaseHistories)
            {
                var pharmacyId = pharmacyNameToId[purchaseHistory.PharmacyName];
                var maskId = maskNameToIdFromPharmacy[pharmacyId][purchaseHistory.MaskName];
                user.Transactions.Add(new Transaction()
                {
                    MaskId = maskId,
                    PharmacyId = pharmacyId,
                    TransactionAmount = purchaseHistory.TransactionAmount,
                    TransactionDate = DateTime.Parse(purchaseHistory.TransactionDate),
                });
            }
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Reads and deserializes a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>A list of deserialized objects.</returns>
    private static IEnumerable<object> ReadJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var jsonData = File.ReadAllText(filePath);

        if (filePath.Contains("users"))
            return JsonSerializer.Deserialize<List<UserJson>>(jsonData)!;
        else if (filePath.Contains("pharmacies"))
            return JsonSerializer.Deserialize<List<PharmacyJson>>(jsonData)!;
        else
            throw new Exception("Unexpected file type");
    }

    /// <summary>
    /// Processes the opening hours string and returns a list of tuples containing week, start time, and end time.
    /// </summary>
    /// <param name="openingHoursString">The opening hours string.</param>
    /// <returns>A list of tuples containing week, start time, and end time.</returns>
    private static List<(int week, TimeOnly startTime, TimeOnly endTime)> ProcessOpeningHoursString(string openingHoursString)
    {
        List<(int week, TimeOnly startTime, TimeOnly endTime)> openingHours = new();

        var splitStrings = openingHoursString.Split(" / ", StringSplitOptions.RemoveEmptyEntries);

        foreach (var splitString in splitStrings)
        {
            var currentString = splitString.Trim().Replace(" - ", "-").Replace(", ", ",");
            var currentStringSplit = currentString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string weekPart = currentStringSplit[0], timePart = currentStringSplit[1];

            // Process time part
            List<(TimeOnly startTime, TimeOnly endTime)> times = ProcessOpeningHoursTimePart(timePart);

            // Process week part
            List<int> dayOfWeeks = ProcessOpeningHoursWeekDayPart(weekPart);

            int timeCount = times.Count;
            foreach (var weekDay in dayOfWeeks)
            {
                openingHours.Add((weekDay, times[0].startTime, times[0].endTime));
                if (timeCount > 1)
                    openingHours.Add(((int)((weekDay + 1) % 7), times[1].startTime, times[1].endTime));
            }
        }

        return openingHours;
    }

    /// <summary>
    /// Processes the time part of the opening hours string.
    /// </summary>
    /// <param name="timePart">The time part of the opening hours string.</param>
    /// <returns>A list of tuples containing start time and end time.</returns>
    private static List<(TimeOnly startTime, TimeOnly endTime)> ProcessOpeningHoursTimePart(string timePart)
    {
        List<(TimeOnly startTime, TimeOnly endTime)> times = new();
        var timePartSplit = timePart.Split('-', StringSplitOptions.RemoveEmptyEntries);
        TimeOnly startTime = TimeOnly.Parse(timePartSplit[0]), endTime = TimeOnly.Parse(timePartSplit[1]);
        if (endTime < startTime)
        {
            times.Add((startTime, TimeOnly.MaxValue));
            times.Add((TimeOnly.MinValue, endTime));
        }
        else
        {
            times.Add((startTime, endTime));
        }
        return times;
    }

    /// <summary>
    /// Processes the week part of the opening hours string.
    /// </summary>
    /// <param name="weekPart">The week part of the opening hours string.</param>
    /// <returns>A list of day of weeks as int.</returns>
    private static List<int> ProcessOpeningHoursWeekDayPart(string weekPart)
    {
        List<int> dayOfWeeks = new();
        if (weekPart.Contains('-'))
        {
            var weekPartContinuousSplit = weekPart.Split('-', StringSplitOptions.RemoveEmptyEntries);
            int startWeekDay = (int)(DayOfWeekAbbr)Enum.Parse(typeof(DayOfWeekAbbr), weekPartContinuousSplit[0]);
            int endWeekDay = (int)(DayOfWeekAbbr)Enum.Parse(typeof(DayOfWeekAbbr), weekPartContinuousSplit[1]);
            for (var i = startWeekDay; i <= endWeekDay; i++)
            {
                dayOfWeeks.Add(i);
            }
        }
        else
        {
            var weekPartSingleSplit = weekPart.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in weekPartSingleSplit)
            {
                dayOfWeeks.Add((int)(DayOfWeekAbbr)Enum.Parse(typeof(DayOfWeekAbbr), item));
            }
        }
        return dayOfWeeks;
    }

    /// <summary>
    /// Processes the mask name string and returns a tuple containing name, color, and quantity per pack.
    /// </summary>
    /// <param name="maskName">The mask name string.</param>
    /// <returns>A tuple containing name, color, and quantity per pack.</returns>
    private static (string name, string color, int quantityPerPack) ProcessMaskName(string maskName)
    {
        var maskNameSplit = maskName.Split("(", StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < maskNameSplit.Length; i++)
        {
            maskNameSplit[i] = maskNameSplit[i].Trim().Replace(")", "");
        }

        string name = maskNameSplit[0];
        string color = maskNameSplit[1];
        var split = maskNameSplit[2].Split(" ", StringSplitOptions.RemoveEmptyEntries);
        int quantityPerPack = int.Parse(split[0]);

        return (name, color, quantityPerPack);
    }
}