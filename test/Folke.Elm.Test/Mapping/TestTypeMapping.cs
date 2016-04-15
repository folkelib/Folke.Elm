using System.ComponentModel.DataAnnotations;
using System.Linq;
using Folke.Elm.Mapping;
using Moq;
using Xunit;

namespace Folke.Elm.Test.Mapping
{
    [Collection("IntegrationTest")]
    public class TestTypeMapping
    {
        private readonly Mock<IMapper> mapperMock;

        public TestTypeMapping()
        {
            mapperMock = new Mock<IMapper>();
        }

        [Fact]
        public void TypeMapping_AutoMap_TypeWithGenericProperties()
        {
            // Arrange
            var referencedTypeMapping = new TypeMapping(typeof(GenericClass<string>));
            mapperMock.Setup(x => x.GetTypeMapping(typeof(GenericClass<string>))).Returns(referencedTypeMapping);
            mapperMock.Setup(x => x.IsMapped(typeof(GenericClass<string>))).Returns(true);

            var typeMapping = new TypeMapping(typeof(TypeWithGenericProperties<string>));

            // Act
            typeMapping.AutoMap(mapperMock.Object);
            
            // Assert
            Assert.True(typeMapping.Columns.Any(x => x.Key == "Id"));
            Assert.True(typeMapping.Columns.Any(x => x.Key == "GenericProperty"));
            Assert.True(typeMapping.Columns.Any(x => x.Value.ColumnName == "GenericProperty_id"));
        }

        public class GenericClass<TKey>
        {
            [Key]
            public TKey Id { get; set; }
        }

        public class TypeWithGenericProperties<TKey>
        {
            public int Id { get; set; }

            public GenericClass<TKey> GenericProperty { get; set; }
        }
    }
}
