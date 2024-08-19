using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models;
[Table("TransactionTypeMapping")]
public class TransactionTypeMapping
{
    public int Id { get; set; }
    [StringLength(50), Required]
    public string MyTransactionType { get; set; }
    [StringLength(50), Required]
    public string Type { get; set; }
}
