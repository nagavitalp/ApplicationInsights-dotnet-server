﻿namespace Functional.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;

    using FunctionalTests.Helpers;

    using AI;
    using Microsoft.ManagementServices.RealTimeDataProcessing.QuickPulseService;

    using Newtonsoft.Json;
    using System.Text;
    using Newtonsoft.Json.Linq;

    internal static class TelemetryItemFactory
    {
        public static IList<Envelope> GetTelemetryItems(string content)
        {
            var items = new List<Envelope>();

            if (string.IsNullOrWhiteSpace(content))
            {
                return items;
            }

            var newLines = new [] { "\r\n", "\n" };

            string[] lines = content.Split(newLines, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                JsonReader reader = new JsonTextReader(new StringReader(line));
                reader.DateParseHandling = DateParseHandling.None;
                JObject obj = JObject.Load(reader);
                var envelope = obj.ToObject<Envelope>();

                var item = CreateTelemetryItem(envelope, line);
                items.Add(item);
            }

            return items;
        }

        public static MonitoringDataPoint[] CreateQuickPulseSamples(Stream content)
        {
            var serializerDataPointArray = new DataContractJsonSerializer(typeof(MonitoringDataPoint[]));

            return serializerDataPointArray.ReadObject(content) as MonitoringDataPoint[];
        }

        private static Envelope CreateTelemetryItem(
            Envelope envelope, 
            string content)
        {
            Envelope result;

            TelemetryItemType type;
            if (Enum.TryParse<TelemetryItemType>(envelope.data.baseType.Replace("Data", ""), out type))
            {
                switch (type)
                {
                    case TelemetryItemType.Exception:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<ExceptionData>>(content);
                            break;
                        }

                    case TelemetryItemType.Request:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<RequestData>>(content);
                            break;
                        }

                    case TelemetryItemType.Metric:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<MetricData>>(content);
                            break;
                        }

                    case TelemetryItemType.RemoteDependency:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<RemoteDependencyData>>(content);
                            break;
                        }

                    case TelemetryItemType.Message:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<MessageData>>(content);
                            break;
                        }
                    default:
                        {
                            throw new InvalidDataException("Unsupported telemetry type");
                        }
                }
            }
            else
            {
                throw new InvalidDataException("Unsupported telemetry type");
            }

            return result;
        }
    }
}

