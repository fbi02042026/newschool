namespace GaokaoSimulator.UI
{
    public class GuideStep
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public GuideStep(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}