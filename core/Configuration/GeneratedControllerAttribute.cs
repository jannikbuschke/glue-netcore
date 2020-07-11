using System;

namespace EfConfigurationProvider.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GeneratedControllerAttribute : Attribute, IPartialConfiguration
    {
        public GeneratedControllerAttribute(string routePath)
        {
            Path = routePath;
        }
        public string SectionId { get; set; }
        public string Path { get; private set; }
        public string Route
        {
            get
            {
                return $"api/configurations/{Path}";
            }
        }

        public string Title { get; set; }
        public string Id { get; set; }

        public PartialConfigurationDto ToPartialConfiguration()
        {
            return new PartialConfigurationDto { Title = Title, Route = Route, Id = Id, SectionId = SectionId };
        }
    }

    public interface IPartialConfiguration
    {
        string Route { get; }
        string Title { get; set; }
        string Id { get; }
        public string SectionId { get; set; }
    }

    public class PartialConfigurationDto : IPartialConfiguration
    {
        public string Route { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public string SectionId { get; set; }
    }
}
