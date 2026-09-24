using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Application.ViewModels;
public class BillUploadRequest
{
    [Required] [MaxLength(14)] public string consumer_number { get; set; } = string.Empty;
    [Required] [MaxLength(30)] public string consumer_Detail { get; set; } = string.Empty;
    [Required] [MaxLength(14)] public string dueDate { get; set; } = string.Empty;
    [Required] [MaxLength(14)] public string expDate { get; set; } = string.Empty;
    [Required] public decimal amount { get; set; }
    [Required] public decimal lateFee { get; set; }
    [Required] public int billStatus { get; set; } = 1; // Active
    [MaxLength(12)] public string cellNo { get; set; } = string.Empty;
    [MaxLength(100)] public string eMail { get; set; } = string.Empty;
    [MaxLength(20)] public string billReference { get; set; } = string.Empty;
    [MaxLength(200)] public string reservedForFutureUser { get; set; } = string.Empty;
}

public class BillUploadResult
{
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMsg { get; set; } = string.Empty;
}

public class BillInquiryResult
{
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMsg { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidDate { get; set; } = string.Empty;
    public string TranId { get; set; } = string.Empty;
}

