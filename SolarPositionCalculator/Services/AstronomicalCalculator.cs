using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of astronomical calculations for solar positioning
/// </summary>
public class AstronomicalCalculator : IAstronomicalCalculator
{
    /// <summary>
    /// Calculates the Julian day number for a given date and time
    /// Based on the algorithm from Jean Meeus "Astronomical Algorithms"
    /// </summary>
    public double CalculateJulianDay(DateTime dateTime)
    {
        // Convert to UTC if not already
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
        
        int year = utcDateTime.Year;
        int month = utcDateTime.Month;
        int day = utcDateTime.Day;
        double hour = utcDateTime.Hour + utcDateTime.Minute / 60.0 + utcDateTime.Second / 3600.0;

        // Adjust for January and February being counted as months 13 and 14 of the previous year
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        // Calculate Julian day number
        int a = year / 100;
        int b = 2 - a + (a / 4);

        double julianDay = Math.Floor(365.25 * (year + 4716)) + 
                          Math.Floor(30.6001 * (month + 1)) + 
                          day + hour / 24.0 + b - 1524.5;

        return julianDay;
    }

    /// <summary>
    /// Calculates the solar declination angle for a given Julian day
    /// Uses simplified formula suitable for most applications
    /// </summary>
    public double CalculateSolarDeclination(double julianDay)
    {
        // Days since J2000.0 epoch
        double n = julianDay - AstronomicalConstants.J2000;
        
        // Mean longitude of the sun (degrees)
        double L = (280.460 + 0.9856474 * n) % 360;
        
        // Mean anomaly of the sun (degrees)
        double g = ((357.528 + 0.9856003 * n) % 360) * AstronomicalConstants.DegreesToRadians;
        
        // Ecliptic longitude of the sun (degrees)
        double lambda = (L + 1.915 * Math.Sin(g) + 0.020 * Math.Sin(2 * g)) * AstronomicalConstants.DegreesToRadians;
        
        // Solar declination (degrees)
        double declination = Math.Asin(Math.Sin(AstronomicalConstants.EarthObliquity * AstronomicalConstants.DegreesToRadians) * 
                                     Math.Sin(lambda)) * AstronomicalConstants.RadiansToDegrees;
        
        return declination;
    }

    /// <summary>
    /// Calculates the solar position (azimuth and elevation) for a given location and time
    /// </summary>
    public SolarPosition CalculateSolarPosition(GeographicCoordinate location, DateTime dateTime)
    {
        if (!location.IsValid)
            throw new ArgumentException("Invalid geographic coordinates", nameof(location));

        double julianDay = CalculateJulianDay(dateTime);
        double declination = CalculateSolarDeclination(julianDay);
        
        // Calculate hour angle
        double hourAngle = CalculateHourAngle(dateTime, location.Longitude);
        
        // Convert to radians for calculations
        double latRad = location.Latitude * AstronomicalConstants.DegreesToRadians;
        double decRad = declination * AstronomicalConstants.DegreesToRadians;
        double hourAngleRad = hourAngle * AstronomicalConstants.DegreesToRadians;
        
        // Calculate elevation angle
        double elevationRad = Math.Asin(
            Math.Sin(decRad) * Math.Sin(latRad) + 
            Math.Cos(decRad) * Math.Cos(latRad) * Math.Cos(hourAngleRad)
        );
        
        double elevation = elevationRad * AstronomicalConstants.RadiansToDegrees;
        
        // Apply atmospheric refraction correction for low elevations
        if (elevation > -AstronomicalConstants.AtmosphericRefraction)
        {
            elevation += CalculateAtmosphericRefraction(elevation);
        }
        
        // Calculate azimuth angle
        double azimuthRad = Math.Atan2(
            Math.Sin(hourAngleRad),
            Math.Cos(hourAngleRad) * Math.Sin(latRad) - Math.Tan(decRad) * Math.Cos(latRad)
        );
        
        double azimuth = (azimuthRad * AstronomicalConstants.RadiansToDegrees + 180) % 360;
        
        return new SolarPosition(azimuth, elevation, dateTime, location);
    }

