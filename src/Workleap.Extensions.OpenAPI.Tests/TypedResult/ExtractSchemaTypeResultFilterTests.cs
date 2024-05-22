using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Workleap.Extensions.OpenAPI.TypedResult;

namespace Workleap.Extensions.OpenAPI.Tests.TypedResult;

public class ExtractSchemaTypeResultFilterTests
{
    public static IEnumerable<object[]> GetResponseMetadataData()
    {
        // Synchronous results
        yield return new object[] { typeof(Ok<TestTypedSchema>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.OK, typeof(TestTypedSchema)) } };
        yield return new object[] { typeof(Accepted<TestTypedSchema>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.Accepted, typeof(TestTypedSchema)) } };
        yield return new object[] { typeof(Results<Ok<TestTypedSchema>, Accepted<TestTypedSchema>, BadRequest<ProblemDetails>, NotFound>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.OK, typeof(TestTypedSchema)), new((int)HttpStatusCode.Accepted, typeof(TestTypedSchema)), new((int)HttpStatusCode.BadRequest, typeof(ProblemDetails)), new((int)HttpStatusCode.NotFound, null) } };
        yield return new object[] { typeof(Ok), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.OK, null) } };

        // // Asynchronous results
        yield return new object[] { typeof(Task<Ok<TestTypedSchema>>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.OK, typeof(TestTypedSchema)) } };
        yield return new object[] { typeof(Task<Results<Ok<TestTypedSchema>, BadRequest<ProblemDetails>, NotFound>>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.OK, typeof(TestTypedSchema)), new((int)HttpStatusCode.BadRequest, typeof(ProblemDetails)), new((int)HttpStatusCode.NotFound, null) } };
        yield return new object[] { typeof(Task<Ok>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { new((int)HttpStatusCode.OK, null) } };

        // Common types we should ignore
        yield return new object[] { typeof(IResult), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { } };
        yield return new object[] { typeof(IActionResult), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { } };
        yield return new object[] { typeof(ActionResult<TestTypedSchema>), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { } };
        yield return new object[] { typeof(TestTypedSchema), new List<ExtractSchemaTypeResultFilter.ResponseMetadata> { } };
    }

    public static IEnumerable<object[]> GetAttributesData()
    {
        var methodsAndExpectedCodes = new Dictionary<string, HashSet<int>>
        {
            { "WithOneProducesResponse", [200] },
            { "WithTwoProducesResponse", [200, 404] },
            { "WithProducesResponseWithType", [200, 404] },
            { "WithProducesResponseWithSwaggerResponse", [200, 400, 404] }
        };

        foreach (var pair in methodsAndExpectedCodes)
        {
            var attributes = typeof(AnnotationTestClass).GetMethod(pair.Key)?.CustomAttributes;
            if (attributes != null)
            {
                yield return new object[] { attributes, pair.Value };
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetResponseMetadataData))]
    internal void GetResponsesMetadata_ReturnsCorrectMetadata_ForTypedResultControllerMethods(Type returnType, IList<ExtractSchemaTypeResultFilter.ResponseMetadata> expectedMetadata)
    {
        // Act
        var responsesMetadata = ExtractSchemaTypeResultFilter.GetResponsesMetadata(returnType).ToList();

        // Assert
        Assert.Equal(expectedMetadata.Count, responsesMetadata.Count);
        foreach (var (expected, actual) in expectedMetadata.Zip(responsesMetadata))
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.HttpCode, actual.HttpCode);
            Assert.Equal(expected.SchemaType, actual.SchemaType);
        }
    }

    [Theory]
    [MemberData(nameof(GetAttributesData))]
    internal void ExtractResponseCodesFromAttributes_ReturnsCorrectHashSet(IEnumerable<CustomAttributeData> endpointAttributes, HashSet<int> expectedResponseCodes)
    {
        // Act
        var actualResponsesCodes = ExtractSchemaTypeResultFilter.ExtractResponseCodesFromAttributes(endpointAttributes);

        // Assert
        Assert.Equal(actualResponsesCodes, expectedResponseCodes);
    }

    private sealed class TestTypedSchema
    {
        public int Count { get; set; }
    }

    private sealed class AnnotationTestClass
    {
        [ProducesResponseType(StatusCodes.Status200OK)]
        public void WithOneProducesResponse()
        {
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public void WithTwoProducesResponse()
        {
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public void WithProducesResponseWithType()
        {
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public void WithProducesResponseWithSwaggerResponse()
        {
        }
    }
}