//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.AgeOnlyCohorts;
using System;

using System.Collections.Generic;

namespace Landis.Extension.LinearWind
{
    public class Event
        : ICohortDisturbance
    {
        //private static RelativeLocation[] neighborhood;
        //private static IEventParameters[] windEventParms;
        private static List<ISeverity> severities;

        private ActiveSite initiationSite;
        private Site endSite;
        private int endRow;
        private int endCol;
        private double length;
        private double width;
        private double intensity;
        private int windDirection;
        private double propIntensityVar;

        private double sizeHectares;
        private int sitesInEvent;
        private int sitesDamaged;
        private int cohortsKilled;
        private double severity;

        private ActiveSite currentSite; // current site where cohorts are being damaged
        private byte siteSeverity;      // used to compute maximum cohort severity at a site
        private double siteIntensity;
        private string type;

        //---------------------------------------------------------------------

        static Event()
        {
           

        }
        //---------------------------------------------------------------------
        public Location StartLocation
        {
            get {
                return initiationSite.Location;
            }
        }

        //---------------------------------------------------------------------
        public Location EndLocation
        {
            get
            {
                return endSite.Location;
            }
        }

        //---------------------------------------------------------------------
        public int EndRow
        {
            get
            {
                return endRow;
            }
        }
        //---------------------------------------------------------------------
        public int EndCol
        {
            get
            {
                return endCol;
            }
        }
        //---------------------------------------------------------------------
        public double Intensity
        {
            get {
                return intensity;
            }
        }
         
        //---------------------------------------------------------------------
        public double PropIntensityVar
        {
            get
            {
                return propIntensityVar;
            }
        }
        //---------------------------------------------------------------------
        public int WindDirection
        {
            get
            {
                return windDirection;
            }
        }
        //---------------------------------------------------------------------
        public double Length
        {
            get
            {
                return length;
            }
        }
        //---------------------------------------------------------------------
        public double Width
        {
            get
            {
                return width;
            }
        }
        //---------------------------------------------------------------------

        public double SizeHectares
        {
            get {
                return sizeHectares;
            }
        }
        //---------------------------------------------------------------------
        public int SitesInEvent
        {
            get {
                return sitesInEvent;
            }
        }
        //---------------------------------------------------------------------
        public int SitesDamaged
        {
            get {
                return sitesDamaged;
            }
        }
        //---------------------------------------------------------------------
        public int CohortsKilled
        {
            get {
                return cohortsKilled;
            }
        }
        //---------------------------------------------------------------------
        public double Severity
        {
            get {
                return severity;
            }
        }
        //---------------------------------------------------------------------
        public string Type
        {
            get
            {
                return type;
            }
        }
        //---------------------------------------------------------------------
        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.ExtType;
            }
        }
        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }
        //---------------------------------------------------------------------
        
        public static void Initialize(List<ISeverity> severities)
        {
            Event.severities = severities;
        }
        
        //---------------------------------------------------------------------

        public static Event Initiate(ActiveSite site,double siteProbability, IInputParameters parameters)
        {

            if (PlugIn.ModelCore.GenerateUniform() <= siteProbability)
            {
                bool eventTornado = (PlugIn.ModelCore.GenerateUniform() <= parameters.TornadoProp);
                double weibullLambda = parameters.TornadoLengthLambda;
                double weibullAlpha = parameters.TornadoLengthAlpha;
                double eventWidth = parameters.TornadoWidth;
                List<double> windIntPct = parameters.TornadoWindIntPct;
                if (!eventTornado)
                {
                    weibullLambda = parameters.DerechoLengthLambda;
                    weibullAlpha = parameters.DerechoLengthAlpha;
                    eventWidth = parameters.DerechoWidth;
                    windIntPct = parameters.DerechoWindIntPct;
                }
                PlugIn.ModelCore.WeibullDistribution.Lambda = weibullLambda;
                PlugIn.ModelCore.WeibullDistribution.Alpha = weibullAlpha;
                double eventLength = PlugIn.ModelCore.WeibullDistribution.NextDouble();
                eventLength = PlugIn.ModelCore.WeibullDistribution.NextDouble();

                int eventDirection = GetWindDirection(parameters.WindDirPct);
                double eventIntensity = GetWindIntensity(windIntPct);

                Event windEvent = new Event(site, eventLength, eventWidth, eventDirection, parameters.PropIntensityVar, eventIntensity, eventTornado);
                windEvent.Spread(PlugIn.ModelCore.CurrentTime, parameters.EcoParameters);
                return windEvent;
            }
            else
                return null;
        }
        //---------------------------------------------------------------------

        private Event(ActiveSite initiationSite,
                      double     eventLength, double eventWidth, int eventDirection, double propIntensityVar, double eventIntensity, bool eventTornado)
        {
            this.initiationSite = initiationSite;
            this.length = eventLength;
            this.width = eventWidth;
            if(eventTornado)
            {
                this.type = "Tornado";
            }
            else
            {
                this.type = "Derecho";
            }
            this.windDirection = eventDirection;
            // LANDIS space has increasing y-coordinates moving from top to bottom
            // Therefore directions need to be converted by swapping N and S in order to map correctly
            int modWindDirection = eventDirection;
            if (modWindDirection == 0)
                modWindDirection = 4;
            else if (modWindDirection == 1)
                modWindDirection = 3;
            else if (modWindDirection == 3)
                modWindDirection = 1;
            else if (modWindDirection == 4)
                modWindDirection = 0;
            else if (modWindDirection == 5)
                modWindDirection = 7;
            else if (windDirection == 7)
                modWindDirection = 5;
            // Randomize the direction +/- 1/8 * PI
            double randomUnif = PlugIn.ModelCore.GenerateUniform();
            double dirRandomizer = (randomUnif * 2.0) - 1.0;  // Random between -1 and 1
            double dirRad = 0.25 * (double)modWindDirection * System.Math.PI + (dirRandomizer * 0.125 * System.Math.PI);  // Cardinal direction +/- 1/8 *PI
            List<int> endLocation = GetEndLocation(initiationSite, eventLength, dirRad);
            //double dirRadOrig = 0.25 * (double)eventDirection * System.Math.PI + (dirRandomizer * 0.125 * System.Math.PI); 
            //double dirAzimuth = dirRad * 180 / Math.PI;
            //this.windDirection = (int)dirAzimuth;

            this.endRow = endLocation[0];
            this.endCol = endLocation[1];
            this.sitesInEvent = 0;
            this.sitesDamaged = 0;
            this.cohortsKilled = 0;
            this.propIntensityVar = propIntensityVar;
            this.intensity = eventIntensity;
        }

        //---------------------------------------------------------------------

        private void Spread(int currentTime, IEcoParameters[] ecoParameters)
        {
            // Future implementation to incorporate spatial autocorellation of intensity variability
            // Grow the event along the axis line, using autocorellation to apply the intensity variability
            // Pass through the landscape to fill in the rest of the event based on distance to the axis
            // Calculate minimum distance to the axis, and get the intensity value for the nearest cell on the axis
            // Use the nearest cell's intensity as the maximum intensity for the target cell, and then apply ecoregion and distance modifiers of intensity
            // Also apply intensity variability to target cells?

            long totalSiteSeverities = 0;

            double slope = (double)(this.StartLocation.Row - this.EndRow) / (double)(this.StartLocation.Column - this.EndCol);
            double intercept = (double)this.StartLocation.Row - slope * (double)this.StartLocation.Column;
            double theta = System.Math.Atan(slope);
            double lengthCells = (this.Length * 1000) / PlugIn.ModelCore.CellLength;
            double radiusCells = ((this.Width * 1000) / PlugIn.ModelCore.CellLength) / 2.0;  //Width is the diameter, distance should be based on radius
            double minY = Math.Min(this.StartLocation.Row, this.EndRow);
            double maxY = Math.Max(this.StartLocation.Row, this.EndRow);
            double minX = Math.Min(this.StartLocation.Column, this.EndCol);
            double maxX = Math.Max(this.StartLocation.Column, this.EndCol);

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape.ActiveSites)
            {
                siteSeverity = 0;
               
                    int siteX = site.Location.Column;
                    int siteY = site.Location.Row;
                    double siteDistance = Math.Sqrt(Math.Pow(Math.Abs(this.StartLocation.Row - siteY), 2) + Math.Pow(Math.Abs(this.StartLocation.Column - siteX), 2));
                    if (siteDistance <= (lengthCells + radiusCells))
                    {
                        double px = this.StartLocation.Column - this.EndCol;
                        double py = this.StartLocation.Row - this.EndRow;

                        double squareP = px * px + py * py;

                        double u = ((siteX - this.EndCol) * px + (siteY - this.EndRow) * py) / squareP;
                        if (u > 1)
                        {
                            u = 1;
                        }
                        else if (u < 0)
                        {
                            u = 0;
                        }
                        double x = this.EndCol + u * px;
                        double y = this.EndRow + u * py;
                        double dx = x - siteX;
                        double dy = y - siteY;
                        double minDistance = Math.Sqrt(dx * dx + dy * dy);

                        if (minDistance <= radiusCells)
                        {
                            currentSite = site;
                            this.sitesInEvent++;
                            double intensitySlope = -1.0 / radiusCells;
                            double intensityInt = 1.0;
                            siteIntensity = (intensitySlope * minDistance + intensityInt) * this.Intensity;
                            if (PlugIn.ModelCore.GenerateUniform() <= this.PropIntensityVar)
                            {
                                siteIntensity -= 0.20;
                            }
                            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                            double ecoModifier = ecoParameters[ecoregion.Index].EcoModifier;
                            siteIntensity += ecoModifier;

                            // Limit site intensity to 0-1
                            siteIntensity = Math.Max(siteIntensity, 0);
                            siteIntensity = Math.Min(siteIntensity, 1);

                            if (siteIntensity > SiteVars.Intensity[site])
                            {
                                SiteVars.Intensity[site] = siteIntensity;

                                KillSiteCohorts(site);
                                if (siteSeverity > 0)
                                {
                                    SiteVars.Event[site] = this;
                                    this.sitesDamaged++;
                                    totalSiteSeverities += siteSeverity;
                                    //UI.WriteLine("  site severity: {0}", siteSeverity);
                                    SiteVars.Disturbed[site] = true;
                                    SiteVars.TimeOfLastEvent[site] = currentTime;
                                    // SiteVars.LastSeverity[site] = siteSeverity;
                                }
                                if (siteSeverity > SiteVars.Severity[site])
                                {
                                    SiteVars.Severity[site] = siteSeverity;
                                }

                            }
                        }
                    
                }
            }
            if (sitesDamaged == 0)
                this.severity = 0;
            else
                this.severity = ((double) totalSiteSeverities) / this.sitesDamaged;
            this.sizeHectares = this.sitesInEvent * (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength) / 10000;
        }

        //---------------------------------------------------------------------
        private void KillSiteCohorts(ActiveSite site)
        {
            SiteVars.Cohorts[site].RemoveMarkedCohorts(this);
        }
        //---------------------------------------------------------------------
        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            float ageAsPercent = cohort.Age / (float) cohort.Species.Longevity;
            foreach (ISeverity severity in severities)
            {
                if(ageAsPercent >= severity.MinAge && ageAsPercent <= severity.MaxAge)
                {
                    if (siteIntensity > severity.MortalityThreshold) {
                       
                        cohortsKilled++;
                        if (severity.Number > siteSeverity)
                            siteSeverity = severity.Number;
                        //UI.WriteLine("  cohort {0}:{1} killed, severity {2}", cohort.Species.Name, cohort.Age, severity.Number);
                        return true;
                    }
                    break;  // No need to search further in the table
                }
            }
            return false;
        }
        //---------------------------------------------------------------------
        public static int GetWindDirection(List<double> windDirPct)
        {
            // ## Change ##
            // Randomly select wind direction (0-7)
            // 0 = N
            // 1 = NE
            // 2 = E
            // 3 = SE
            // 4 = S
            // 5 = SW
            // 6 = W
            // 7 = NW

            double cutOff0 = windDirPct[0] / 2;
            double cutOff1 = (windDirPct[1] - windDirPct[0]) / 2 + cutOff0;
            double cutOff2 = (windDirPct[2] - windDirPct[1]) / 2 + cutOff1;
            double cutOff3 = (windDirPct[3] - windDirPct[2]) / 2 + cutOff2;
            double cutOff4 = windDirPct[0] / 2 + cutOff3;
            double cutOff5 = (windDirPct[1] - windDirPct[0]) / 2 + cutOff4;
            double cutOff6 = (windDirPct[2] - windDirPct[1]) / 2 + cutOff5;

            int windDirection;
            double randNum = PlugIn.ModelCore.GenerateUniform() * 100;

            if (randNum < cutOff0)
                windDirection = 0;
            else if (randNum < cutOff1)
                windDirection = 1;
            else if (randNum < cutOff2)
                windDirection = 2;
            else if (randNum < cutOff3)
                windDirection = 3;
            else if (randNum < cutOff4)
                windDirection = 4;
            else if (randNum < cutOff5)
                windDirection = 5;
            else if (randNum < cutOff6)
                windDirection = 6;
            else
                windDirection = 7;

            return windDirection;
        }
        //---------------------------------------------------------------------
        public static double GetWindIntensity(List<double> windIntPct)
        {
            // ## Change ##
            // Randomly select wind intensity (1-5)

            //double cutOff0 = windIntPct[0];
            //double cutOff1 = (windIntPct[1] - windIntPct[0]) + cutOff0;
            //double cutOff2 = (windIntPct[2] - windIntPct[1]) + cutOff1;
            //double cutOff3 = (windIntPct[3] - windIntPct[2]) + cutOff2;
            //double cutOff4 = (windIntPct[4] - windIntPct[3]) + cutOff3;

            double windIntensity;
            double randNum = PlugIn.ModelCore.GenerateUniform() * 100;

            if (randNum < windIntPct[0])
                windIntensity = 0.2;
            else if (randNum < windIntPct[1])
                windIntensity = 0.4;
            else if (randNum < windIntPct[2])
                windIntensity = 0.6;
            else if (randNum < windIntPct[3])
                windIntensity = 0.8;
            else
                windIntensity = 1.0;

            return windIntensity;
        }
        //---------------------------------------------------------------------
        public static List<int> GetEndLocation(ActiveSite startSite, double length, double dirRad)
        {
            Location startLocation = startSite.Location;
            double lengthCells = (length * 1000) / PlugIn.ModelCore.CellLength;
          double cols = System.Math.Sin(dirRad) * lengthCells;  // change in X
            double rows = System.Math.Cos(dirRad) * lengthCells;  // change in Y
            int newRow = startLocation.Row + (int)System.Math.Round(rows);
            int newCol = startLocation.Column + (int)System.Math.Round(cols);

            List<int> endLocation = new List<int>();
            endLocation.Add(newRow);
            endLocation.Add(newCol);
            
            return endLocation;
        }
        //---------------------------------------------------------------------
        public static List<ActiveSite> GetEventSites(ActiveSite startSite, int endRow, int endCol, double width)
        {
            List<ActiveSite> eventSites = new List<ActiveSite>();

            double slope = (double)(startSite.Location.Row - endRow) / (double)(startSite.Location.Column - endCol);
            double intercept = (double)startSite.Location.Column - slope * (double)startSite.Location.Row;
            double theta = System.Math.Atan(slope);


            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                int siteX = site.Location.Column;
                int siteY = site.Location.Row;
                double lineX = ((double)siteY - intercept) / slope;
                double lineY = slope * (double)siteX + intercept;
                double distanceX = System.Math.Abs((double)siteX - lineX);
                double distanceY = System.Math.Abs((double)siteY - lineY);
                double distanceTangent = System.Math.Sin(theta) * distanceX;
                double minDistance = System.Math.Min(distanceX, distanceY);
                minDistance = System.Math.Min(minDistance, distanceTangent);
                if (minDistance <= width)
                {

                }
            }

            return eventSites;
        }
    }
}