    /// <summary>
    /// Calculates the equation of time for a specific date
    /// </summary>
    public double CalculateEquationOfTime(DateTime date)
    {
        double julianDay = CalculateJulianDay(date);
        double n = julianDay - AstronomicalConstants.J2000;
        
        // Mean longitude of the sun (radians)
        double L = ((280.460 + 0.9856474 * n) % 360) * AstronomicalConstants.DegreesToRadians;
        
        // Mean anomaly of the sun (radians)
        double g = ((357.528 + 0.9856003 * n) % 360) * AstronomicalConstants.DegreesToRadians;
        
        // Equation of time in minutes
        double equationOfTime = 4 * (L - 0.0057183 - Math.Atan2(Math.Tan(L), Math.Cos(AstronomicalConstants.EarthObliquity * AstronomicalConstants.DegreesToRadians))) * AstronomicalConstants.RadiansToDegrees;
        
        // Alternative more accurate formula
        double y = Math.Tan(AstronomicalConstants.EarthObliquity * AstronomicalConstants.DegreesToRadians / 2);
        y *= y;
        
        double sin2L = Math.Sin(2 * L);
        double sinM = Math.Sin(g);
        double cos2L = Math.Cos(2 * L);
        double sin4L = Math.Sin(4 * L);
        double sin2M = Math.Sin(2 * g);
        
        equationOfTime = y * sin2L - 2 * AstronomicalConstants.EccentricityFactor * sinM + 
                        4 * AstronomicalConstants.EccentricityFactor * y * sinM * cos2L - 
                        0.5 * y * y * sin4L - 1.25 * AstronomicalConstants.EccentricityFactor * AstronomicalConstants.EccentricityFactor * sin2M;
        
        return equationOfTime * AstronomicalConstants.RadiansToDegrees * 4; // Convert to minutes
    }

    /// <summary>
    /// Calculates the complete sun path for a given location and date
    /// </summary>
    public SunPath CalculateDailySunPath(GeographicCoordinate location, DateTime date)
    {
        if (!location.IsValid)
            throw new ArgumentException("Invalid geographic coordinates", nameof(location));

        var dailyPositions = new List<SolarPosition>();
        SolarPosition? sunrise = null;
        SolarPosition? sunset = null;
        
        // Calculate positions every 15 minutes throughout the day
        var baseDate = date.Date;
        for (int minutes = 0; minutes < 1440; minutes += 15) // 1440 minutes in a day
        {
            var currentTime = baseDate.AddMinutes(minutes);
            var position = CalculateSolarPosition(location, currentTime);
            dailyPositions.Add(position);
            
            // Detect sunrise (first time sun becomes visible)
            if (sunrise == null && position.IsSunVisible && minutes > 0)
            {
                // Refine sunrise time with higher precision
                sunrise = FindSunriseOrSunset(location, baseDate.AddMinutes(minutes - 15), baseDate.AddMinutes(minutes), true);
            }
            
            // Detect sunset (last time sun is visible)
            if (position.IsSunVisible)
            {
                var nextPosition = CalculateSolarPosition(location, currentTime.AddMinutes(15));
                if (!nextPosition.IsSunVisible)
                {
                    // Refine sunset time with higher precision
                    sunset = FindSunriseOrSunset(location, currentTime, currentTime.AddMinutes(15), false);
                }
            }
        }
        
        return new SunPath(location, date, dailyPositions.ToArray(), sunrise, sunset);
    }

    /// <summary>
    /// Calculates equation of time data for an entire year
    /// </summary>
    public EquationOfTimeData[] CalculateAnnualEquationOfTime(int year)
    {
        var data = new List<EquationOfTimeData>();
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);
        
