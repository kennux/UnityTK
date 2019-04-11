using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityTK.Benchmarking;
using UnityTK.Serialization.Prototypes;
using UnityTK.Serialization;

namespace UnityTK.Editor.Benchmarking
{
    /// <summary>
    /// Example benchmark for UnityTK benchmarking
    /// </summary>
    public class PrototypeParserBenchmark : Benchmark
    {
		public class SimplePrototype : IPrototype
		{
			public string identifier { get; set; }
			public int someInt;
		}

		private PrototypeParser parser;

		private string xml;

        protected override void Prepare()
        {
            this.parser = new PrototypeParser(PrototypeParser.CreateXMLSerializer("UnityTK.Editor.Benchmarking"));

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<PrototypeContainer>");
			sb.AppendLine("	<SimplePrototype Id=\"Base\">\n" +
				"		<someInt>32</someInt>\n" +
				"	</SimplePrototype>");

			for (int i = 0; i < 10000; i++)
			{
				sb.Append("	<SimplePrototype Id=\"Test");
				sb.Append(i.ToString());
				sb.AppendLine("\" Inherits=\"Base\">");
				sb.AppendLine("		<someInt>123</someInt>");
				sb.AppendLine("	</SimplePrototype>");
			}

			sb.AppendLine("</PrototypeContainer>");
			this.xml = sb.ToString();
        }

        protected override void RunBenchmark(BenchmarkResult bRes)
        {
            bRes.BeginLabel("10k simple prototypes load");

            this.parser.Parse(this.xml, "TEST");

            foreach (var error in this.parser.GetParsingErrors())
                error.DoUnityDebugLog();

            bRes.EndLabel();
        }
    }
}