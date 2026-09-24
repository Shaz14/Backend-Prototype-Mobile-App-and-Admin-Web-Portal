using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.ViewModels;

namespace DHAFacilitationAPIs.Application.Common.Interfaces;
public interface ISmartPayService
{
    /// <summary>
    /// Uploads a bill to SmartPay and returns the result.
    /// </summary>
    Task<BillUploadResult> UploadBillAsync(BillUploadRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Checks the payment status of a bill from SmartPay.
    /// </summary>
    Task<BillInquiryResult> InquireBillAsync(string consumerNumber, CancellationToken cancellationToken);
}
