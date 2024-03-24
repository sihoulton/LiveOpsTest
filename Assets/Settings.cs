using System.Collections;
using System.Collections.Generic;

public class Settings
{
    public string Version { get; set; }
    public IEnumerable<AudioFile> AudioFiles { get; set; }

}

public class AudioFile 
{
    public string Location { get; set; }
    public string Name { get; set; }
}
