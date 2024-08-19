
using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.DTOs
{
    public partial class TransactionDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [StringLength(150)]
        public string Description { get; set; }
        [StringLength(70)]
        public string Type { get; set; }
        [Required]
        public decimal MoneyIn { get; set; }
        [Required]
        public decimal MoneyOut { get; set; }
        [Required]
        public decimal Balance { get; set; }
        [StringLength(70)]
        public string MyTransactionType { get; set; }
        public string ImportFilename { get; set; }
        public DateTime ImportDate { get; set; }
    }
}