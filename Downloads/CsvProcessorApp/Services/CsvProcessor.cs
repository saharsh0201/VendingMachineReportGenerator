using CsvHelper;
using CsvHelper.Configuration;
using CsvProcessorApp.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvProcessorApp.Services
{
    public static class CsvProcessor
    {
        public static ProcessingResult MatchFiles(
            string empPath,
            List<string> tlistPaths,
            out string combinedCsv,
            out DateTime startDate,
            out DateTime endDate)
        {
            var empRecords = ReadCsv(empPath);
            var tlistRecords = tlistPaths.SelectMany(ReadCsv).ToList();

            var dateProvider = CultureInfo.CreateSpecificCulture("en-US");

            DateTime ParseDateTlist(string input)
            {
                input = input.Trim();
                if (DateTime.TryParseExact(input, "MMMM dd, yyyy HH:mm", dateProvider, DateTimeStyles.None, out DateTime dt))
                    return dt;
                return DateTime.MinValue;
            }

            DateTime ParseDateEmplist(string input)
            {
                input = input.Trim();
                if (DateTime.TryParseExact(input, "dd MMM HH:mm", dateProvider, DateTimeStyles.None, out DateTime dt))
                    return dt;
                return DateTime.MinValue;
            }

            var tlistDates = tlistRecords
                .Where(r => r.ContainsKey("created_at"))
                .Select(r => ParseDateTlist(r["created_at"]))
                .Where(dt => dt != DateTime.MinValue)
                .ToList();

            startDate = tlistDates.Count > 0 ? tlistDates.Min() : DateTime.MinValue;
            endDate = tlistDates.Count > 0 ? tlistDates.Max() : DateTime.MinValue;

            var matchedTxnIds = new HashSet<string>();
            var sb = new StringBuilder();

            // Initialize MachineWiseStats dictionary
            var result = new ProcessingResult
            {
                MachineWiseStats = new Dictionary<string, MachineStats>()
            };

            sb.AppendLine("id,transaction_id,employee_name,created_at,used_amount,refunded_amount,POS_ID,status");

            foreach (var emp in empRecords)
            {
                string empStatus = emp.GetValueOrDefault("order_status", "");
                if (empStatus == "PAYMENT_FAILED")
                    continue;

                string empTxnId = emp.GetValueOrDefault("transaction_id", "N/A");
                string employeeName = emp.GetValueOrDefault("employee_name", "");
                string createdAtStr = emp.GetValueOrDefault("created_at", "");
                string machineName = ExtractMachineIdEmplist(emp.GetValueOrDefault("machine_name", ""));
                decimal price = ExtractPriceFromProduct(emp.GetValueOrDefault("product_name", ""));
                DateTime empDate = ParseDateEmplist(createdAtStr);

                if (empDate == DateTime.MinValue)
                    continue;

                var match = tlistRecords.FirstOrDefault(t =>
                {
                    if (!t.ContainsKey("used_amount") || !t.ContainsKey("created_at") ||
                        !t.ContainsKey("POS_ID") || !t.ContainsKey("wallettxnid") || !t.ContainsKey("status") || !t.ContainsKey("id"))
                        return false;

                    DateTime tDate = ParseDateTlist(t["created_at"]);

                    if (Math.Abs((tDate - empDate).TotalMinutes) > 1)
                        return false;

                    if (ExtractMachineIdTlist(t["POS_ID"]) != machineName)
                        return false;

                    if (!decimal.TryParse(t["used_amount"], out decimal usedAmt))
                        return false;

                    if (!decimal.TryParse(t["total_amount"], out decimal totalAmt))
                        return false;

                    if (totalAmt != price)
                        return false;

                    string txnid = t["wallettxnid"].Trim('\'', '"').Trim();
                    if (matchedTxnIds.Contains(txnid))
                        return false;

                    matchedTxnIds.Add(txnid);
                    return true;
                });

                string tlistId = match != null ? match.GetValueOrDefault("id", "N/A") : "N/A";
                string posId = match != null ? match.GetValueOrDefault("POS_ID", "N/A") : "N/A";
                decimal used = match != null && decimal.TryParse(match.GetValueOrDefault("used_amount", "0"), out var u) ? u : 0;
                decimal refunded = match != null && decimal.TryParse(match.GetValueOrDefault("refunded_amount", "0"), out var r) ? r : 0;

                string status = "asdf";
                if (match != null && match.TryGetValue("status", out string tlistStatus))
                {
                    if (tlistStatus == "Reversed")
                    {
                        status = "Fully Refunded";
                    }
                    else if (tlistStatus == "Vend Success" && refunded > 0)
                    {
                        status = "Partially Refunded";
                    }
                    else if (tlistStatus == "Vend Success")
                    {
                        status = "Vend Success";
                    }
                }

                // Ensure machine stats entry exists
                if (!result.MachineWiseStats.ContainsKey(machineName))
                    result.MachineWiseStats[machineName] = new MachineStats();

                var machineStats = result.MachineWiseStats[machineName];

                result.TotalUsedAmount += used;
                result.TotalRefundedAmount += refunded;
                machineStats.TotalUsedAmount += used;
                machineStats.TotalRefundedAmount += refunded;

                if (status == "Vend Success")
                {
                    result.TotalTransactions++;
                    machineStats.TotalTransactions++;
                }
                else if (status == "Fully Refunded")
                {
                    result.RefundedTransactions++;
                    result.FullyRefundedAmount += refunded;

                    machineStats.RefundedTransactions++;
                    machineStats.FullyRefundedAmount += refunded;
                }
                else if (status == "Partially Refunded")
                {
                    result.PartiallyRefundedTransactions++;
                    result.PartiallyRefundedAmount += refunded;

                    result.TotalTransactions++;
                    machineStats.TotalTransactions++;

                    machineStats.PartiallyRefundedTransactions++;
                    machineStats.PartiallyRefundedAmount += refunded;
                }
                result.StartDate = startDate;
                result.EndDate = endDate;

                sb.AppendLine($"{tlistId},{empTxnId},{employeeName},{createdAtStr},{used},{refunded},{posId},{status}");
            }

            // Filter out machines with zero stats
            result.MachineWiseStats = result.MachineWiseStats
                .Where(kvp =>
                    kvp.Value.TotalTransactions > 0 ||
                    kvp.Value.TotalUsedAmount > 0 ||
                    kvp.Value.TotalRefundedAmount > 0 ||
                    kvp.Value.RefundedTransactions > 0 ||
                    kvp.Value.FullyRefundedAmount > 0 ||
                    kvp.Value.PartiallyRefundedTransactions > 0 ||
                    kvp.Value.PartiallyRefundedAmount > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            combinedCsv = sb.ToString();
            return result;
        }

        private static List<Dictionary<string, string>> ReadCsv(string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectColumnCountChanges = true,
                PrepareHeaderForMatch = args => args.Header.ToLower().Trim()
            });

            var records = new List<Dictionary<string, string>>();
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                var row = csv.HeaderRecord.ToDictionary(
                    header => header,
                    header => csv.GetField(header));
                records.Add(row);
            }

            return records;
        }

        private static string ExtractMachineIdEmplist(string input)
        {
            var match = Regex.Match(input, @"\(?(\d{5})\)?");
            if (match.Success)
                return match.Groups[1].Value;

            // Fallback: return whole input if no match
            return input?.Trim() ?? "UNKNOWN";
        }

        private static string ExtractMachineIdTlist(string input)
        {
            return input.Replace("'", "");
        }

        private static decimal ExtractPriceFromProduct(string product)
        {
            var match = Regex.Match(product, @"(?:Rs\.?\s*)(\d+(?:\.\d{1,2})?)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                return decimal.Parse(match.Groups[1].Value);
            }

            match = Regex.Match(product, @"\d+(?:\.\d{1,2})?");
            return match.Success ? decimal.Parse(match.Value) : 0;
        }

        private static string GetValueOrDefault(this Dictionary<string, string> dict, string key, string fallback)
        {
            return dict.TryGetValue(key, out var val) ? val : fallback;
        }
    }
}
