using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Publishers;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.IO;

namespace ThoughtWorks.CruiseControl.WebDashboard.Plugins.ProjectReporterPlugin
{
	public class ProjectReporterPageRenderer
	{
		private readonly IPathMapper pathMapper;
		private readonly IRequestWrapper requestWrapper;
		private readonly IBuildRetrieverForRequest buildRetrieverForRequest;

		public ProjectReporterPageRenderer(IRequestWrapper requestWrapper, IBuildRetrieverForRequest buildRetrieverForRequest, IPathMapper pathMapper)
		{
			this.buildRetrieverForRequest = buildRetrieverForRequest;
			this.requestWrapper = requestWrapper;
			this.pathMapper = pathMapper;
		}

		public ProjectReportResults Do()
		{
			StringBuilder builder = new StringBuilder();
			try
			{
				using(StringReader logReader = new StringReader(buildRetrieverForRequest.GetBuild(requestWrapper).Log))
				{
					XPathDocument document = new XPathDocument(logReader);
					IList list = (IList) ConfigurationSettings.GetConfig("CCNet/xslFiles");
					foreach (string xslFile in list) 
					{
						builder.Append(Transform(xslFile, document));
						builder.Append("<br/>");
					}
				}
			}
			catch(XmlException ex)
			{
				throw new CruiseControlException(String.Format("Bad XML in logfile: " + ex.Message));
			}

			return new ProjectReportResults(builder.ToString());
		}

		private string Transform(string xslfile, XPathDocument logFileDocument)
		{
			// Is there a better way?
			string directory = Path.GetDirectoryName(xslfile);
			string file = Path.GetFileName(xslfile);
			string transformFile = Path.Combine(pathMapper.GetLocalPathFromURLPath(directory), file);
			return new BuildLogTransformer().Transform(logFileDocument, transformFile);
		}
	}
}
