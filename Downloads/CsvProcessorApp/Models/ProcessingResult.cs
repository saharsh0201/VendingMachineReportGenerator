using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CsvProcessorApp.Models
{


    public class ProcessingResultDto
    {
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalUsedAmount { get; set; }
        public decimal TotalRefundedAmount { get; set; }
        public decimal FullyRefundedAmount { get; set; }
        public decimal PartiallyRefundedAmount { get; set; }

        public int TotalTransactions { get; set; }
        public int RefundedTransactions { get; set; }
        public int PartiallyRefundedTransactions { get; set; }

        public string ResultFilePath { get; set; }

        public Dictionary<string, MachineStats> MachineWiseStats { get; set; } = new();

    }

    public class MachineStats
    {
        public int TotalTransactions { get; set; }
        public decimal TotalUsedAmount { get; set; }
        public decimal TotalRefundedAmount { get; set; }
        public int PartiallyRefundedTransactions { get; set; }
        public decimal PartiallyRefundedAmount { get; set; }
        public int RefundedTransactions { get; set; }
        public decimal FullyRefundedAmount { get; set; }

    }

    public class ProcessingResult
    {
        [Key]
        public string Id { get; set; } // Format: yyyyMMdd_yyyyMMdd

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalUsedAmount { get; set; }
        public decimal TotalRefundedAmount { get; set; }
        public decimal FullyRefundedAmount { get; set; }
        public decimal PartiallyRefundedAmount { get; set; }

        public int TotalTransactions { get; set; }
        public int RefundedTransactions { get; set; }
        public int PartiallyRefundedTransactions { get; set; }

        public string ResultFilePath { get; set; }

        [NotMapped]
        public Dictionary<string, MachineStats> MachineWiseStats { get; set; } = new();
        public string UploadedMachineIds { get; set; }
    }

}
