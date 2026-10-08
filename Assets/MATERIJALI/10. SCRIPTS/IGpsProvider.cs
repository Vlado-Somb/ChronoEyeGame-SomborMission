using System;
public interface IGpsProvider
{
    event Action OnLocationChanged;
    bool IsGpsAvailable { get; }
    double Latitude { get; }
    double Longitude { get; }
    double Altitude { get; }

}
