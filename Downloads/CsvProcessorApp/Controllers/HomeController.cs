using CsvProcessorApp.Data;
using CsvProcessorApp.Models;
using CsvProcessorApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CsvProcessorApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProcessController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("match")]
        public async Task<IActionResult> MatchFiles([FromForm] IFormFile empFile, [FromForm] List<IFormFile> tlistFiles)
        {
            if (empFile == null || tlistFiles == null || tlistFiles.Count == 0)
                return BadRequest("Emplist is required and at least one Tlist file must be uploaded.");

            var username = HttpContext.User.Identity?.Name ?? "anonymous";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadDir);

            string empPath = await SaveFile(empFile, uploadDir, "EMPLIST", username);
            var tlistPaths = new List<string>();
            var uploadedMachineIds = new List<string>();

            foreach (var file in tlistFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName).ToLower();
                if (fileName.StartsWith("20020") || fileName.StartsWith("20506") || fileName.StartsWith("20508"))
                {
                    var filePath = await SaveFile(file, uploadDir, "TLIST", username);
                    tlistPaths.Add(filePath);
                    uploadedMachineIds.Add(fileName.Substring(0, 5));
                }
            }

            if (tlistPaths.Count == 0)
                return BadRequest("No valid TLIST files found. Filenames must start with 20020, 20506, or 20508.");

            var fullResult = CsvProcessor.MatchFiles(empPath, tlistPaths, out string combinedCsv, out DateTime startDate, out DateTime endDate);
            string resultFileName = $"{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_matched.csv";
            string resultFilePath = Path.Combine(uploadDir, resultFileName);
            await System.IO.File.WriteAllTextAsync(resultFilePath, combinedCsv, Encoding.UTF8);

            string recordId = $"{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            var existing = await _context.ProcessingResults.FindAsync(recordId);

            if (existing == null)
            {
                fullResult.Id = recordId;
                fullResult.StartDate = startDate;
                fullResult.EndDate = endDate;
                fullResult.ResultFilePath = "/uploads/" + resultFileName;
                fullResult.UploadedMachineIds = string.Join(",", uploadedMachineIds);
                _context.ProcessingResults.Add(fullResult);
                await _context.SaveChangesAsync();
            }
            else
            {
                fullResult = existing;
                fullResult.MachineWiseStats = CsvProcessor.MatchFiles(empPath, tlistPaths, out _, out _, out _).MachineWiseStats;
            }

            var dto = new ProcessingResultDto
            {
                Id = fullResult.Id,
                StartDate = fullResult.StartDate,
                EndDate = fullResult.EndDate,
                TotalUsedAmount = fullResult.TotalUsedAmount,
                TotalRefundedAmount = fullResult.TotalRefundedAmount,
                FullyRefundedAmount = fullResult.FullyRefundedAmount,
                PartiallyRefundedAmount = fullResult.PartiallyRefundedAmount,
                TotalTransactions = fullResult.TotalTransactions,
                RefundedTransactions = fullResult.RefundedTransactions,
                PartiallyRefundedTransactions = fullResult.PartiallyRefundedTransactions,
                ResultFilePath = fullResult.ResultFilePath,
                MachineWiseStats = fullResult.MachineWiseStats
            };

            return Ok(new
            {
                success = true,
                downloadLink = dto.ResultFilePath,
                result = dto
            });
        }

        private async Task<string> SaveFile(IFormFile file, string folder, string fileType, string username)
        {
            var filePath = Path.Combine(folder, Path.GetFileName(file.FileName));
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            FileLogger.LogUpload(file.FileName, fileType, true, username);
            return filePath;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var query = _context.ProcessingResults.AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.StartDate >= from.Value.Date);

            if (to.HasValue)
            {
                var endOfDay = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.EndDate <= endOfDay);
            }

            var results = await query.OrderByDescending(r => r.StartDate).ToListAsync();
            return Ok(results ?? new List<ProcessingResult>()); // ✅ Ensure always JSON, never null
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(string id)
        {
            var record = await _context.ProcessingResults.FindAsync(id);
            if (record == null)
                return NotFound();

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
            var empFile = Path.Combine(uploadDir, $"emplist.csv");
            var tlistFiles = new List<string>();

            if (!string.IsNullOrWhiteSpace(record.UploadedMachineIds))
            {
                var machineIds = record.UploadedMachineIds.Split(',');
                foreach (var machineId in machineIds)
                {
                    var filePath = Path.Combine(uploadDir, $"{machineId}_tlist.csv");
                    if (System.IO.File.Exists(filePath))
                        tlistFiles.Add(filePath);
                }
            }

            var latestResult = CsvProcessor.MatchFiles(empFile, tlistFiles, out _, out _, out _);

            var dto = new ProcessingResultDto
            {
                Id = record.Id,
                StartDate = record.StartDate,
                EndDate = record.EndDate,
                TotalUsedAmount = record.TotalUsedAmount,
                TotalRefundedAmount = record.TotalRefundedAmount,
                FullyRefundedAmount = record.FullyRefundedAmount,
                PartiallyRefundedAmount = record.PartiallyRefundedAmount,
                TotalTransactions = record.TotalTransactions,
                RefundedTransactions = record.RefundedTransactions,
                PartiallyRefundedTransactions = record.PartiallyRefundedTransactions,
                ResultFilePath = record.ResultFilePath,
                MachineWiseStats = latestResult.MachineWiseStats
            };

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var record = await _context.ProcessingResults.FindAsync(id);
            if (record != null)
            {
                var physicalPath = Path.Combine(_env.WebRootPath, record.ResultFilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);

                _context.ProcessingResults.Remove(record);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, deletedId = id });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var record = await _context.ProcessingResults.FindAsync(id);
            if (record == null || string.IsNullOrEmpty(record.ResultFilePath))
                return NotFound("Result file not found.");

            var fullPath = Path.Combine(_env.WebRootPath, record.ResultFilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found on disk.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);

            return File(fileBytes, "text/csv", fileName);
        }
    }
}
