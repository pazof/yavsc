namespace PostIt.Services;

public sealed class CurrentLocationResult
{
    private CurrentLocationResult(bool isSuccess, bool isPermissionDenied, double? latitude, double? longitude, string message)
    {
        IsSuccess = isSuccess;
        IsPermissionDenied = isPermissionDenied;
        Latitude = latitude;
        Longitude = longitude;
        Message = message;
    }

    public bool IsSuccess { get; }
    public bool IsPermissionDenied { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }
    public string Message { get; }

    public static CurrentLocationResult Success(double latitude, double longitude, string? message = null)
        => new(true, false, latitude, longitude, message ?? "Position récupérée.");

    public static CurrentLocationResult PermissionDenied(string? message = null)
        => new(false, true, null, null, message ?? "La géolocalisation n'est pas autorisée.");

    public static CurrentLocationResult Unavailable(string? message = null)
        => new(false, false, null, null, message ?? "La géolocalisation n'est pas disponible sur cette plateforme.");
}
