using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IBarcodeScanService
{
    Task<BarcodeScanResultDto?> ScanBarcodeAsync(string barcode, int userId);
}