        // Calculate equation of time for every day of the year
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            double equationOfTime = CalculateEquationOfTime(date);
            data.Add(new EquationOfTimeData(date, equationOfTime));
        }
        
        return data.ToArray();
    }

    /// <summary>
    /// Calculates the hour angle for a given time and longitude
    /// </summary>
    private double CalculateHourAngle(DateTime dateTime, double longitude)
    {
        // Convert to UTC if not already
        var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
        
        // Calculate solar time
        double solarTime = utcDateTime.Hour + utcDateTime.Minute / 60.0 + utcDateTime.Second / 3600.0;
        
        // Apply longitude correction (4 minutes per degree)
        solarTime += longitude / 15.0;
        
        // Apply equation of time correction
        double equationOfTime = CalculateEquationOfTime(dateTime);
        solarTime += equationOfTime / 60.0;
        
        // Hour angle in degrees (15 degrees per hour, 0 at solar noon)
        double hourAngle = (solarTime - 12.0) * 15.0;
        
        return hourAngle;
    }

    /// <summary>
    /// Calculates atmospheric refraction correction for elevation angles
    /// </summary>
    private double CalculateAtmosphericRefraction(double elevation)
    {
        if (elevation < -0.575)
            return 0;
        
        if (elevation > 85)
            return 0;
        
        // Simplified atmospheric refraction formula
        double elevationRad = elevation * AstronomicalConstants.DegreesToRadians;
        double refraction = 1.02 / Math.Tan(elevationRad + 10.3 / (elevationRad + 5.11)) / 60.0;
        
        return refraction;
    }

    /// <summary>
    /// Determines if a location experiences polar conditions (midnight sun or polar night)
    /// </summary>
    public PolarCondition GetPolarCondition(GeographicCoordinate location, DateTime date)
    {
        if (!location.IsValid)
            throw new ArgumentException("Invalid geographic coordinates", nameof(location));

        bool isPolarRegion = IsPolarRegion(location);
        
        // Calculate sun path for the day to analyze conditions
        var sunPath = CalculateDailySunPath(location, date);
        
        double maxElevation = sunPath.DailyPositions.Max(p => p.Elevation);
        double minElevation = sunPath.DailyPositions.Min(p => p.Elevation);
        
        // Determine polar condition type
        PolarConditionType conditionType;
        string description;
        TimeSpan? daylightDuration = null;
        
        if (sunPath.IsPolarDay)
        {
            conditionType = PolarConditionType.MidnightSun;
            description = "The sun remains above the horizon for the entire 24-hour period.";
            daylightDuration = TimeSpan.FromHours(24);
        }
        else if (sunPath.IsPolarNight)
        {
            if (maxElevation > -6)
            {
                conditionType = PolarConditionType.CivilTwilight;
                description = "Continuous civil twilight - the sun stays close to but below the horizon.";
            }
            else if (maxElevation > -12)
            {
                conditionType = PolarConditionType.NauticalTwilight;
                description = "Continuous nautical twilight - suitable for navigation by stars.";
            }
            else if (maxElevation > -18)
            {
                conditionType = PolarConditionType.AstronomicalTwilight;
                description = "Continuous astronomical twilight - dark sky conditions.";
            }
            else
            {
                conditionType = PolarConditionType.PolarNight;
                description = "Complete polar night - the sun remains well below the horizon.";
            }
            daylightDuration = TimeSpan.Zero;
        }
        else
        {
            conditionType = PolarConditionType.Normal;
            description = "Normal day and night cycle with sunrise and sunset.";
            
            // Calculate daylight duration for normal conditions
            if (sunPath.HasSunrise && sunPath.HasSunset)
            {
                daylightDuration = sunPath.Sunset!.Timestamp - sunPath.Sunrise!.Timestamp;
            }
        }
        
        return new PolarCondition(
            conditionType,
            description,
            isPolarRegion,
            maxElevation,
            minElevation,
            daylightDuration);
    }

    /// <summary>
    /// Checks if a location is in a polar region (above Arctic or below Antarctic Circle)
    /// </summary>
    public bool IsPolarRegion(GeographicCoordinate location)
    {
        if (!location.IsValid)
            return false;
            
        // Arctic Circle: approximately 66.5° N
        // Antarctic Circle: approximately 66.5° S
        const double polarCircleLatitude = 66.5;
        
        return Math.Abs(location.Latitude) >= polarCircleLatitude;
    }

    /// <summary>
    /// Finds precise sunrise or sunset time using binary search
    /// </summary>
    private SolarPosition FindSunriseOrSunset(GeographicCoordinate location, DateTime startTime, DateTime endTime, bool isSunrise)
    {
        var precision = TimeSpan.FromSeconds(30); // 30-second precision
        
        while (endTime - startTime > precision)
        {
            var midTime = startTime.AddTicks((endTime - startTime).Ticks / 2);
            var position = CalculateSolarPosition(location, midTime);
            
            if (isSunrise)
            {
                if (position.IsSunVisible)
                    endTime = midTime;
                else
                    startTime = midTime;
            }
            else
            {
                if (position.IsSunVisible)
                    startTime = midTime;
                else
                    endTime = midTime;
            }
        }
        
        return CalculateSolarPosition(location, startTime.AddTicks((endTime - startTime).Ticks / 2));
    }
}