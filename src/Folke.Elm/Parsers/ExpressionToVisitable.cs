using Folke.Elm.Mapping;

namespace Folke.Elm.Parsers
{
    public class ExpressionToVisitable
    {
        public ExpressionToVisitable(IMapper mapper)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; }
    }
}
