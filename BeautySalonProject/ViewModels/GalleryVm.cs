namespace BeautySalonProject.ViewModels
{
    public class GalleryVm
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Description { get; set; } = "";
        public string Folder { get; set; } = "";
        public List<string> Images { get; set; } = new();
    }
}
