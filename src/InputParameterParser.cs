//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:    Robert M. Scheller, James B. Domingo

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.LinearWind
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParameterParser
        : TextParser<IInputParameters>
    {
        public static IEcoregionDataset EcoregionsDataset = PlugIn.ModelCore.Ecoregions;

        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }

        //---------------------------------------------------------------------

        public InputParameterParser()
        {
            // FIXME: Hack to ensure that Percentage is registered with InputValues
            Edu.Wisc.Forest.Flel.Util.Percentage p = new Edu.Wisc.Forest.Flel.Util.Percentage();
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {

            const string WindSeverities = "WindSeverities";
            const string EcoregionTable = "EcoregionModifiers";

            ReadLandisDataVar();

            InputParameters parameters = new InputParameters(PlugIn.ModelCore.Ecoregions.Count);

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            //Read in the number of events mean
            InputVar<double> numEventsMean = new InputVar<double>("NumEventsMean");
            ReadVar(numEventsMean);
            parameters.NumEventsMean = numEventsMean.Value;

            //Read in the number of events stdev
            InputVar<double> numEventsStDev = new InputVar<double>("NumEventsStDev");
            ReadVar(numEventsStDev);
            parameters.NumEventsStDev = numEventsStDev.Value;

            //Read in the tornado length lambda
            InputVar<double> tornadoLengthLambda = new InputVar<double>("TornadoLengthLambda");
            ReadVar(tornadoLengthLambda);
            parameters.TornadoLengthLambda = tornadoLengthLambda.Value;

            //Read in the tornado length alpha
            InputVar<double> tornadoLengthAlpha = new InputVar<double>("TornadoLengthAlpha");
            ReadVar(tornadoLengthAlpha);
            parameters.TornadoLengthAlpha = tornadoLengthAlpha.Value;

            //Read in the tornado width
            InputVar<double> tornadoWidth = new InputVar<double>("TornadoWidth");
            ReadVar(tornadoWidth);
            parameters.TornadoWidth = tornadoWidth.Value;

            //  Read table of wind intensity percentages
            InputVar<double> tornadoWindIntPct = new InputVar<double>("Intensity Percent");

            ReadName("TornadoIntensityTable");
            int windIntIndex = 0;
            double cumulativeIntPct = 0;
            while (!AtEndOfInput && CurrentName != "TornadoProp")
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(tornadoWindIntPct, currentLine);
                cumulativeIntPct += tornadoWindIntPct.Value;
                parameters.TornadoWindIntPct.Add(cumulativeIntPct);
                windIntIndex++;
                GetNextLine();
            }
            if (!(cumulativeIntPct == 100))
            {
                throw new InputValueException(tornadoWindIntPct.Value.String,
                                                  "TornadoIntensityTable percentages do not sum to 100");
            }

            //Read in the tornado proportion
            InputVar<double> tornadoProp = new InputVar<double>("TornadoProp");
            ReadVar(tornadoProp);
            parameters.TornadoProp = tornadoProp.Value;

            //Read in the derecho length lambda
            InputVar<double> derechoLengthLambda = new InputVar<double>("DerechoLengthLambda");
            ReadVar(derechoLengthLambda);
            parameters.DerechoLengthLambda = derechoLengthLambda.Value;

            //Read in the derecho length alpha
            InputVar<double> derechoLengthAlpha = new InputVar<double>("DerechoLengthAlpha");
            ReadVar(derechoLengthAlpha);
            parameters.DerechoLengthAlpha = derechoLengthAlpha.Value;

            //Read in the derecho width
            InputVar<double> derechoWidth = new InputVar<double>("DerechoWidth");
            ReadVar(derechoWidth);
            parameters.DerechoWidth = derechoWidth.Value;

            //  Read table of wind intensity percentages
            InputVar<double> derechoWindIntPct = new InputVar<double>("Intensity Percent");

            ReadName("DerechoIntensityTable");
            windIntIndex = 0;
            cumulativeIntPct = 0;
            while (!AtEndOfInput && CurrentName != "PropIntensityVar")
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(derechoWindIntPct, currentLine);
                cumulativeIntPct += derechoWindIntPct.Value;
                parameters.DerechoWindIntPct.Add(cumulativeIntPct);
                windIntIndex++;
                GetNextLine();
            } 
            if (!(cumulativeIntPct == 100))
            {
                throw new InputValueException(derechoWindIntPct.Value.String,
                                                  "DerechoIntensityTable percentages do not sum to 100");
            }

            //Read in the proportion of intensity variability
            InputVar<double> propIntensityVar = new InputVar<double>("PropIntensityVar");
            ReadVar(propIntensityVar);
            parameters.PropIntensityVar= propIntensityVar.Value;
            
            //  Read table of wind direction percentages
            InputVar<double> windDirPct = new InputVar<double>("Direction Percent");
            //List<double> windDirPctList = new List<double>(4);

            ReadName("WindDirectionTable");
            int windDirIndex = 0;
            double cumulativePct = 0;
            while (!AtEndOfInput && CurrentName != EcoregionTable && CurrentName != WindSeverities)
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(windDirPct, currentLine);
                cumulativePct += windDirPct.Value;
                parameters.WindDirPct.Add(cumulativePct);
                windDirIndex++;
                GetNextLine();
            }
            if(!(cumulativePct == 100))
            {
                throw new InputValueException(windDirPct.Value.String,
                                                  "WindDirectionTable percentages do not sum to 100");
            }
            if (CurrentName == EcoregionTable)  //Ecoregion modifiers are optional
            {
                //--------- Read In Ecoreigon Table ---------------------------------------
                ReadName(EcoregionTable);
                PlugIn.ModelCore.UI.WriteLine("Begin parsing ECOREGION table.");

                InputVar<string> ecoName = new InputVar<string>("Ecoregion Name");
                InputVar<double> ecoModifier = new InputVar<double>("Ecoregion Modifier");

                Dictionary<string, int> lineNumbers = new Dictionary<string, int>();


                while (!AtEndOfInput && CurrentName != WindSeverities)
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    ReadValue(ecoName, currentLine);
                    IEcoregion ecoregion = EcoregionsDataset[ecoName.Value.Actual];
                    if (ecoregion == null)
                        throw new InputValueException(ecoName.Value.String,
                                                      "{0} is not an ecoregion name.",
                                                      ecoName.Value.String);
                    int lineNumber;
                    if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                        throw new InputValueException(ecoName.Value.String,
                                                      "The ecoregion {0} was previously used on line {1}",
                                                      ecoName.Value.String, lineNumber);
                    else
                        lineNumbers[ecoregion.Name] = LineNumber;

                    IEcoParameters ecoParms = new EcoParameters();
                    ReadValue(ecoModifier, currentLine);
                    ecoParms.EcoModifier = ecoModifier.Value;
                    parameters.EcoParameters[ecoregion.Index] = ecoParms;

                    CheckNoDataAfter("the " + ecoModifier.Name + " column",
                                     currentLine);
                    GetNextLine();
                }
            }
            //  Read table of wind severities.
            //  Severities are in decreasing order.
            ReadName(WindSeverities);

            InputVar<byte> number = new InputVar<byte>("Severity Number");
            InputVar<Percentage> minAge = new InputVar<Percentage>("Min Age");
            InputVar<Percentage> maxAge = new InputVar<Percentage>("Max Age");
            InputVar<float> mortalityThreshold = new InputVar<float>("Mortality Threshold");

            
            const string IntensityMapNames = "IntensityMapNames";
            const string SeverityMapNames = "SeverityMapNames";
            const string LogFile = "LogFile";
            byte previousNumber = 6;
            Percentage previousMaxAge = null;

            while (!AtEndOfInput && CurrentName != SeverityMapNames && CurrentName != IntensityMapNames && CurrentName != LogFile && previousNumber != 1)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ISeverity severity = new Severity();
                parameters.WindSeverities.Add(severity);

                ReadValue(number, currentLine);
                severity.Number = number.Value;

                //  Check that the current severity's number is 1 less than
                //  the previous number (numbers are must be in decreasing
                //  order).
                if (number.Value.Actual != previousNumber - 1)
                    throw new InputValueException(number.Value.String,
                                                  "Expected the severity number {0}",
                                                  previousNumber - 1);
                previousNumber = number.Value.Actual;

                ReadValue(minAge, currentLine);

                severity.MinAge = (double) minAge.Value.Actual;

                if (parameters.WindSeverities.Count == 1) {
                    //  Minimum age for this severity must be equal to 0%
                    if (minAge.Value.Actual != 0)
                        throw new InputValueException(minAge.Value.String,
                                                      "It must be 0% for the first severity");
                }
                else {
                    //  Minimum age for this severity must be equal to the
                    //  maximum age of previous severity.
                    if (minAge.Value.Actual != (double) previousMaxAge)
                        throw new InputValueException(minAge.Value.String,
                                                      "It must equal the maximum age ({0}) of the preceeding severity",
                                                      previousMaxAge);
                }

                TextReader.SkipWhitespace(currentLine);
                string word = TextReader.ReadWord(currentLine);
                if (word != "to") {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Expected \"to\" after the minimum age ({0})",
                                         minAge.Value.String);
                    if (word.Length > 0)
                        message.AppendFormat(", but found \"{0}\" instead", word);
                    throw NewParseException(message.ToString());
                }

                ReadValue(maxAge, currentLine);
                severity.MaxAge = (double) maxAge.Value.Actual;
                if (number.Value.Actual == 1) {
                    //  Maximum age for the last severity must be 100%
                    if (maxAge.Value.Actual != 1)
                        throw new InputValueException(minAge.Value.String,
                                                      "It must be 100% for the last severity");
                }
                previousMaxAge = maxAge.Value.Actual;

                ReadValue(mortalityThreshold, currentLine);
                severity.MortalityThreshold = mortalityThreshold.Value;

                CheckNoDataAfter("the " + mortalityThreshold.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            if (parameters.WindSeverities.Count == 0)
                throw NewParseException("No severities defined.");
            if (previousNumber != 1)
                throw NewParseException("Expected wind severity {0}", previousNumber - 1);


            InputVar<string> intensityMapNames = new InputVar<string>(IntensityMapNames);
            if (ReadOptionalVar(intensityMapNames))
            {
                parameters.IntensityMapNamesTemplate = intensityMapNames.Value;
            }

            InputVar<string> severityMapNames = new InputVar<string>(SeverityMapNames);
            if (ReadOptionalVar(severityMapNames))
            {
                parameters.MapNamesTemplate = severityMapNames.Value;
            }

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;

            CheckNoDataAfter(string.Format("the {0} parameter", logFile.Name));

            return parameters; //.GetComplete();
        }
    }
}
