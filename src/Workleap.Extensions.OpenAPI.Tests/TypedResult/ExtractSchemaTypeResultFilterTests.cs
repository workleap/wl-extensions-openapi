using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Workleap.Extensions.OpenAPI.TypedResult;

namespace Workleap.Extensions.OpenAPI.Tests.TypedResult;

public class ExtractSchemaTypeResultFilterTests
{
    public static IEnumerable<object[]> GetData()
    {
        // Synchronous results
        yield return new object[]
        {
            typeof(Ok<TestTypedSchema>),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata>
            {
                new((int)HttpStatusCode.OK, typeof(TestTypedSchema))
            }
        };
        yield return new object[]
        {
            typeof(Results<Ok<TestTypedSchema>, BadRequest<ProblemDetails>, NotFound>), // TODO: Document from this example the output OpeanAPI document (also include Produce(json))
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata>
            {
                new((int)HttpStatusCode.OK, typeof(TestTypedSchema)),
                new((int)HttpStatusCode.BadRequest, typeof(ProblemDetails)),
                new((int)HttpStatusCode.NotFound, null)
            }
        };
        yield return new object[]
        {
            typeof(Ok),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata>
            {
                new((int)HttpStatusCode.OK, null)
            }
        };

        // // Asynchronous results
        yield return new object[]
        {
            typeof(Task<Ok<TestTypedSchema>>),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata>
            {
                new((int)HttpStatusCode.OK, typeof(TestTypedSchema))
            }
        };
        yield return new object[]
        {
            typeof(Task<Results<Ok<TestTypedSchema>, BadRequest<ProblemDetails>, NotFound>>),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata>
            {
                new((int)HttpStatusCode.OK, typeof(TestTypedSchema)),
                new((int)HttpStatusCode.BadRequest, typeof(ProblemDetails)),
                new((int)HttpStatusCode.NotFound, null)
            }
        };
        yield return new object[]
        {
            typeof(Task<Ok>),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata>
            {
                new((int)HttpStatusCode.OK, null)
            }
        };

        // Common types we should ignore
        yield return new object[]
        {
            typeof(IResult),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata> {}
        };
        yield return new object[]
        {
            typeof(IActionResult),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata> {}
        };
        yield return new object[]
        {
            typeof(ActionResult<TestTypedSchema>),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata> {}
        };
        yield return new object[]
        {
            typeof(TestTypedSchema),
            new List<ExtractSchemaTypeResultFilter.ResponseMetadata> {}
        };
    }

    [Theory]
    [MemberData(nameof(GetData))]
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

    private sealed class TestTypedSchema
    {
        public int Count { get; set; }
    }
}