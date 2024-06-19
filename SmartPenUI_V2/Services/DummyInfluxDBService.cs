using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartPenUI_V2.Models;

namespace SmartPenUI_V2.Services;
public class DummyInfluxDBService
{
	// list of measurements
	private ObservableCollection<Project> _projects { get; set; } = [];

	public DummyInfluxDBService()
	{
		// Initialize the list of projects
		// dummy values here, can be used to simulate the data from the InfluxDB
		_projects.Add(new Project("TestProject", new List<string> { "Measurement1", "Measurement2", "Measurement3" }));
		_projects.Add(new Project("SmartPen", new List<string> { "MeasurementA", "MeasurementB", "MeasurementC" }));
		_projects.Add(new Project("SmartPenV2", new List<string> { "MeasurementAlpha", "MeasurementBeta", "MeasurementGamma" }));
	}

	public ObservableCollection<Project> GetProjects()
	{
		return _projects;
	}
}
