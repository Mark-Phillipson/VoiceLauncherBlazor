using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models;

public class Transaction
{
    // Properties Should Be Based on the Following Headers:
    // "Date","Description","Type","Money In","Money Out","Balance"
    public int Id { get; set; }
    public DateTime Date { get; set; }
    [StringLength(150)]
    public string? Description { get; set; }
    [StringLength(70)]
    public string? Type { get; set; }
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public decimal Balance { get; set; }
    [StringLength(70)]
    public string? MyTransactionType { get; set; }
    public string? ImportFilename { get; set; }
    public DateTime ImportDate { get; set; }
}