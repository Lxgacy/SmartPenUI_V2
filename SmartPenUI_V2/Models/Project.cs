using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenUI_V2.Models
{	
	public class Project
	{
		public string ProjectName { get; set; }
		public List<string> Measurements { get; set; }

		public Project(string projectName, List<string> measurements)
		{
			ProjectName = projectName;
			Measurements = measurements;
		}
	}

}
