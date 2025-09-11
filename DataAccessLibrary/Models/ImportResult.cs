using System;
using System.Collections.Generic;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Models;

public class ImportResult
{
    public List<TransactionDTO> Transactions { get; set; } = new List<TransactionDTO>();
    public List<string> Errors { get; set; } = new List<string>();
}