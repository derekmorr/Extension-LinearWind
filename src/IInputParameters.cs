//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:    Robert M. Scheller, James B. Domingo

using System.Collections.Generic;

namespace Landis.Extension.LinearWind
{
	/// <summary>
	/// Parameters for the plug-in.
	/// </summary>
	public interface IInputParameters
	{
		/// <summary>
		/// Timestep (years)
		/// </summary>
		int Timestep
		{
			get;set;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Definitions of wind severities.
		/// </summary>
		List<ISeverity> WindSeverities
		{
			get;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Template for the filenames for severity output maps.
		/// </summary>
		string MapNamesTemplate
		{
			get;set;
		}

		//---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for intensity output maps.
        /// </summary>
        string IntensityMapNamesTemplate
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

		/// <summary>
		/// Name of log file.
		/// </summary>
		string LogFileName
		{
			get;set;
		}
        //---------------------------------------------------------------------
        /// <summary>
        /// Mean Events
        /// </summary>
        double NumEventsMean
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// StDev Events
        /// </summary>
        double NumEventsStDev
        {
            get;
            set;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Lambda Length (km) - Tornadoes
        /// </summary>
        double TornadoLengthLambda
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Alpha Length (km) - Tornadoes
        /// </summary>
        double TornadoLengthAlpha
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Width (km) - Tornadoes
        /// </summary>
        double TornadoWidth
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Prop of events with small width - Tornadoes
        /// </summary>
        double TornadoProp
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Lambda Length (km) - Derechos
        /// </summary>
        double DerechoLengthLambda
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Alpha Length (km) - Derechos
        /// </summary>
        double DerechoLengthAlpha
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Large width (km) - Derechos
        /// </summary>
        double DerechoWidth
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Proportion of event with reduced intensity
        /// </summary>
        double PropIntensityVar
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Wind direction percentages
        /// </summary>
        List<double> WindDirPct
        {
            get;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Wind itensity percentages - Tornado
        /// </summary>
        List<double> TornadoWindIntPct
        {
            get;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Wind itensity percentages - Tornado
        /// </summary>
        List<double> DerechoWindIntPct
        {
            get;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// <summary>
        /// Ecoregion modifiers
        /// </summary>
        IEcoParameters[] EcoParameters
        {
            get;
            set;
        }
        //---------------------------------------------------------------------

	}
}
