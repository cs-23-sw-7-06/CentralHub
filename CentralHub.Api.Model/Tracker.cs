namespace CentralHub.Api.Model;

public class Tracker
{
    public Tracker(string name, string location, string macAddress, TrackerType trackerType)
    {
        Name = name;
        Location = location;
        MacAddress = macAddress;
        TrackerType = trackerType;
    }

    public int TrackerId { get; set; }

    public string Name { get; set; }

    public string Location { get; set; }

    public string MacAddress { get; set; }

    public TrackerType TrackerType { get; set; }
}
