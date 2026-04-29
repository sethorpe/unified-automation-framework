namespace UAF.Core.Config;

public class BrowserSettings
{
    public string Browser { get; set; } = "Chromium";
    public bool Headless { get; set; } = true;
    public int Timeout { get; set; } = 30000;
}
