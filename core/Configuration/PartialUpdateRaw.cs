using MediatR;

namespace EfConfigurationProvider
{
    public class PartialUpdateRaw<T> : IRequest
    {
        public string ConfigurationId { get; set; }
        public T Value { get; set; }
    }
}
